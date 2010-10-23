@echo %BuildConfig% | find /i "Debug"
@if "%ERRORLEVEL%" == "0" (
    @set _SilverlightBuildDirectory=Silverlight Debug
) else (
    @set _SilverlightBuildDirectory=Silverlight Release
)

::Auto-generate tests for packages
powershell "%~dp0GenSlIpTests.ps1" %~dp0
if not "%ERRORLEVEL%" == "0" (
    exit /B %ERRORLEVEL%
)

::Copy all ipy tests ...
xcopy /q /r /y "%Dlr_Root%\Languages\IronPython\Tests\*.py" "%~dp0"

::Copy test library files ...
xcopy /q /r /y "%Dlr_Root%\Languages\IronPython\IronPython\lib\iptest\*.py" "%~dp0iptest\"
xcopy /q /r /y "%Dlr_Root%\Languages\IronPython\Tests\testpkg1\*.py" "%~dp0testpkg1\"

::Copy IronPythonTest.dll ...
xcopy /q /r /y "%Dlr_Root%\Bin\%_SilverlightBuildDirectory%\IronPythonTest.dll" "%~dp0test\"

::Copy __future__.py
copy /Y "%DLR_ROOT%\Languages\IronPython\IronPython\lib\__future__.py" "%~dp0"

