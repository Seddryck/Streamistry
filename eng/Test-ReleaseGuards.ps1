$ErrorActionPreference = 'Stop'
Import-Module "$PSScriptRoot/ReleaseGuards.psm1" -Force
$commit = 'a' * 40
function Assert-Rejected([string] $Name, [scriptblock] $Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (-not $rejected) { throw "Unsafe release case accepted: $Name" }
    Write-Output "Rejected: $Name"
}
Assert-ReleaseContext -EventName workflow_dispatch -Ref refs/heads/main -Requested $true -Enabled true -LegacyDisabled true
Assert-Rejected 'PR publication' { Assert-ReleaseContext pull_request refs/heads/main $true true true }
Assert-Rejected 'ordinary push publication' { Assert-ReleaseContext push refs/heads/main $true true true }
Assert-Rejected 'feature branch publication' { Assert-ReleaseContext workflow_dispatch refs/heads/feat/example $true true true }
Assert-Rejected 'unrequested publication' { Assert-ReleaseContext workflow_dispatch refs/heads/main $false true true }
Assert-Rejected 'disabled publishing' { Assert-ReleaseContext workflow_dispatch refs/heads/main $true false true }
Assert-Rejected 'legacy publisher not retired' { Assert-ReleaseContext workflow_dispatch refs/heads/main $true true false }
Assert-Rejected 'version regression' { Assert-ReleaseVersion 0.39.1 @('0.39.52') $null $commit }
Assert-Rejected 'duplicate without tag' { Assert-ReleaseVersion 0.40.0 @('0.40.0') $null $commit }
Assert-Rejected 'tag points elsewhere' { Assert-ReleaseVersion 0.40.0 @('0.39.52') ('b' * 40) $commit }
Assert-Rejected 'prerelease publication' { Assert-ReleaseVersion '0.40.0-beta.1' @() $null $commit }
Assert-ReleaseVersion 0.40.0 @('0.39.52') $null $commit
Assert-ReleaseVersion 0.40.0 @('0.40.0') $commit $commit
$mapping = @{ ci = 'build'; feat = 'new-feature'; fix = 'bug'; docs = 'docs'; test = 'none' }
foreach ($case in @(
    @{ Title = 'ci: migrate'; Expected = 'build' },
    @{ Title = 'feat!: change API'; Expected = 'new-feature,breaking-change' },
    @{ Title = 'fix(parser): preserve null'; Expected = 'bug' },
    @{ Title = 'test: verify'; Expected = '' },
    @{ Title = 'A normal title'; Expected = '' }
)) {
    if ((@(Get-ConventionalLabels $case.Title $mapping) -join ',') -ne $case.Expected) { throw "Incorrect labels: $($case.Title)" }
}
Write-Output 'Release gates, version baseline, tag collision/retry, and changelog label checks passed.'
