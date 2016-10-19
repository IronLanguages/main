@echo off

setlocal

if "%DLR_BIN%"=="" (
  echo "You must set DLR_BIN before running this command"
  exit /b -1
)

if "%DLR_ROOT%"=="" (
  echo "You must set DLR_ROOT before running this command"
  exit /b -1
)

set _test_root=%DLR_ROOT%\Test
set _config=%CONFIGURATION%
if "%_config%"=="" (
  set _config=Debug
)

set _runner=%_test_root%\TestRunner\TestRunner\bin\%_config%\TestRunner.exe


call :build_runner

"%_runner%" "%_test_root%\IronPython.tests" /threads:1 /binpath:"%DLR_BIN%" /nunitoutput:"%_test_root%\TestResult.xml" %*

endlocal
goto:eof

:build_runner
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /t:Rebuild %_test_root%\ClrAssembly\ClrAssembly.csproj /p:Configuration=%_config% /v:quiet /nologo
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /t:Rebuild %_test_root%\TestRunner\TestRunner.sln /p:Configuration=%_config% /v:quiet /nologo
goto:eof
