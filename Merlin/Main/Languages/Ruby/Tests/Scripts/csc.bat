@echo off
if /i "%*" == "" (
  @"ruby.exe" "%~dpn0.rb" "%MERLIN_ROOT%\Languages\Ruby\Tests\Interop"
  goto :END
  )
@"ruby.exe" "%~dpn0.rb"

:END

