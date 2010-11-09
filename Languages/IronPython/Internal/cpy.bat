@echo OFF

REM ---------------------------------------------------------------------------
if %PROCESSOR_ARCHITECTURE%==x86 (set FLAVOR=x86)
if %PROCESSOR_ARCHITECTURE%==AMD64 (set FLAVOR=x64)
echo %FLAVOR% (CPython)
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


REM ---------------------------------------------------------------------------
REM GLOBALS

set EXECUTABLE=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\python.exe
set TEST=%*

set ORIG_PATH=%PATH%
set PATH=%PATH%;%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\lib\site-packages\pywin32_system32


REM ---------------------------------------------------------------------------
REM Run the test

%EXECUTABLE% -B %TEST%
set LAST_EL=%ERRORLEVEL%


REM ---------------------------------------------------------------------------
REM Restore the environment

set PATH=%ORIG_PATH%


REM ---------------------------------------------------------------------------
REM Exit appropriately for the platform
if not "%IS_VISTA%" == "1" ( exit %LAST_EL% ) else (exit /b %LAST_EL%)
