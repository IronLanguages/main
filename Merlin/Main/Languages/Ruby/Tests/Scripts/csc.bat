@echo off
if /i "%*" == "" (
  @"ruby.exe" "%~dpn0.rb" "%MERLIN_ROOT%\Languages\Ruby\Tests\Interop\net"
  goto :END
  )
@"ruby.exe" "%~dpn0.rb" "%*"

:END

