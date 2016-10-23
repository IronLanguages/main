@echo off
setlocal
set PATH=%PATH%;%ProgramFiles(x86)%\MSBuild\14.0\Bin\;%WINDIR%\Microsoft.NET\Framework\v4.0.30319;%WINDIR%\Microsoft.NET\Framework\v3.5

:getopts
if "%1"=="" (goto :default) else (goto :%1)
goto :exit

:default
goto :debug

:debug
set _target=Build
set _flavour=Debug
goto :main

:clean-debug
set _target=Clean
set _flavour=Debug
goto :main

:stage-debug
set _target=Stage
set _flavour=Debug
goto :main

:release
set _target=Build
set _flavour=Release
goto :main

:clean-release
set _target=Clean
set _flavour=Release
goto :main

:stage-release
set _target=Stage
set _flavour=Release
goto :main

:package-release
set _target=Package
set _flavour=Release
goto :main

:clean
echo No target 'clean'. Try 'clean-debug' or 'clean-release'.
goto :exit

:stage
echo No target 'stage'. Try 'stage-debug' or 'stage-release'.
goto :exit

:package
echo No target 'package'. Try 'package-release'.
goto :exit

:test
Test\test-ipy-tc.cmd /category:Languages\IronPython\2.X
goto :exit

:testall
Test\test-ipy-tc.cmd /all /runlong
goto :exit

:testalldisabled
Test\test-ipy-tc.cmd /all /runlong /rundisabled
goto :exit

:distclean
msbuild /t:DistClean /p:BaseConfiguration=Release /verbosity:minimal /nologo
msbuild /t:DistClean /p:BaseConfiguration=Debug /verbosity:minimal /nologo
goto :main

:main
msbuild /t:%_target% /p:BaseConfiguration=%_flavour% /verbosity:minimal /nologo
goto :exit

:core
dotnet restore
set _flavour=Release
dotnet build -c %_flavour% -o bin\netcoreapp1.0%_flavour%  -f netcoreapp1.0 .\Languages\IronPython\IronPythonConsole
msbuild Test\ClrAssembly\ClrAssembly.csproj /p:Configuration=%_flavour%
copy bin\%_flavour%\rowantest* bin\netcoreapp1.0%_flavour%

:exit
endlocal