Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Assert-ReleaseContext {
    param([string] $EventName, [string] $Ref, [bool] $Requested, [string] $Enabled, [string] $LegacyDisabled)
    if ($EventName -ne 'workflow_dispatch' -or $Ref -ne 'refs/heads/main' -or -not $Requested) { throw 'Publishing requires an explicit dispatch on main.' }
    if ($Enabled -ne 'true' -or $LegacyDisabled -ne 'true') { throw 'Publishing is not enabled or AppVeyor retirement has not been confirmed.' }
}
function Assert-ReleaseVersion {
    param([string] $Version, [string[]] $Published, [AllowNull()] [string] $ExistingTagCommit, [string] $Commit)
    if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Only stable release versions may be published.' }
    if ($ExistingTagCommit -and $ExistingTagCommit -ne $Commit) { throw 'Release tag identifies a different commit.' }
    $stable = @($Published | Where-Object { $_ -match '^\d+\.\d+\.\d+$' })
    foreach ($publishedVersion in $stable) {
        if ([version]$publishedVersion -gt [version]$Version) { throw "Release $Version is below published version $publishedVersion." }
        if ([version]$publishedVersion -eq [version]$Version -and $ExistingTagCommit -ne $Commit) { throw 'An existing package version may only be retried with its reserved commit tag.' }
    }
}
function Get-ConventionalLabels {
    param([string] $Title, [hashtable] $Mapping)
    if ($Title -match '^(?<type>\w+)(?:\([^)]+\))?(?<breaking>!)?:') {
        $label = $Mapping[$Matches.type]
        if ($label -and $label -ne 'none') { $label }
        if ($Matches['breaking'] -eq '!') { 'breaking-change' }
    }
}
Export-ModuleMember -Function Assert-ReleaseContext, Assert-ReleaseVersion, Get-ConventionalLabels
