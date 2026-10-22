[CmdletBinding()]
param([string] $Version = '0.0.0-ci.local', [string] $Commit = (git rev-parse HEAD))
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
$packages = Join-Path $root 'artifacts/packages'
function Assert-Rejected([string] $Name, [scriptblock] $Action, [string] $Expected) {
    $message = $null
    try { & $Action } catch { $message = $_.Exception.Message }
    if (-not $message -or $message -notlike $Expected) { throw "Unexpected result for $($Name): $message" }
    Write-Output "Rejected: $Name"
}
& "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit $Commit
Assert-Rejected 'wrong build commit' { & "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit ('b' * 40) } '*commit/version mismatch*'
Assert-Rejected 'wrong build version' { & "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version '0.0.0-wrong' -Commit $Commit } '*commit/version mismatch*'
$manifest = Get-Content -LiteralPath (Join-Path $root 'artifacts/build/manifest.json') -Raw | ConvertFrom-Json
$binaryPath = Join-Path $root $manifest.files[0].path
$original = [IO.File]::ReadAllBytes($binaryPath)
try {
    [IO.File]::WriteAllBytes($binaryPath, [byte[]]($original + [byte]0))
    Assert-Rejected 'changed compiled bytes' { & "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit $Commit } '*Build output changed or is missing*'
}
finally { [IO.File]::WriteAllBytes($binaryPath, $original) }
& "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit $Commit -PackageDirectory $packages
Assert-Rejected 'wrong package commit' { & "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit ('b' * 40) -PackageDirectory $packages } '*provenance identity/count mismatch*'
$package = Get-ChildItem -LiteralPath $packages -Filter '*.nupkg' -File | Select-Object -First 1
$originalPackage = [IO.File]::ReadAllBytes($package.FullName)
try {
    [IO.File]::WriteAllBytes($package.FullName, [byte[]]($originalPackage + [byte]0))
    Assert-Rejected 'changed package archive' { & "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit $Commit -PackageDirectory $packages } '*Package hash mismatch*'
}
finally { [IO.File]::WriteAllBytes($package.FullName, $originalPackage) }
$provenancePath = Join-Path $packages 'provenance.json'
$originalProvenance = [IO.File]::ReadAllBytes($provenancePath)
try {
    $provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
    $provenance.assets[0].sha256 = '0' * 64
    $provenance | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $provenancePath -Encoding utf8
    Assert-Rejected 'package differs from tested build' { & "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit $Commit -PackageDirectory $packages } '*Packaged asset differs from tested build*'
}
finally { [IO.File]::WriteAllBytes($provenancePath, $originalProvenance) }
& "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit $Commit
& "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit $Commit -PackageDirectory $packages
Write-Output 'Six artifact rejection scenarios passed; original build and package bytes restored.'
