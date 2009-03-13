@echo off
setlocal

pushd %MERLIN_ROOT%\Languages\Ruby\Tests
%MERLIN_ROOT%\..\external\languages\ruby\ruby-1.8.6p287\bin\ruby.exe %~dp0run.rb -checkin %*
set EXITCODE=%ERRORLEVEL%

popd

exit /b %EXITCODE%
