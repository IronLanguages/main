@echo off

setlocal

set _test_root=%DLR_ROOT%\Test
set _runner_sln=%_test_root%\TestRunner\TestRunner.sln
set _runner=%_test_root%\TestRunner\TestRunner\bin\Debug\TestRunner.exe

call :build_runner

"%_runner%" "%_test_root%\IronPython.tests" /verbose /all /threads:1 /binpath:"%DLR_BIN%" /nunitoutput:"%_test_root%\TestResult.xml"

endlocal
goto:eof

:build_runner
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /t:Rebuild %_runner_sln%
goto:eof
