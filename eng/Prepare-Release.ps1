[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Version,
    [Parameter(Mandatory)] [string] $Commit,
    [Parameter(Mandatory)] [string] $PackageDirectory,
    [switch] $Requested
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module "$PSScriptRoot/ReleaseGuards.psm1" -Force
Assert-ReleaseContext $env:GITHUB_EVENT_NAME $env:GITHUB_REF $Requested.IsPresent $env:NUGET_PUBLISHING_ENABLED $env:APPVEYOR_PUBLISHING_DISABLED
if (Test-Path -LiteralPath (Join-Path (Split-Path $PSScriptRoot -Parent) 'appveyor.yml')) { throw 'Legacy AppVeyor configuration still exists.' }
& "$PSScriptRoot/Package-Provenance.ps1" -Mode Verify -Version $Version -Commit $Commit -PackageDirectory $PackageDirectory
& git fetch origin '+refs/heads/main:refs/remotes/origin/main' --tags
if ($LASTEXITCODE -ne 0) { throw 'Refreshing release refs failed.' }
$headCommit = (& git rev-parse HEAD | Out-String).Trim()
$mainCommit = (& git rev-parse origin/main | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or $headCommit -ne $Commit -or $mainCommit -ne $Commit) { throw 'Release must identify the tested current main commit.' }
& dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'GitVersion restore failed.' }
$raw = & dotnet tool run dotnet-gitversion -- /output json /nofetch /nonormalize
if ($LASTEXITCODE -ne 0) { throw 'Release version recalculation failed.' }
$calculated = ($raw -join "`n") | ConvertFrom-Json
if ($calculated.SemVer -ne $Version -or $calculated.Sha -ne $Commit) { throw 'Release version or commit changed after testing.' }
$tag = "v$Version"
$tagCommit = $null
$remote = @(& git ls-remote --tags origin "refs/tags/$tag" "refs/tags/$tag^{}")
if ($LASTEXITCODE -ne 0) { throw 'Reading remote release tag failed.' }
if ($remote.Count -gt 0) {
    $peeled = @($remote | Where-Object { $_ -like "*refs/tags/$tag^{}" })
    $tagCommit = (($peeled + $remote)[0] -split '\s+')[0]
}
foreach ($id in @('streamistry', 'streamistry.json', 'streamistry.rabbitmq')) {
    # Network errors fail closed. The version baseline must be checked for every package.
    $published = Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/$id/index.json"
    Assert-ReleaseVersion -Version $Version -Published $published.versions -ExistingTagCommit $tagCommit -Commit $Commit
}
if (-not $tagCommit) {
    & git -c user.name='github-actions[bot]' -c user.email='41898282+github-actions[bot]@users.noreply.github.com' tag --annotate $tag --message "Streamistry $Version" $Commit
    if ($LASTEXITCODE -ne 0) { throw 'Release tag creation failed.' }
    & git push origin "refs/tags/$tag"
    if ($LASTEXITCODE -ne 0) { throw 'Release tag reservation failed; inspect the remote before retrying.' }
}
Write-Output "Release identity $tag reserved for tested commit $Commit."
