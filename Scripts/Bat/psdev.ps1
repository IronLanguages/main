function ConvertFrom-CMDString {
  <#
    .Synopsis
      Converts strings from CMD style (%env%) to Powershell style ($env:env)
        
    .Parameter String
      CMD formatted string containing variables in the format %fubar%
                  
    .ReturnValue
      Converted string with $env: syntax
            
    .Notes
       NAME:      ConvertFrom-CMDString
       AUTHOR:    REDMOND\tbert and REDMOND\jdeville
       LASTEDIT:  6/14/2010       
       Requires -Version 2.0
  #>
  [CmdletBinding(SupportsShouldProcess=$False,
                 SupportsTransactions=$False, 
                 ConfirmImpact="None",
                 DefaultParameterSetName="")]
  param (
    [parameter(mandatory=$true,valuefrompipeline=$true)]
    [ValidateNotNullorEmpty()]
    [String]
    $String
  )
    
  process {
    $string = $string.Replace("'", "```'").Replace("cd /d ", "cd ")
    $string = $string -replace '\$(\d)', "`'+`$args[`$1]+`'"
    $string = $string -replace '\$\*', "`'+`$args+`'"

    $string = $string -replace '\$T', '; & '
    $string = $string -replace '%(.*?)%', "`'+`$ENV:`$1+`'"
    return $string
  }
}

function Convert-Doskey {
  <#
    .Synopsis
      Registers doskey functions equivalent to CMD counterparts
            
    .Parameter AliasFile
      alias file to process
    
    .Example
      (join-path $env:INIT "cue.pri"),
      (join-path $env:INIT "dbg.pub"),
      (join-path $env:INIT "cue.pub")  | ? { test-path "$_" } | Get-Item | Convert-Doskey | import-module

    .Notes
      NAME:      Convert-Doskey
      AUTHOR:    REDMOND\jdeville
      LASTEDIT:  6/14/2010
      Requires -Version 2.0
  #>

  [CmdletBinding(SupportsShouldProcess=$False,
                 SupportsTransactions=$False, 
                 ConfirmImpact="None",
                 DefaultParameterSetName="")]
                 
  param (
    [parameter(mandatory=$true,valuefrompipeline=$true)]
    [ValidateNotNullorEmpty()]
    [system.io.fileinfo] $AliasFile
  )
    
  begin { $slash = [System.IO.Path]::DirectorySeparatorChar; }
    
  process { 
    $moduleScript = ""
    if (Test-Path $AliasFile) {
      $Aliases = Get-Content $AliasFile | ? { ( $_ -and $_.trim() -and ($_ -notmatch "^(#|;)") ) }
  
      foreach ($Alias in $Aliases) {
        if($alias -match "^(?<name>[^=\s]+)\s*=(?<macro>.+)$") {
          Write-Verbose "Processing $alias"
          $cmd = $matches['macro']
          $name = $matches['name']

          $cmd = ConvertFrom-CMDString $cmd
                 
          $cmd = "invoke-expression(`'& " + $cmd + "`')"

          $cmd = $cmd.Replace("+`'`'", '')


        } else {
          throw "not a valid doskey macro $alias"
        }

        write-verbose "Path: function:$name"
        write-verbose "Script: $cmd"

        $textblock = "`${function:$name} = { $cmd }"
        $moduleScript += "`n$textBlock"
        trap {
          Write-Debug ($_ | Out-String)
          continue
        }
      }
    } else { throw "Alias file not found: $AliasFile" }
    $moduleScript += "`nExport-moduleMember -Alias * -Function * `n"
    write-verbose "Module: $moduleScript"
    $scriptBlock = [scriptblock]::Create($moduleScript)
    new-module -ScriptBlock $scriptBlock
  }
}

push-location (split-path $MyInvocation.MyCommand.Definition)
. $PWD\invoke-cmdscript.ps1 Dev.bat $args

(resolve-path "./alias.txt"),(resolve-path "./aliasInternal.txt") | 
? { test-path "$_" } |
get-item |
convert-doskey |
import-module

if (test-path "$env:DLR_ROOT%\..\Users\$env:USERNAME\psdev.ps1") {
  . "$env:DLR_ROOT%\..\Users\$env:USERNAME\psdev.ps1"
}
pop-location
