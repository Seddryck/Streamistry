[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $PackageDirectory,
    [Parameter(Mandatory)] [string] $Version
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression.FileSystem

$packageIds = @('Streamistry', 'Streamistry.Json', 'Streamistry.RabbitMQ')
$frameworks = @('net8.0', 'net9.0', 'net10.0')
$files = @(Get-ChildItem -LiteralPath $PackageDirectory -File | Where-Object { $_.Extension -in '.nupkg', '.snupkg' })
if ($files.Count -ne 6) { throw "Expected three packages and three symbol packages; found $($files.Count)." }

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
Write-Output "Validated all three package identities, version $Version, framework assemblies, symbols, readmes, and icons."
