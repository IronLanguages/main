<#
.Description
Invoke-CmdScript.ps1 runs a batch script and adjusts the environment,
current working directory and LastExitCode to match what the command script output.

.SYNOPSIS
Use this program to run .bat/.cmd scripts which modify the environment,
and automatically propagate the environment to the calling powershell
session.

.PARAMETER Command
The .bat/.cmd script to run.

.PARAMETER Arguments
The arguments to provide to the -Command.  The remaining unbound arguments
to Invoke-CmdScript will be assigned to this Argument.

.PARAMETER Separator
This string is used to separate the output of -Command and the environment
settings that are parsed by Invoke-CmdScript.  You may need to provide this
parameter if the default -Separator matches some output of your -Command.

.PARAMETER UseTempBatFile
This forces a temporary bat file in $env:TMP or $env:TEMP to be used to
run $Command, rather than running it from the command line.  This option is
forces if delayed environment variable expansion is not enabled in the command
processor (required for ERRORLEVEL and the current working diretory to be set
correctly without delayed variable expansion).

.NOTES
Some attempt is made to escape special characters in your Arguments.  If
quoting fails, please examine filter:Quote-InvokedArguments in this script.

.EXAMPLE
Invoke-CmdScript vcenvvars.bat
Setup a build window for Visual Studio.

#>

param(
[switch]$WhatIf,
[parameter(Position=0,Mandatory=$true)][string]$Command,
[parameter()][string]$Separator = "==############### Invoke-CmdScript.ps1 SEPARATOR ###############",
[parameter(ValueFromRemainingArguments=$true)][String[]]$Arguments = @(),
[parameter()][switch]$UseTempBatFile
)

filter Quote-CmdInvokedArguments {
    process {
            if ($_ -match ' ') {
                    $_ = '"' + ($_ -replace '(\\*)"','$1$1\"') + '"'
            }
            $_
    }
}

