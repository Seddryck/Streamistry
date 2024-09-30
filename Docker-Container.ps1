Function Deploy-Container {
	[CmdletBinding()]
	Param(
		[Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
		[string] $fullname,

		[Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]        
		[string] $nickname = $fullname,

		[Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]        
		[string] $filePath = $null,
		
		[Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]        
		[string[]] $arguments = @(),

		[Parameter(Mandatory=$false)]
		[ScriptBlock] $scriptBlock = $null
	)

	Begin {
		$previously_running = $false
		if ($null -eq $filePath -or $filePath -eq "") {
			$filePath = ".\Streamistry.RabbitMQ.Testing\run-$fullname-docker.cmd"
		}
	}

	Process {
		$running = & docker container ls --format "{{.ID}}" --filter "name=$nickname"
		if ($null -ne $running) {
			$previously_running = $true
			Write-Host "`tContainer for '$fullname' is already running with ID '$running'."
		} else {
			Write-Host "`tStarting new container"
			Start-Process -FilePath $filePath -ArgumentList $arguments
			do {
				$running = & docker container ls --format "{{.ID}}" --filter "name=$nickname"
				if ($null -eq $running) {
					Start-Sleep -s 1
				}
			} while($null -eq $running)
			Write-Host "`tContainer for '$fullname' started with ID '$running'."

			if ($null -ne $scriptBlock) {
				$startWait = Get-Date
				do {
					$isRunning = Invoke-Command -ScriptBlock $scriptBlock
					$wait = New-TimeSpan -Start $startWait
					if (!$isRunning) {
						if ($wait -gt (New-TimeSpan -Seconds 1)) {
							Write-Host "`t`tWaiting since $($wait.ToString("ss")) seconds ..."
						}
						Start-Sleep -s 1
					}
				} while (!$isRunning -and !($wait -gt (New-TimeSpan -Seconds 60)))
				if (!$isRunning) {
					Write-Warning "Waited during $($wait.ToString("ss")) seconds. Stopping test harness."
					exit 0
				} else {
					Write-Host "`tServer is available: waited $($wait.ToString("ss")) seconds to get it live."
				}
			}
		}
    }

    End {
        return $previously_running, $running
    }
}

Function Remove-Container {
	[CmdletBinding()]
	Param(
		[Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
		[string] $container
	)

	Begin {
		 if ($null -eq $container) {
			 exit
		 }
	}

	Process {
		Write-Host "`tForcefully removing container '$container' ..."
		& docker rm --force $container | Out-Null
		Write-Host "`tContainer removed."
    }

    End { 
    }
}

Function Connect-Network {
	[CmdletBinding()]
	Param(
		[Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
		[string] $container,

		[Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
		[string] $network
	)

	Begin {
		 if ($null -eq $container) {
			 exit
		 }
	}

	Process {
		if ($containers.Contains($container)) {
			Write-Host "`tContainer '$container' already connected to network '$network'."
		} else {
			Write-Host "`tConnecting container '$container' to network '$network' ..."
			& docker network connect $network $container
			Write-Host "`tContainer '$container' connected to network '$network'."
		}
	}

	End { 
    }
}