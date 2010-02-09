@echo off

set config=Release

echo Cleaning

call %~dp0clean.bat > %~dp0clean.log

echo Make a %config% silverlight build, which generates slvx files

msbuild "%~dp0..\Silverlight.sln" /p:Configuration="Silverlight %config%" /p:SilverlightPath="C:\Program Files\Microsoft Silverlight\3.0.50106.0" /t:Rebuild /nologo /noconlog

if errorlevel 1 goto BUILDFAIL

echo Build dlr.xap

mkdir "%~dp0release\dlr"
%~dp0"..\..\..\Bin\Silverlight %config%\Chiron.exe" /d:"%~dp0release-dlrxap" /x:"%~dp0release\dlr\dlr.xap" /s

echo Copy slvx files to release

xcopy /Q "%~dp0..\..\..\Bin\Silverlight %config%\IronRuby.slvx" "%~dp0release\dlr"
xcopy /Q "%~dp0..\..\..\Bin\Silverlight %config%\IronPython.slvx" "%~dp0release\dlr"
xcopy /Q "%~dp0..\..\..\Bin\Silverlight %config%\Microsoft.Scripting.slvx" "%~dp0release\dlr"

echo Generate dlr.js and copy it to release

ruby "%~dp0generate_dlrjs.rb" > "%~dp0gendlr.log"
xcopy /Q "%~dp0dlr.js" "%~dp0release"

echo Copy getting.started

xcopy /Q /I "%~dp0..\Samples1\getting.started" "%~dp0release\getting-started"

del /Q "%~dp0gendlr.log"
del /Q "%~dp0clean.log"

echo DONE. Release is in %~dp0release

exit 0

:BUILDFAIL

echo Build FAILED!
