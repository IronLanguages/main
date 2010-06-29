@echo off
pushd %~dp0
rake tutorial:test
set E=%ERRORLEVEL%
popd
exit /B %E%
:END
