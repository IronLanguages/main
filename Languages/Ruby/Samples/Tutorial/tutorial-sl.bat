@echo off
setlocal ENABLEDELAYEDEXPANSION

call %~dp0build.bat

set CHIRON="%~dp0..\..\silverlight\bin\Chiron.exe"
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  set CHIRON="%~dp0..\..\..\..\Bin\Silverlight3Debug\Chiron.exe"
  if not EXIST !CHIRON! (
    set CHIRON="%~dp0..\..\..\..\Bin\Silverlight3Release\Chiron.exe"
    if not EXIST !CHIRON! (
      echo Could not find Chiron.exe.
      echo Do you have a build of Silverlight? If not, type "bsd".
      goto END
    )
  )
  
  set QUERY_STRING=
)

%CHIRON% /n /b:sl_tutorial.html%QUERY_STRING% %*
:END
