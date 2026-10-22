[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('Create', 'Verify', 'Save', 'Restore')] [string] $Mode,
    [Parameter(Mandatory)] [string] $Version,
    [Parameter(Mandatory)] [string] $Commit,
    [string] $Root = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projects = @('Streamistry.Core', 'Streamistry.Json', 'Streamistry.RabbitMQ', 'Streamistry.SourceGenerator', 'Streamistry.Testing', 'Streamistry.Json.Testing', 'Streamistry.RabbitMQ.Testing')
$manifestPath = Join-Path $Root 'artifacts/build/manifest.json'
$archivePath = Join-Path $Root 'artifacts/build/build.tar.gz'
Push-Location $Root
try {
    if ($Mode -eq 'Restore') {
        & tar -xzf $archivePath -C $Root
        if ($LASTEXITCODE -ne 0) { throw 'Build archive extraction failed.' }
    }
    if ($Mode -eq 'Create') {
        $files = @(
            foreach ($project in $projects) {
                foreach ($file in Get-ChildItem -LiteralPath "$project/bin/Release" -File -Recurse) {
                    [ordered]@{ path = [IO.Path]::GetRelativePath($Root, $file.FullName).Replace('\', '/'); sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
                }
            }
        )
        if ($files.Count -eq 0) { throw 'No build outputs found.' }
        New-Item -ItemType Directory -Path (Split-Path $manifestPath) -Force | Out-Null
        [ordered]@{ commit = $Commit; version = $Version; files = $files } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifestPath -Encoding utf8
    }
    else {
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.commit -ne $Commit -or $manifest.version -ne $Version) { throw 'Build artifact commit/version mismatch.' }
        foreach ($file in $manifest.files) {
            $fullPath = [IO.Path]::GetFullPath((Join-Path $Root $file.path))
            $prefix = [IO.Path]::GetFullPath($Root).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
            if (-not $fullPath.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid build artifact path.' }
            if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf) -or (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash -ne $file.sha256) { throw "Build output changed or is missing: $($file.path)" }
        }
    }
    if ($Mode -eq 'Save') {
        $paths = @('artifacts/build/manifest.json')
        foreach ($project in $projects) { $paths += "$project/bin", "$project/obj" }
        & tar -czf $archivePath @paths
        if ($LASTEXITCODE -ne 0) { throw 'Build archive creation failed.' }
    }
    Write-Output "Build artifact $Mode validated for $Commit / $Version."
}
finally { Pop-Location }
