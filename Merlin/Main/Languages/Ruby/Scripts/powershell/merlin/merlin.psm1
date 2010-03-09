function get-batchfile {
  param([string]$file)
  $cmd = "`"$file`" & set"
  cmd /c $cmd | Foreach-Object {
      $p, $v = $_.split('=')
      Set-Item -path env:$p -value $v
  }
}

function insert-path {
  param([string]$append)

  set-content Env:\Path ((get-content Env:\Path) + ";" + $append)
}

function set-envvar {
  param([string]$var)
  if ($var -eq "") {
    Get-ChildItem env: | Sort-Object name
  } else {
    if ($var -match "^(\S*?)\s*=\s*(.*)$") {
      Set-Item -Force -Path "env:$($matches[1])" -Value $matches[2];
    } else {
      Write-Error "ERROR Usage: VAR=VALUE"
    }
  }
}

function initialize-merlin {
  param([string]$path = "C:\vsl\",[string]$version="9.0")

  initialize-vsvars -version $version

  set-envvar TERM=
  set-envvar MERLIN_ROOT="$path\Merlin\Main"
  set-envvar MERLIN_EXTERNAL="${env:MERLIN_ROOT}\..\External.LCA_RESTRICTED"
  set-envvar RUBY18_BIN="${env:MERLIN_EXTERNAL}\Languages\Ruby\ruby-1.8.6p368\bin"
  set-envvar RUBY18_EXE="${env:RUBY18_BIN}\ruby.exe"
  set-envvar RUBY19_EXE="${env:MERLIN_EXTERNAL}\Languages\Ruby\1.9.1p0\bin\ruby.exe"
  set-envvar GEM_PATH="${env:MERLIN_EXTERNAL}\Languages\Ruby\ruby-1.8.6p368\lib\ruby\gems\1.8"
  $alias = $env:MERLIN_ROOT + '\Scripts\bat\Alias.ps1'
  $alias_internal = $env:MERLIN_ROOT + '\Scripts\bat\AliasInternal.ps1'
  if (test-path $alias) { . $alias }
  if (test-path $alias_internal) { . $alias_internal }
   
  if (Test-Path function:r) {Remove-item -Force function:r}
  insert-path $env:RUBY18_BIN
  insert-path "$env:MERLIN_ROOT\External\Tools"
  insert-path "$env:MERLIN_ROOT\Scripts\Bat"
  insert-path "$env:MERLIN_ROOT\..\Snap\bin"
  insert-path "$env:MERLIN_EXTERNAL\Languages\IronRuby\mspec\mspec\bin"   
  insert-path "$env:MERLIN_ROOT\Languages\Ruby\Scripts"
  insert-path "$env:MERLIN_ROOT\Languages\Ruby\Scripts\Bin"

  function global:dbin { cd "$env:MERLIN_ROOT\Bin\Debug" }
  function global:rbin { cd "$env:MERLIN_ROOT\Bin\Release" }
  if (Test-Path function:\rbt) { Remove-Item -Force function:\rbt }
  function global:rbt { cd "$env:MERLIN_ROOT\Languages\Ruby\Tests"}
  function global:script { cd "$env:MERLIN_ROOT\Scripts\Bat" }
  function global:root { cd "$env:MERLIN_ROOT" }
  function global:ext { cd "$env:MERLIN_EXTERNAL"}
  function global:d {devenv $args}
  if (Test-Path function:ruby) {Remove-item -Force function:ruby}
  if (Test-Path function:irb) {Remove-item -Force function:irb}
  rb
}

function initialize-vsvars {
  param([string]$version = "9.0")

  if (test-path HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\$version) {
          $VsKey = get-itemproperty HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\$version
  }
  else {
          if (test-path HKLM:SOFTWARE\Microsoft\VisualStudio\$version) {
                  $VsKey = get-itemproperty HKLM:SOFTWARE\Microsoft\VisualStudio\$version
          }
  }
    $VsInstallPath = [System.IO.Path]::GetDirectoryName($VsKey.InstallDir)
    $VsToolsDir = [System.IO.Path]::GetDirectoryName($VsInstallPath)
    $VsToolsDir = [System.IO.Path]::Combine($VsToolsDir, "Tools")
    $BatchFile = [System.IO.Path]::Combine($VsToolsDir, "vsvars32.bat")
    Get-Batchfile $BatchFile
    "Visual Studio $version Configured"
}

function write-command {
  param(
        [string]$command,
        [string]$message
       )
  $logfile = $env:logfile    
  $command | out-file $logfile -Append
  Invoke-Expression $command | out-file $logfile -Append
  if ($LASTEXITCODE -ne 0) {
    write-error "Command $command failed!"    
    exit-pushd
    exit 1
  }
}

function exit-pushd {
  while((Get-Command -stack).count -gt 0) {
    Pop-Location  
  }  
}
Export-ModuleMember initialize-merlin, initialize-vsvars, write-command, exit-pushd, set-envvar


