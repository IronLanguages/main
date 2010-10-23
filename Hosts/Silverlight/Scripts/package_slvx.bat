@echo off

echo DLR slvx files being created ...

set build_configuration=Silverlight3Debug
if %1 neq "" set build_configuration=%1

set build_path=%~dp0..\..\..\Bin\%build_configuration%

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
)
if exist System.Numerics.dll (
  xcopy /Y /Q System.Numerics.dll __Microsoft.Scripting.slvx
)
if exist Microsoft.CSharp.dll (
  xcopy /Y /Q Microsoft.CSharp.dll __Microsoft.Scripting.slvx
)

REM Create the SLVX files
Chiron.exe /d:__IronRuby.slvx /x:IronRuby.slvx /s
Chiron.exe /d:__IronPython.slvx /x:IronPython.slvx /s
Chiron.exe /d:__Microsoft.Scripting.slvx /x:Microsoft.Scripting.slvx /s

REM Remove temporary folders
rmdir /S /Q __IronRuby.slvx
rmdir /S /Q __IronPython.slvx
rmdir /S /Q __Microsoft.Scripting.slvx

popd

echo DLR slvx files created

echo Generating dlr.js

"%DLR_ROOT%\Util\IronRuby\bin\ir.exe" %~dp0generate_dlrjs.rb 1> %~dp0generate_dlrjs.log 2>&1
if "%ERRORLEVEL%" equ "0" (
  del %~dp0generate_dlrjs.log
  copy %~dp0dlr.js %build_path% 1> %~dp0copylog.log 2>&1
  if "%ERRORLEVEL%" equ "0" (
    del %~dp0copylog.log
  ) else ( GOTO :DLRJSERROR )
) else ( GOTO :DLRJSERROR )

echo Generating dlr.xap
if exist %~dp0dlr.xap del %~dp0dlr.xap
pushd %build_path%
Chiron.exe /d:%dlr_root%\Hosts\Silverlight\Scripts\release-dlrxap /x:dlr.xap /s /n
popd

GOTO VERYEND

:DLRJSERROR
echo Failed, see generate_dlrjs.log
exit /b 1

:END
echo %build_configuration% build does not exist, aborting slvx creation
exit /b 1

:VERYEND
exit /b 0
