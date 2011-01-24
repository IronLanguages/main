@echo OFF

REM ---------------------------------------------------------------------------
if %PROCESSOR_ARCHITECTURE%==x86 (set FLAVOR=x86)
if %PROCESSOR_ARCHITECTURE%==AMD64 (set FLAVOR=x64)
echo %FLAVOR% (IronPython)
ver | find "6.1"
if "%ERRORLEVEL%"=="0" (
    set IS_VISTA=1
)   else (
    ver | find "6.0.600"
    if "%ERRORLEVEL%"=="0" (set IS_VISTA=1) else (set IS_VISTA=0)
    ver | find "5.2.3790"
    if "%ERRORLEVEL%"=="0" (set IS_2003=1)  else (set IS_2003=0)
    ver | find "5.1.2600"
    if "%ERRORLEVEL%"=="0" (set IS_XP=1)    else (set IS_XP=0)
)


REM ---------------------------------------------------------------------------
REM Sanity checks

if not defined DLR_BIN (
	echo "You need to set DLR_BIN before running tests"
	if not "%IS_VISTA%" == "1" ( exit -1 ) else (exit /b -1)
)

if exist "%CD%\@test" (
	echo "Removing %CD%\@test"
	del /F /Q /S "%CD%\@test"
)

REM ---------------------------------------------------------------------------
REM GLOBALS

set EXECUTABLE=%DLR_BIN%\ipy.exe
set TEST=%*
set ORIG_TEST_OPTIONS=%TEST_OPTIONS%


REM ---------------------------------------------------------------------------
REM Handle SNAP jobs types here

if defined ISINTERACTIVE (
	set TEST=%DLR_ROOT%\Languages\IronPython\Scripts\run_interactive.py %TEST%
)

if defined ISCOMPILED (
   set TEST=%DLR_ROOT%\Languages\IronPython\Scripts\run_compiled.py %TEST%
)

REM ---------------------------------------------------------------------------
REM Determine which version of the CPython standard library this particular test
REM needs to be run against.

REM set TEST_OPTIONS=-X:Python25 %TEST_OPTIONS%

set ORIG_IRONPYTHONPATH=%IRONPYTHONPATH%
set IRONPYTHONPATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\Lib

REM ---------------------------------------------------------------------------
REM Run the test
:RUN_TEST
echo %EXECUTABLE% %TEST_OPTIONS% %TEST%

%EXECUTABLE% %TEST_OPTIONS% %TEST%
set LAST_EL=%ERRORLEVEL%

if NOT "%IS_FLAKEY%" == "" (
	set /a IS_FLAKEY=%IS_FLAKEY% - 1
	if "%IS_FLAKEY%" == "0" (
        set IS_FLAKEY=
	)

	if not "%LAST_EL%" == "0" (
		echo This flakey test failed. Will try rerunning again.
		goto RUN_TEST
	)
)

REM ---------------------------------------------------------------------------
REM Restore the environment
set TEST_OPTIONS=%ORIG_TEST_OPTIONS%
set IRONPYTHONPATH=%ORIG_IRONPYTHONPATH%


REM ---------------------------------------------------------------------------
REM Exit appropriately for the platform
if not "%IS_VISTA%" == "1" ( exit %LAST_EL% ) else (exit /b %LAST_EL%)
