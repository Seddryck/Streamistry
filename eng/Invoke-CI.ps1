[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateSet('Build', 'Unit', 'Integration', 'LinuxSmoke', 'Package')] [string] $Task,
    [ValidateSet('net8.0', 'net9.0', 'net10.0')] [string] $Framework = 'net10.0',
    [ValidatePattern('^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$')] [string] $Version = '0.0.0-ci.local',
    [string] $Commit = (git rev-parse HEAD)
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($args[0]) failed with exit code $LASTEXITCODE." }
}
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $resultsRoot = Join-Path (Get-Location) "artifacts/test-results/$Task/$Framework"
    if ($Task -eq 'Build') {
        Invoke-DotNet restore Streamistry.sln
        Invoke-DotNet build Streamistry.sln --configuration Release --no-restore "-p:Version=$Version" '-p:ContinuousIntegrationBuild=true'
        & "$PSScriptRoot/Build-Artifact.ps1" -Mode Create -Version $Version -Commit $Commit
        return
    }
    & "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit $Commit
    switch ($Task) {
        'Unit' {
            foreach ($name in @('Streamistry', 'Streamistry.Json')) {
                $results = Join-Path $resultsRoot $name
                Invoke-DotNet test "$name.Testing" --configuration Release --framework $Framework --no-build --no-restore --logger 'trx;LogFileName=tests.trx' --results-directory $results '-p:CollectCoverage=true' '-p:CoverletOutputFormat=opencover' "-p:Include=[$name]*" '-p:Threshold=10' '-p:ThresholdType=line' "-p:CoverletOutput=$results/coverage.xml"
            }
        }
        'Integration' {
            $assembly = "Streamistry.RabbitMQ.Testing/bin/Release/$Framework/Streamistry.RabbitMQ.Testing.dll"
            Invoke-DotNet vstest $assembly '/TestCaseFilter:TestCategory=RabbitMQ' '/Logger:trx;LogFileName=tests.trx' "/ResultsDirectory:$resultsRoot"
        }
        'LinuxSmoke' {
            $assembly = "Streamistry.Testing/bin/Release/$Framework/Streamistry.Testing.dll"
            Invoke-DotNet vstest $assembly '/TestCaseFilter:FullyQualifiedName~GlobbingSourceTests' '/Logger:trx;LogFileName=tests.trx' "/ResultsDirectory:$resultsRoot"
        }
        'Package' {
            # Refresh machine-specific NuGet assets after restoring bin/obj; do not compile.
            Invoke-DotNet restore Streamistry.sln --force-evaluate
            $packages = Join-Path (Get-Location) 'artifacts/packages'
            New-Item -ItemType Directory -Path $packages -Force | Out-Null
            foreach ($project in @('Streamistry.Core', 'Streamistry.Json', 'Streamistry.RabbitMQ')) {
                Invoke-DotNet pack $project --configuration Release --no-build --no-restore --include-symbols --output $packages "-p:Version=$Version" '-p:ContinuousIntegrationBuild=true'
            }
            & "$PSScriptRoot/Test-Packages.ps1" -PackageDirectory $packages -Version $Version
            & "$PSScriptRoot/Package-Provenance.ps1" -Mode Create -Version $Version -Commit $Commit -PackageDirectory $packages
        }
    }
    # In particular, coverage must restore the compiled bytes before they are archived.
    & "$PSScriptRoot/Build-Artifact.ps1" -Mode Verify -Version $Version -Commit $Commit
}
finally { Pop-Location }
