
@echo off

set CURRENT=%CD%
set RUBY_SCRIPTS=%~dp0
set DLR_ROOT=%RUBY_SCRIPTS:~0,-24%
set ROWAN_RUNTIME=

set PROGRAM_FILES_32=%ProgramFiles%
set PROGRAM_FILES_64=%ProgramFiles%
set PROGRAM_FILES_x86=%ProgramFiles(x86)%

if EXIST "%PROGRAM_FILES_x86%" set PROGRAM_FILES_32=%PROGRAM_FILES_x86%
if EXIST "%PROGRAM_FILES_x86%" set PROGRAM_FILES_64=%ProgramW6432%

REM ruby.exe needs to be on the path
set RUBY18_BIN=
set RUBY18_EXE=ruby.exe
set RUBY19_EXE=c:\ruby19\bin\ruby.exe
set RUBYOPT=
set GEM_PATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.8.6p368\lib\ruby\gems\1.8

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

set PATH=%PATH%;%DLR_ROOT%\Languages\Ruby\Scripts;%DLR_ROOT%\Languages\Ruby\Scripts\bin;%RUBY18_BIN%;%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronRuby\mspec\mspec\bin

REM -- Mono
if "%1" == "mono" (
  set ROWAN_RUNTIME=mono
  if NOT EXIST %2\mono.exe (
    echo Error: Mono Runtime not found at %2
    goto END
  )
)
REM Do not encapsulate this if-statement in parentheses, they might interact with %PATH% if it contains (x86):
if "%1" == "mono" set PATH=%PATH%;%2

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

call doskey /macrofile="%DLR_ROOT%\Scripts\Bat\%Alias.txt"
cd /D %CURRENT%

:Continue

REM Run user specific setup
if EXIST %DLR_ROOT%\..\Users\%USERNAME%\Dev.bat call %DLR_ROOT%\..\Users\%USERNAME%\Dev.bat

cls

:End

set BAT=
set CURRENT=

