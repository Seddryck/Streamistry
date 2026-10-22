[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Unit', 'Integration', 'LinuxSmoke', 'Package')]
    [string] $Task,
    [ValidateSet('net8.0', 'net9.0', 'net10.0')]
    [string] $Framework = 'net10.0',
    [ValidatePattern('^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$')]
    [string] $Version = '0.0.0-ci.local'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($args[0]) failed with exit code $LASTEXITCODE."
    }
}

function Build-TestProject([string] $Project) {
    Invoke-DotNet build $Project --configuration Release --framework $Framework --no-restore `
        "-p:Version=$Version" '-p:ContinuousIntegrationBuild=true'
}

Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    Invoke-DotNet restore Streamistry.sln
    $resultsRoot = Join-Path (Get-Location) "artifacts/test-results/$Task/$Framework"

    switch ($Task) {
        'Unit' {
            foreach ($name in @('Streamistry', 'Streamistry.Json')) {
                $project = "$name.Testing"
                Build-TestProject $project
                $results = Join-Path $resultsRoot $name
                Invoke-DotNet test $project --configuration Release --framework $Framework --no-build --no-restore `
                    --logger 'trx;LogFileName=tests.trx' --results-directory $results `
                    '-p:CollectCoverage=true' '-p:CoverletOutputFormat=opencover' "-p:Include=[$name]*" `
                    '-p:Threshold=10' '-p:ThresholdType=line' "-p:CoverletOutput=$results/coverage.xml"
            }
        }
        'Integration' {
            Build-TestProject Streamistry.RabbitMQ.Testing
            Invoke-DotNet test Streamistry.RabbitMQ.Testing --configuration Release --framework $Framework `
                --no-build --no-restore --filter 'TestCategory=RabbitMQ' `
                --logger 'trx;LogFileName=tests.trx' --results-directory $resultsRoot
        }
        'LinuxSmoke' {
            Build-TestProject Streamistry.Testing
            Invoke-DotNet test Streamistry.Testing --configuration Release --framework $Framework `
                --no-build --no-restore --filter 'FullyQualifiedName~GlobbingSourceTests' `
                --logger 'trx;LogFileName=tests.trx' --results-directory $resultsRoot
        }
        'Package' {
            Invoke-DotNet build Streamistry.sln --configuration Release --no-restore `
                "-p:Version=$Version" '-p:ContinuousIntegrationBuild=true'
            $packages = Join-Path (Get-Location) 'artifacts/packages'
            New-Item -ItemType Directory -Path $packages -Force | Out-Null
            foreach ($project in @('Streamistry.Core', 'Streamistry.Json', 'Streamistry.RabbitMQ')) {
                Invoke-DotNet pack $project --configuration Release --no-build --no-restore `
                    --include-symbols --output $packages "-p:Version=$Version"
            }
            & "$PSScriptRoot/Test-Packages.ps1" -PackageDirectory $packages -Version $Version
        }
    }
}
finally {
    Pop-Location
}
