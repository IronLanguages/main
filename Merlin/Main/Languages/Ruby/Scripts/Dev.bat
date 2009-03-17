@echo off

set CURRENT=%CD%
set RUBY_SCRIPTS=%~dp0
set MERLIN_ROOT=%RUBY_SCRIPTS:~0,-24%

set PROGRAM_FILES_32=%ProgramFiles%
set PROGRAM_FILES_64=%ProgramFiles%
set PROGRAM_FILES_x86=%ProgramFiles(x86)%

REM ruby.exe needs to be on the path
set RUBY18_BIN=
set RUBY18_EXE=ruby.exe
set RUBY19_EXE=c:\ruby19\bin\ruby.exe
set RUBYOPT=

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

set PATH=%PATH%;%MERLIN_ROOT%\Languages\Ruby\Scripts;%MERLIN_ROOT%\Languages\Ruby\Scripts\bin;%RUBY18_BIN%;%MERLIN_ROOT%\..\External\Languages\IronRuby\mspec\mspec\bin

if DEFINED HOME (
  set LOCAL_HOME=%HOME%
  goto SetRubyEnv
)

if DEFINED HOMEDRIVE (
  if DEFINED HOMEDIR (
    set LOCAL_HOME="%HOMEDRIVE%\%HOMEDIR%"
    goto SetRubyEnv
  )
)

if DEFINED USERPROFILE (
  set LOCAL_HOME=%USERPROFILE%
  goto SetRubyEnv
)

echo No suitable HOME environment found. This means that all of
echo HOME, HOMEDIR, HOMEDRIVE, and USERPROFILE are not set
goto RubyDone

:SetRubyEnv

if NOT exist "%LOCAL_HOME%\.mspecrc" (
  copy "%MERLIN_ROOT%\Languages\Ruby\default.mspec" "%LOCAL_HOME%\.mspecrc"
  goto RubyDone
)

:RubyDone

call doskey /macrofile=%MERLIN_ROOT%\Scripts\Bat\%Alias.txt
cd /D %CURRENT%

:Continue

REM Run user specific setup
if EXIST %MERLIN_ROOT%\..\Users\%USERNAME%\Dev.bat call %MERLIN_ROOT%\..\Users\%USERNAME%\Dev.bat

cls

:End

set BAT=
set CURRENT=
set LOCAL_HOME=

