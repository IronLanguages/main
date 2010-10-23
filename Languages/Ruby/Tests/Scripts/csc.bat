@echo off
if /i "%*" == "" (
  @"ruby.exe" "-W0" "%~dpn0.rb" "%DLR_ROOT%\Languages\Ruby\Tests\Interop\net"
  goto :END
  )
@"ruby.exe" "-W0" "%~dpn0.rb" "%*"

:END

