@echo off
setlocal ENABLEDELAYEDEXPANSION

set CHR_CMD="%~dp0..\..\silverlight\bin\ir.exe"
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  if not EXIST !CHR_CMD! set CHR_CMD="%MERLIN_ROOT%\bin\Silverlight Release\Chiron.exe"
  if not EXIST !CHR_CMD! set CHR_CMD="%MERLIN_ROOT%\bin\Silverlight Debug\Chiron.exe"
  if not EXIST !CHR_CMD! (
    echo Could not find Chiron.exe.
    echo Do you have a build of Silverlight? If not, type "bsd".
    goto END
  )
)

%CHR_CMD% /b:Tutorial/index.html /d:"%~dp0.." %*

:END
