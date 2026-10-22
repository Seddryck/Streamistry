[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repository = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $repository ('artifacts/versioning-tests/' + [guid]::NewGuid().ToString('N'))
$originalGitHubActions = $env:GITHUB_ACTIONS
$originalAppVeyor = $env:APPVEYOR
$script:fixtureSequence = 0

function Invoke-FixtureGit {
    & git -C $fixture @args | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Fixture git command failed: $($args[0])" }
}

function Add-FixtureCommit([string] $Message) {
    Set-Content -LiteralPath (Join-Path $fixture 'change.txt') -Value ([guid]::NewGuid().ToString('N'))
    Invoke-FixtureGit add change.txt
    $script:fixtureSequence++
    $previousAuthorDate = $env:GIT_AUTHOR_DATE
    $previousCommitterDate = $env:GIT_COMMITTER_DATE
    try {
        $date = ([DateTimeOffset]::Parse('2025-01-01T00:00:00Z')).AddMinutes($script:fixtureSequence).ToString('o')
        $env:GIT_AUTHOR_DATE = $date
        $env:GIT_COMMITTER_DATE = $date
        Invoke-FixtureGit -c user.name='Versioning Tests' -c user.email='versioning@example.invalid' `
            commit -m $Message
    }
    finally {
        $env:GIT_AUTHOR_DATE = $previousAuthorDate
        $env:GIT_COMMITTER_DATE = $previousCommitterDate
    }
}

function Assert-FixtureVersion([string] $Scenario, [string] $Pattern) {
    $raw = & dotnet tool run dotnet-gitversion -- $fixture /output json /nocache /nonormalize
    if ($LASTEXITCODE -ne 0) { throw "GitVersion failed for $Scenario." }
    $version = (($raw -join "`n") | ConvertFrom-Json).SemVer
    if ($version -notmatch $Pattern) { throw "$Scenario produced $version; expected $Pattern." }
    Write-Output "${Scenario}: $version"
}

Push-Location $repository
try {
    # Fixtures must be evaluated from their own branches, not the enclosing CI run's PR ref.
    $env:GITHUB_ACTIONS = $null
    $env:APPVEYOR = $null
    New-Item -ItemType Directory -Path $fixture -Force | Out-Null
    Invoke-FixtureGit init --initial-branch=main
    Copy-Item -LiteralPath (Join-Path $repository 'GitVersion.yml') -Destination $fixture
    Invoke-FixtureGit add GitVersion.yml
    Add-FixtureCommit 'chore: initialize version fixture'
    Invoke-FixtureGit tag v1.2.3
    Assert-FixtureVersion 'Tagged release' '^1\.2\.3$'

    Add-FixtureCommit 'fix: correct a value'
    Assert-FixtureVersion 'Patch on main' '^1\.2\.4$'
    Add-FixtureCommit 'ci: update validation'
    Assert-FixtureVersion 'Successive untagged patch' '^1\.2\.5$'
    Add-FixtureCommit 'feat: add a value'
    Assert-FixtureVersion 'Feature on main' '^1\.3\.0$'
    Add-FixtureCommit 'ci!: change the build contract'
    Assert-FixtureVersion 'Breaking conventional commit' '^2\.0\.0$'
    Add-FixtureCommit 'docs: clarify behavior +semver: none'
    Assert-FixtureVersion 'Explicit no bump' '^2\.0\.0$'
    Add-FixtureCommit 'chore: prepare a capability +semver: minor'
    Assert-FixtureVersion 'Explicit minor bump' '^2\.1\.0$'
    Add-FixtureCommit 'docs: clarify the pipeline'
    Assert-FixtureVersion 'Default patch for other messages' '^2\.1\.1$'
    Add-FixtureCommit 'feat: defer a capability +semver: none'
    Assert-FixtureVersion 'No bump takes precedence over feature' '^2\.1\.1$'
    Add-FixtureCommit 'chore: change compatibility +semver: major'
    Assert-FixtureVersion 'Explicit major bump' '^3\.0\.0$'
    Add-FixtureCommit "ci: change compatibility`n`nBREAKING CHANGE: drop an old runner"
    Assert-FixtureVersion 'Breaking change footer' '^4\.0\.0$'
    Add-FixtureCommit 'docs: request a patch +semver: patch'
    Assert-FixtureVersion 'Explicit patch bump' '^4\.0\.1$'
    Add-FixtureCommit 'chore: defer packaging +semver: skip'
    Assert-FixtureVersion 'Explicit skip' '^4\.0\.1$'

    Invoke-FixtureGit checkout -b feat/sample v1.2.3
    Add-FixtureCommit 'feat: add a sample'
    Assert-FixtureVersion 'Feature prerelease' '^1\.3\.0-beta\.sample\.\d+$'
    Invoke-FixtureGit checkout -b fix/sample v1.2.3
    Add-FixtureCommit 'fix: correct a sample'
    Assert-FixtureVersion 'Fix prerelease' '^1\.2\.4-fix-sample\.\d+$'
    Invoke-FixtureGit checkout -b release/next v1.2.3
    Add-FixtureCommit 'chore: prepare release candidate'
    Assert-FixtureVersion 'Release candidate' '^2\.0\.0-rc\.next\.\d+$'
}
finally {
    $env:GITHUB_ACTIONS = $originalGitHubActions
    $env:APPVEYOR = $originalAppVeyor
    Pop-Location
}
