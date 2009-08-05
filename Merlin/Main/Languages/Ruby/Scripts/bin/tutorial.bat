@echo off

if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  "%MERLIN_ROOT%\Languages\Ruby\Samples\Tutorial\tutorial.bat"
) else (
  ..\Samples\Tutorial\tutorial.bat"
)

