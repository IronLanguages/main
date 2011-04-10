@echo off

setlocal

set _test_root=%DLR_ROOT%\Test
set _runner_sln=%_test_root%\TestRunner\TestRunner.sln
set _runner=%_test_root%\TestRunner\TestRunner\bin\Debug\TestRunner.exe

if not exist "%_runner%" call :build_runner

"%_runner%" "%_test_root%\IronPython.tests" /verbose /all /binpath:"%DLR_BIN%" /nunitoutput:"%_test_root%\TestResult.xml"

endlocal
goto:eof

:build_runner
msbuild %_runner_sln%
goto:eof
