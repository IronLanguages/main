@echo off

if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  call "%MERLIN_ROOT%\Languages\Ruby\Samples\Tutorial\tutorial.bat"
) else (
  call "%~dp0..\Samples\Tutorial\tutorial.bat"
)

