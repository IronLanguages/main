@echo off

echo DLR slvx files being created ...

set configuration="Silverlight Debug"
if %1 neq "" set configuration=%1

set build_path="%MERLIN_ROOT%\Bin"\%configuration%

if not exist %build_path% goto END

pushd %build_path%

REM Create temporary folders for compressing
mkdir __IronPython.slvx
mkdir __IronRuby.slvx
mkdir __Microsoft.Scripting.slvx

REM Copy assemblies to the appropriate folder
xcopy /Y /Q IronRuby.dll __IronRuby.slvx
xcopy /Y /Q IronRuby.Libraries.dll __IronRuby.slvx
xcopy /Y /Q IronPython.dll __IronPython.slvx
xcopy /Y /Q IronPython.Modules.dll __IronPython.slvx
xcopy /Y /Q Microsoft.Scripting.dll __Microsoft.Scripting.slvx
xcopy /Y /Q Microsoft.Dynamic.dll __Microsoft.Scripting.slvx
xcopy /Y /Q Microsoft.Scripting.Silverlight.dll __Microsoft.Scripting.slvx
if exist Microsoft.Scripting.Core.dll (
  xcopy /Y /Q Microsoft.Scripting.Core.dll __Microsoft.Scripting.slvx
  xcopy /Y /Q Microsoft.Scripting.ExtensionAttribute.dll __Microsoft.Scripting.slvx
)
if exist System.Numerics.dll (
  xcopy /Y /Q System.Numerics.dll __Microsoft.Scripting.slvx
)
if exist Microsoft.CSharp.dll (
  xcopy /Y /Q Microsoft.CSharp.dll __Microsoft.Scripting.slvx
)

REM Create the SLVX files
chiron /d:__IronRuby.slvx /x:IronRuby.slvx /s
chiron /d:__IronPython.slvx /x:IronPython.slvx /s
chiron /d:__Microsoft.Scripting.slvx /x:Microsoft.Scripting.slvx /s

REM Remove temporary folders
rmdir /S /Q __IronRuby.slvx
rmdir /S /Q __IronPython.slvx
rmdir /S /Q __Microsoft.Scripting.slvx

echo DLR slvx files created

popd

GOTO VERYEND

:END
echo %configuration% build does not exist, aborting slvx creation
exit /b 1

:VERYEND
exit /b 0
