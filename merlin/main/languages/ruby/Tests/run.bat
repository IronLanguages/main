@echo off
setlocal

:: Dev unit tests

echo Running dev unit test first
if "%ROWAN_BIN%" == "" set ROWAN_BIN=%MERLIN_ROOT%\bin\debug
%ROWAN_BIN%\IronRuby.Tests.exe
if NOT "%ERRORLEVEL%" == "0" (
  echo At least 1 of dev unit tests failed
  exit /b 1
)

:: IronRuby test suite

%MERLIN_ROOT%\..\external\languages\ruby\ruby-1.8.6\bin\ruby.exe %~dp0run.rb -checkin %*
set EXITCODE=%ERRORLEVEL%

exit /b %EXITCODE%
