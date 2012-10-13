@echo off
setlocal

set IR_CMD="%~dp0iruby.exe"
if defined DLR_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  set IR_CMD="%DLR_ROOT%\bin\Debug\iruby.exe"
)

%IR_CMD% "%~dp0rdoc" %*
