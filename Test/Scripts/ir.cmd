@echo off
setlocal

if "%DLR_BIN%" == "" (
    set TEMP_IR_PATH=%DLR_ROOT%\bin\debug
) else (
    set TEMP_IR_PATH=%DLR_BIN%
)

set HOME=%USERPROFILE%
set RUBY_EXE=%TEMP_IR_PATH%\ir.exe

if "%DLR_VM%" == "" (
    set _EXE="%RUBY_EXE%"
) else (
    set _EXE=%DLR_VM% "%RUBY_EXE%"
)
if "%THISISSNAP%" == "1" (
  if exist "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe" (
          call %~dp0elevate.bat "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe"  /unregserver > NUL 2>&1
          call %~dp0elevate.bat "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe"  /regserver > NUL 2>&1
  )
)

%_EXE% %TEST_OPTIONS% %*
::
:: There should be no operations after this point so that the exitcode or ir.exe will be avilable as ERRORLEVEL
::
