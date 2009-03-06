@echo off

set CURRENT=%CD%
set RUBY_SCRIPTS=%~dp0
set MERLIN_ROOT=%RUBY_SCRIPTS:~0,-24%
set RUBYOPT=
set RUBY18_BIN=%MERLIN_ROOT%\..\External\Languages\Ruby\ruby-1.8.6\bin
set PROGRAM_FILES_32=%ProgramFiles%
set PROGRAM_FILES_64=%ProgramFiles%
set PROGRAM_FILES_x86=%ProgramFiles(x86)%

if exist "%PROGRAM_FILES_x86%" set PROGRAM_FILES_32=%PROGRAM_FILES_x86%

if exist "%PROGRAM_FILES_32%\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat" (
    call "%PROGRAM_FILES_32%\Microsoft Visual Studio 9.0\Common7\Tools\vsvars32.bat"
    goto EnvDone
)

if exist "%PROGRAM_FILES_32%\Microsoft Visual Studio 8.0\SDK\v2.0\Bin\sdkvars.bat" (
    call "%PROGRAM_FILES_32%\Microsoft Visual Studio 8.0\SDK\v2.0\Bin\sdkvars.bat"
    goto EnvDone
)

REM But perhaps only the 64-bit SDK is installed
if exist "%PROGRAM_FILES_32%\Microsoft.NET\SDK\v2.0 64bit\Bin\sdkvars.bat" (
    call "%PROGRAM_FILES_32%\Microsoft.NET\SDK\v2.0 64bit\Bin\sdkvars.bat"
    goto EnvDone
)

if exist "%PROGRAM_FILES_32%\Microsoft.NET\SDK\v2.0\Bin\sdkvars.bat" (
    call "%PROGRAM_FILES_32%\Microsoft.NET\SDK\v2.0\Bin\sdkvars.bat"
    goto EnvDone
)

:EnvDone

set PATH=%PATH%;%MERLIN_ROOT%\Languages\Ruby\Scripts;%MERLIN_ROOT%\Languages\Ruby\Scripts\bin;%RUBY18_BIN%

call doskey /macrofile=%MERLIN_ROOT%\Scripts\Bat\%Alias.txt
cd /D %CURRENT%

REM Disable strong name validation for the assemblies we build, if it isn't already
sn -Vl | find "*,31bf3856ad364e35" > NUL 2>&1
IF NOT "%ERRORLEVEL%"=="0" goto DisableSNValidation
goto Continue

:DisableSNValidation

sn -Vr *,31bf3856ad364e35
IF NOT "%ERRORLEVEL%"=="0" goto SnError

:Continue

REM Run user specific setup
if EXIST %MERLIN_ROOT%\..\Users\%USERNAME%\Dev.bat call %MERLIN_ROOT%\..\Users\%USERNAME%\Dev.bat

set BAT=
set CURRENT=

cls

goto End

:SnError

cls
color 0C
echo. 
echo Disabling strong name validation failed!
echo.
echo We use delay signing for assemblies we build and so you need to disable 
echo strong name validation during development.
echo This requires elevated permissions. 
echo Please run this script ONCE using "Run as administrator" command.
echo.

:End

