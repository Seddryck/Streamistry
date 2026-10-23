$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Import-Module "$PSScriptRoot/ReleaseGuards.psm1" -Force
$root = Split-Path $PSScriptRoot -Parent
$mapping = Get-Content -LiteralPath (Join-Path $root '.github/conventional_commits_labels.json') -Raw | ConvertFrom-Json -AsHashtable
$pulls = & gh api "repos/$env:GITHUB_REPOSITORY/commits/$env:GITHUB_SHA/pulls"
if ($LASTEXITCODE -ne 0) { throw 'Reading merged pull requests failed.' }
foreach ($pull in (($pulls -join '') | ConvertFrom-Json)) {
    if (-not $pull.merged_at -or $pull.base.ref -ne 'main') { continue }
    $labels = @(Get-ConventionalLabels -Title $pull.title -Mapping $mapping)
    foreach ($label in $labels) {
        $encodedLabel = [Uri]::EscapeDataString($label)
        & gh api "repos/$env:GITHUB_REPOSITORY/labels/$encodedLabel" --silent 2>$null
        if ($LASTEXITCODE -ne 0) {
            & gh label create $label --repo $env:GITHUB_REPOSITORY --color '006b75'
            if ($LASTEXITCODE -ne 0) { throw 'Ensuring changelog label failed.' }
        }
        & gh pr edit $pull.number --repo $env:GITHUB_REPOSITORY --add-label $label
        if ($LASTEXITCODE -ne 0) { throw 'Updating changelog labels failed.' }
    }
}
