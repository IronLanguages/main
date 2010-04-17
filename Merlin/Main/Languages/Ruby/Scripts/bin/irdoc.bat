@echo off
setlocal

set IR_CMD="%~dp0ir.exe"
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  set IR_CMD="%MERLIN_ROOT%\bin\Debug\ir.exe"
)

%IR_CMD% "%~dp0rdoc" %*
