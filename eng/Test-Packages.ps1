[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $PackageDirectory,
    [Parameter(Mandatory)] [string] $Version
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression.FileSystem

$inventory = Import-PowerShellDataFile "$PSScriptRoot/Projects.psd1"
$packageIds = $inventory.Packages
$frameworks = $inventory.Frameworks
$files = @(Get-ChildItem -LiteralPath $PackageDirectory -File | Where-Object { $_.Extension -in '.nupkg', '.snupkg' })
if ($files.Count -ne ($packageIds.Count * 2)) { throw "Expected $($packageIds.Count) packages and matching symbol packages; found $($files.Count)." }

foreach ($id in $packageIds) {
    foreach ($extension in @('nupkg', 'snupkg')) {
        $path = Join-Path $PackageDirectory "$id.$Version.$extension"
        $archive = [IO.Compression.ZipFile]::OpenRead($path)
        try {
            $nuspec = $archive.GetEntry("$id.nuspec")
            if ($null -eq $nuspec) { throw "Missing nuspec in $path." }
            $reader = [IO.StreamReader]::new($nuspec.Open())
            try { [xml] $metadata = $reader.ReadToEnd() }
            finally { $reader.Dispose() }
            if ($metadata.package.metadata.id -ne $id -or $metadata.package.metadata.version -ne $Version) {
                throw "Unexpected package identity/version in $path."
            }
            if ($id -eq 'Streamistry') {
                foreach ($dependency in $metadata.SelectNodes("//*[local-name()='dependency']")) {
                    if ($dependency.id -match '^(Expressif|DubUrl|PocketCsvReader|Packata|Streamistry\.(Expressif|Json|PocketCsvReader|Packata|DubUrl))') {
                        throw 'Core package must remain independent of optional integrations.'
                    }
                }
            }
            $assetExtension = if ($extension -eq 'nupkg') { 'dll' } else { 'pdb' }
            foreach ($framework in $frameworks) {
                if ($null -eq $archive.GetEntry("lib/$framework/$id.$assetExtension")) {
                    throw "Missing $framework $assetExtension in $path."
                }
            }
            if ($extension -eq 'nupkg') {
                foreach ($asset in @('README.md', 'icon/Streamistry-icon-128.png')) {
                    if ($null -eq $archive.GetEntry($asset)) { throw "Missing $asset in $path." }
                }
            }
        }
        finally { $archive.Dispose() }
    }
}
Write-Output "Validated all $($packageIds.Count) package identities, version $Version, framework assemblies, symbols, readmes, and icons."
