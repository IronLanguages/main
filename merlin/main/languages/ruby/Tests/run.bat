echo Running dev unit test first

if "%ROWAN_BIN%" == "" ( 
  %MERLIN_ROOT%\bin\debug\IronRuby.Tests.exe 
  goto :SUITE
)

%ROWAN_BIN%\IronRuby.Tests.exe

:SUITE

if NOT "%ERRORLEVEL%" == "0" (
  echo At least 1 of dev unit tests failed
  exit /b 1
)

set RubyOpt_Old=%RubyOpt%
set RubyOpt=

%MERLIN_ROOT%\..\external\languages\ruby\ruby-1.8.6\bin\ruby.exe %~dp0run.rb -checkin %*
set EXITCODE=%ERRORLEVEL%

set RubyOpt=%RubyOpt_Old%
set RubyOpt_Old=

exit /b %EXITCODE%
