Param(
	[switch] $force=$false
	, $config= "Release"
	, [string[]] $frameworks = @("net7.0", "net8.0")
)
. $PSScriptRoot\..\Run-TestSuite.ps1
. $PSScriptRoot\..\Docker-Container.ps1

$filesChanged = & git diff --name-only HEAD HEAD~1
if ($force -or ($filesChanged -like "*rabbitmq*")) {
	Write-Host "Deploying RabbitMQ testing environment"

	$previouslyRunning, $running = Deploy-Container -FullName "rabbitmq"
	if (!$previouslyRunning) {
		Start-Sleep -s 30
	}

	# Running QA tests
	Write-Host "Running QA tests related to RabbitMQ"
	$testSuccessful = Run-TestSuite @("RabbitMQ") -config $config -frameworks $frameworks

	# Removing container
    Remove-Container $running

	# Raise failing tests
	exit $testSuccessful
} else {
	return -1
}
