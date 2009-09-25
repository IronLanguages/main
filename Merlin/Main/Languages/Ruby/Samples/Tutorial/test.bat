@echo off
set IR=%~dp0..\..\..\..\Bin\Debug\ir.exe
if NOT EXIST %IR% ( set IR=%~dp0..\..\..\..\Bin\Release\ir.exe )
if NOT EXIST %IR% (
    echo No IronRuby build found! Run "bd" from a Dev.bat prompt
    goto END
)

call %~dp0clean.bat
call %~dp0build.bat

pushd %~dp0test
echo Running Tutorial desktop tests
%IR% test_console.rb

echo Running Tutorial Silverlight tests
ruby %~dp0..\..\..\..\Hosts\Silverlight\Scripts\run_tests.rb
popd

:END
