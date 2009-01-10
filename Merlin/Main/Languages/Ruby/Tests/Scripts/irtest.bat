@echo off

echo ===================================================================
echo == IronRuby.Tests.exe
echo ===================================================================


if "%ROWAN_BIN%" == "" ( 
  set ROWAN_BIN=%MERLIN_ROOT%\bin\debug
)

%ROWAN_BIN%\IronRuby.Tests.exe

if NOT "%ERRORLEVEL%" == "0" (
  echo ""
  echo At least 1 of the dev unit tests failed
  exit /b 1
)

echo ===================================================================
echo == IronRuby.Tests.exe /partial
echo ===================================================================

pushd %ROWAN_BIN%

%ROWAN_BIN%\IronRuby.Tests.exe /partial
set EXITLEVEL=%ERRORLEVEL%
popd

if NOT "%EXITLEVEL%" == "0" (
  echo ""
  echo At least 1 of the dev unit tests failed; args: /partial
  exit /b 1
)

echo ===================================================================
echo == IronRuby.Tests.exe /interpret
echo ===================================================================

pushd %ROWAN_BIN%

%ROWAN_BIN%\IronRuby.Tests.exe /interpret
set EXITLEVEL=%ERRORLEVEL%
popd

if NOT "%EXITLEVEL%" == "0" (
  echo ""
  echo At least 1 of the dev unit tests failed; args: /interpret
  exit /b 1
)

echo ===================================================================
echo == IronRuby.Tests.exe /partial /interpret 
echo ===================================================================

pushd %ROWAN_BIN%

%ROWAN_BIN%\IronRuby.Tests.exe /partial /interpret
set EXITLEVEL=%ERRORLEVEL%
popd

if NOT "%EXITLEVEL%" == "0" (
  echo ""
  echo At least 1 of the dev unit tests failed; args: /partial /interpret
  exit /b 1
)

echo OK


