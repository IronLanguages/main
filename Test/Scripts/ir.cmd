@echo off
setlocal

if "%DLR_VM%" == "" (
    set _DEFAULT_CONFIG=Debug
) else (
    set _DEFAULT_CONFIG=v2Debug
)    

if "%DLR_BIN%" == "" (
    set RUBY_EXE=%DLR_ROOT%\bin\%_DEFAULT_CONFIG%\ir.exe
) else (
    set RUBY_EXE=%DLR_BIN%\ir.exe
)

if "%DLR_VM%" == "" (
    set _EXE="%RUBY_EXE%"
) else (
    set _EXE="%DLR_VM%" "%RUBY_EXE%"
)

set HOME=%USERPROFILE%

if "%THISISSNAP%" == "1" (
  if exist "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe" (
          call %~dp0elevate.bat "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe"  /unregserver > NUL 2>&1
          call %~dp0elevate.bat "%DLR_ROOT%\Languages\Ruby\Tests\interop\com\ComTest.exe"  /regserver > NUL 2>&1
  )
)

if NOT EXIST %_EXE% (
  echo File not found: %_EXE%
  goto End
)

%_EXE% %TEST_OPTIONS% %*
::
:: There should be no operations after this point so that the exitcode or ir.exe will be avilable as ERRORLEVEL
::

:End