filter Quote-PSInvokedArguments {
    begin {
        # If a command name contains any of these chars, it needs to be quoted
        $_charsRequiringQuotes = ('`&@''#{}()$,;|<> ' + "`t").ToCharArray()
    }
    process {
        # If the command contains non-word characters (space, ) ] ; ) etc.)
        # then it needs to be quoted
        if ($_.IndexOfAny($_charsRequiringQuotes) -eq -1) {
            $_
        } elseif ($_.IndexOf('''') -ge 0) {
            '"{0}"' -f $_.Replace('"','`"').Replace('$','`$').Replace('`','``') #'#
        } else {
            '"{0}"' -f $_
        }
    }
}

# WARNING: ensure this string is unique in the environment, and the new environment,
#          and any output from the called script
$Count = 0
$NewEnvironment = @{}

$ComSpec = if (test-path env:ComSpec) { $env:ComSpec } else { "cmd.exe" }

if (-not $UseTempBatFile)
{
        $out = & $ComSpec '/c' "echo !CD!"
        if ($out -eq '!CD!')
        {
                Write-Warning "Warning: delayed environment variable expansion not enabled in $ComSpec, forcing temporary bat file usage.  See $ComSpec /? for more details"
                $UseTempBatFile = $true
        }
}

if ($UseTempBatFile)
{
        if (-not $TempBatFilePath)
        {
                'TMP','TEMP' |% { if (test-path (Get-Content "env:$_")) { $Temp = Get-Content "env:$_" } }
                if (test-path $Temp) {
                        $i = 0;
                        do {
                                $TempBatFilePath = Join-Path $Temp "Invoke-CmdScript_$i.bat"
                                ++$i;
                        } while (Test-Path $TempBatFilePath)
                }
        }

        if (-not $TempBatFilePath)
        {
                Write-Error "Could not find a location for a temporary bat file\n"
        }
}

if ($TempBatFilePath)
{
        #Prepare the arguments for execution by escaping special characters
        $Arguments = @($Arguments | Quote-CmdInvokedArguments)
        $Command = @($Command | Quote-CmdInvokedArguments)
        $NewArguments = '/c',@($tempbatfilepath | Quote-PSInvokedArguments)

        "@echo off",`
            "call $Command ${Arguments}",`
            "echo",`
            "echo ${Separator}",`
            "echo ERRORLEVEL=%ERRORLEVEL%",`
            "echo CD=%CD%",`
            "set" |
            Set-Content $tempbatfilepath
}
else
{
    #Prepare the arguments for execution by escaping special characters
    $Arguments = @($Arguments | Quote-PSInvokedArguments)
    $Command = @($Command | Quote-PSInvokedArguments)
    $ExtraCmdString = "&echo ${Separator}&echo ERRORLEVEL=!ERRORLEVEL!&echo CD=!CD!&set"
    $NewArguments = '/S','/V:ON','/C',"`"$Command ${Arguments}${ExtraCmdString}`""
}

#Execute the script
& $ComSpec @NewArguments | Out-String -stream |% `
{
    if ($_ -ceq $Separator)
    {
	    $Count += 1
    }
    elseif ($Count -eq 0)
    {
        write-host $_
    }
    else
    {
        $pos = $_.IndexOf("=")
        if ( $pos -ge 0)
        {
	        $Name = $_.SubString(0,$pos)
		    $Value = $_.SubString($pos+1)
            if ($Count -eq 1)
            {
			    $NewEnvironment[$Name] = $Value
	        }
            else
            {
	            Write-Error "Too many separators in output, environment unmodified"
	            return
	        }
        }
        else
        {
	        Write-Error "Could not parse line '$_'"
        }
    }
}

if ($UseTempBatFile -and (Test-Path $TempBatFilePath))
{
        Remove-Item $TempBatFilePath
}

if ($Count -ne 1)
{
    Write-Error "No separator line encounted, cmdscript exited with error"
    exit $LastExitCode
}

Get-ChildItem env:* |% `
{
    $Name = $_.Name
    if (-not $NewEnvironment.ContainsKey($Name))
    {
        Write-Verbose "DEL : $Name"
	    Remove-Item "env:$Name" -WhatIf:$WhatIf
    }
} >> $null

$MyLastExitCode = $null
$NewEnvironment.Keys | Sort-Object |% `
{
    $Name = $_
    $Value = $NewEnvironment[$Name]
    $Path = "env:$Name"
    $Action = $null

    if ('DATE','TIME','RANDOM','CMDEXTVERSION','CMDCMDLINE' -contains $Name)
    {
        #Special environment variables that may have been echoed
    }
    elseif ($Name -ceq "ERRORLEVEL") # Special environment variable
    {
        $MyLastExitCode = $Value
    }
    elseif ($Name -ceq "CD") # Special environment variable
    {
        if ((Get-Location).Path -cne $Value)
        {
            Write-Verbose "CD  : $Value"
            if ($WhatIf) #Set-Location doesn't support -WhatIf
            {
                Write-Host "What if: Performing operation `"Set Location`" on LiteralPath `"$Value`"."
            }
            else
            {
                Set-Location -LiteralPath $Value
            }
        }
    }
    elseif (test-path $Path)
    {
		if (-not ((Get-Content $Path) -ceq $Value))
        {
            $Action = "EDIT";
	        Remove-Item $Path -WhatIf:$WhatIf
        }
	}
    else
    {
        $Action = "NEW ";
	}

    if ($Action)
    {
        Write-Verbose "${Action}: $Name=$Value"
        if (-not (test-path $Path)) {
	        New-Item "env:$Name" -Value $Value -WhatIf:$WhatIf
        }
    }
} >> $null

if ($MyLastExitCode -ne $null)
{
    Write-Verbose "`$LastExitCode=$MyLastExitCode"
    if ($WhatIf)
    {
        Write-Host "What if: Performing operation `"Set LastExitCode`" to Value `"$MyLastExitCode`"."
    }
    else
    {
        exit $MyLastExitCode
    }
}
