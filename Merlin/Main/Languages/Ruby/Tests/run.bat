@echo off
setlocal

pushd %MERLIN_ROOT%\Languages\Ruby\Tests
%RUBY18_EXE% %~dp0run.rb -checkin %*
set EXITCODE=%ERRORLEVEL%

popd

exit /b %EXITCODE%
