@echo off

setlocal

set _test_root=%DLR_ROOT%\Test
set _runner=%_test_root%\TestRunner\TestRunner\bin\Debug\TestRunner.exe

call :build_runner

"%_runner%" "%_test_root%\IronPython.tests" /verbose /threads:1 /binpath:"%DLR_BIN%" /nunitoutput:"%_test_root%\TestResult.xml" %*

endlocal
goto:eof

:build_runner
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /t:Rebuild %_test_root%\ClrAssembly\ClrAssembly.csproj /p:Configuration=Debug /v:quiet /nologo
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe /t:Rebuild %_test_root%\TestRunner\TestRunner.sln /p:Configuration=Debug /v:quiet /nologo
goto:eof
