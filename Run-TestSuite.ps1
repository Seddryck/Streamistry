Function Run-TestSuite {
	[CmdletBinding()]
	Param(
		[Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string[]] $categories
		, [string] $config = "Release"
		, [string[]] $frameworks = @("net8.0")
	)

	Begin {
		$testSuccessful = 0
		if ($env:APPVEYOR -eq "True") {
			$adapters = "--test-adapter-path:.", "--logger:Appveyor"
		}
	}

	Process {
		foreach ($framework in $frameworks) {
			$buildMsg = & dotnet build "..\Streamistry.RabbitMQ.Testing" -c $config -f $framework --nologo
			if ($lastexitcode -ne 0) {
				Write-Warning "Cannot build the Test assembly! `r`n$($buildMsg -join "`r`n")"
			} else {
				foreach ($category in $categories) {
					Write-Host "`tRunning test-suite for $category ($framework)"
					$arguments  = @("test", "..\Streamistry.RabbitMQ.Testing")
					$arguments += @("--filter", "`"TestCategory=$($category.Split("+") -join "`"`"&`"`"TestCategory=")`"")
					$arguments += @("-c", $config)
					$arguments += @("-f", $framework)
					$arguments += @("--no-build", "--nologo")
					$arguments += $adapters
					& dotnet $arguments | Out-Host
					$testSuccessful += $lastexitcode
				}
			}
		}
    }

    End {
        return $testSuccessful
    }
}


