[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('Create', 'Verify')] [string] $Mode,
    [Parameter(Mandatory)] [string] $Version,
    [Parameter(Mandatory)] [string] $Commit,
    [Parameter(Mandatory)] [string] $PackageDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName System.IO.Compression.FileSystem
$provenancePath = Join-Path $PackageDirectory 'provenance.json'
& "$PSScriptRoot/Test-Packages.ps1" -PackageDirectory $PackageDirectory -Version $Version
if ($Mode -eq 'Create') {
    $root = Split-Path $PSScriptRoot -Parent
    $build = Get-Content -LiteralPath (Join-Path $root 'artifacts/build/manifest.json') -Raw | ConvertFrom-Json
    if ($build.commit -ne $Commit -or $build.version -ne $Version) { throw 'Package build identity mismatch.' }
    $assets = @(
        foreach ($id in @('Streamistry', 'Streamistry.Json', 'Streamistry.RabbitMQ')) {
            $project = if ($id -eq 'Streamistry') { 'Streamistry.Core' } else { $id }
            foreach ($framework in @('net8.0', 'net9.0', 'net10.0')) {
                foreach ($extension in @('dll', 'pdb')) {
                    $path = "$project/bin/Release/$framework/$id.$extension"
                    $file = @($build.files | Where-Object path -eq $path)
                    if ($file.Count -ne 1) { throw "Missing build provenance for $path." }
                    [ordered]@{ id = $id; entry = "lib/$framework/$id.$extension"; extension = $extension; sha256 = $file[0].sha256 }
                }
            }
        }
    )
    $packages = @(Get-ChildItem -LiteralPath $PackageDirectory -File | Where-Object Extension -in '.nupkg', '.snupkg' | ForEach-Object {
        [ordered]@{ name = $_.Name; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    [ordered]@{ commit = $Commit; version = $Version; packages = $packages; assets = $assets } | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $provenancePath -Encoding utf8
}
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
if ($provenance.commit -ne $Commit -or $provenance.version -ne $Version -or @($provenance.packages).Count -ne 6 -or @($provenance.assets).Count -ne 18) { throw 'Package provenance identity/count mismatch.' }
foreach ($package in $provenance.packages) {
    $path = Join-Path $PackageDirectory $package.name
    if ([IO.Path]::GetFileName($package.name) -ne $package.name -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $package.sha256) { throw 'Package hash mismatch.' }
}
foreach ($asset in $provenance.assets) {
    $extension = if ($asset.extension -eq 'dll') { 'nupkg' } else { 'snupkg' }
    $archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $PackageDirectory "$($asset.id).$Version.$extension"))
    try {
        $stream = $archive.GetEntry($asset.entry).Open()
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) }
        finally { $stream.Dispose() }
        if ($hash -ne $asset.sha256) { throw "Packaged asset differs from tested build: $($asset.entry)" }
    }
    finally { $archive.Dispose() }
}
Write-Output "Package provenance verified for $Commit / $Version."
