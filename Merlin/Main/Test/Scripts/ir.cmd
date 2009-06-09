@echo off
setlocal

if "%ROWAN_BIN%" == "" (
	set TEMP_IR_PATH=%MERLIN_ROOT%\bin\debug
) else (
	set TEMP_IR_PATH=%ROWAN_BIN%
)

if "%IR_OPTIONS%" == "" (
    set IR_OPTIONS=-X:Interpret
)

set HOME=%USERPROFILE%
set RUBY_EXE=%TEMP_IR_PATH%\ir.exe

%TEMP_IR_PATH%\ir.exe %IR_OPTIONS% %*
::
:: There should be no operations after this point so that the exitcode or ir.exe will be avilable as ERRORLEVEL
::