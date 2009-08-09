@echo off
setlocal ENABLEDELAYEDEXPANSION

set IR_CMD="%~dp0..\..\bin\ir.exe"
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  if not EXIST !IR_CMD! set IR_CMD="%MERLIN_ROOT%\bin\Release\ir.exe"
  if not EXIST !IR_CMD! set IR_CMD="%MERLIN_ROOT%\bin\Debug\ir.exe"
  if not EXIST !IR_CMD! (
    echo Could not find ir.exe
    goto END
  )
)

%IR_CMD% %~dp0wpf_tutorial.rb %*

:END
