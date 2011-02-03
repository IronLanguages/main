@echo off

set CURRENT=%CD%
set BAT=%~dp0
set TERM=
set DLR_ROOT=%BAT:~0,-13%
set DLR_BIN=%DLR_ROOT%\bin\Debug
set DLR_VM=
set PROGRAM_FILES_32=%ProgramFiles%
set PROGRAM_FILES_64=%ProgramFiles%
set PROGRAM_FILES_x86=%ProgramFiles(x86)%

if EXIST "%PROGRAM_FILES_x86%" set PROGRAM_FILES_32=%PROGRAM_FILES_x86%
if EXIST "%PROGRAM_FILES_x86%" set PROGRAM_FILES_64=%ProgramW6432%
if EXIST "%DLR_ROOT%\Internal" set INTERNALDEV="1"
If DEFINED THISISSNAP set INTERNALDEV="1"

set RUBY19_BIN=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\Ruby\ruby19\bin
set RUBY19_EXE=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\Ruby\ruby19\bin\ruby.exe



set RUBYOPT=
set GEM_PATH=%RUBY19_BIN%\..\lib\ruby\gems\1.9.1

REM -- IronPython environment variables
set IRONPYTHONPATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\Lib

REM -- Python environment variables
set PYTHONPATH=.;%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\lib;%DLR_ROOT%\Languages\IronPython\IronPython\Lib

if EXIST "%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\bin\sn.exe" (
  set SN_UTIL="%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\bin\sn.exe"
  goto SnDone
)

if EXIST "%ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin\sn.exe" (
  set SN_UTIL="%ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin\sn.exe"
  goto SnDone
)

if EXIST "%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v7.0A\bin\sn.exe" (
  set SN_UTIL="%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v7.0A\bin\sn.exe"
  goto SnDone
)

if EXIST "%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v6.0A\bin\sn.exe" (
  set SN_UTIL="%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v6.0A\bin\sn.exe"
  goto SnDone
)

if EXIST "%ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin\x64\sn.exe" (
  set SN_UTIL="%ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin\x64\sn.exe"
  goto SnDone
)

if EXIST "%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v6.0A\bin\x64\sn.exe" (
  set SN_UTIL="%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v6.0A\bin\x64\sn.exe"
  goto SnDone
)

if EXIST "%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\sn.exe" (
  set SN_UTIL="%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\sn.exe"
  goto SnDone
)

if EXIST "%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\sn.exe" (
  set SN_UTIL="%PROGRAM_FILES_64%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\sn.exe"
  goto SnDone
)

REM SN_UTIL should be defined even if we can't find sn.exe.
set SN_UTIL="sn.exe"

:SnDone

if exist "%PROGRAM_FILES_32%\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat" (
    call "%PROGRAM_FILES_32%\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"
    goto EnvDone
)

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
if DEFINED INTERNALDEV set PATH=%PATH%;%DLR_ROOT%\External\Tools;%DLR_ROOT%\Scripts\Bat;%DLR_ROOT%\Util\Internal\Snap\bin;%DLR_ROOT%\Util\tfpt

set PATH=%PATH%;%DLR_ROOT%\Languages\Ruby\Scripts;%DLR_ROOT%\Languages\Ruby\Scripts\bin;%DLR_ROOT%\Languages\Ruby\Tests\mspec\mspec\bin;%RUBY19_BIN%

REM -- Mono
if defined DLR_VM_PATH goto MonoInitEnd
if not "%1" == "mono" goto MonoInitEnd
set DLR_VM_PATH=%~f2
if NOT EXIST %DLR_VM_PATH%\mono.exe goto MonoNotFound
set DLR_VM=%DLR_VM_PATH%\mono.exe
set PATH=%DLR_VM_PATH%;%PATH%
goto MonoInitEnd

:MonoNotFound
echo Mono runtime not found at %2
goto End

:MonoInitEnd

if not DEFINED HOME_FOR_MSPECRC (
  if DEFINED HOME (
      set HOME_FOR_MSPECRC=%HOME%
      goto SetRubyEnv
  )
  
  if DEFINED HOMEDRIVE (
    if DEFINED HOMEPATH (
      set HOME_FOR_MSPECRC=%HOMEDRIVE%%HOMEPATH%
      goto SetRubyEnv
    )
  )
  if not DEFINED USERPROFILE (
    echo Error: One of HOME, HOMEDRIVE,HOMEPATH, or USERPROFILE needs to be set
    goto END
  )
  set HOME_FOR_MSPECRC=%USERPROFILE%
)

:SetRubyEnv

if NOT EXIST "%HOME_FOR_MSPECRC%\.mspecrc" (
  copy "%DLR_ROOT%\Languages\Ruby\default.mspec" "%HOME_FOR_MSPECRC%\.mspecrc"
)

doskey /macrofile=%BAT%Alias.txt
cd /D %CURRENT%

if not DEFINED INTERNALDEV goto Continue2

REM Disable strong name validation for the assemblies we build, if it isn't already
%SN_UTIL% -Vl | find "*,31bf3856ad364e35" > NUL 2>&1
IF NOT "%ERRORLEVEL%"=="0" goto DisableSNValidation
goto Continue

:DisableSNValidation

%SN_UTIL% -Vr *,31bf3856ad364e35
IF NOT "%ERRORLEVEL%"=="0" goto SnError

:Continue

REM Disable strong name verification for development builds of Silverlight
%DLR_ROOT%\Util\Internal\Silverlight\x86ret\snskipverf.exe

:Continue2

REM Run user specific setup
if EXIST %DLR_ROOT%\..\Users\%USERNAME%\Dev.bat call %DLR_ROOT%\..\Users\%USERNAME%\Dev.bat

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

set BAT=
set CURRENT=
