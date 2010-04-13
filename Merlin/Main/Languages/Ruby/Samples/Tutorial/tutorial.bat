@echo off
pushd %~dp0
rake run
set E=%ERRORLEVEL%
popd
exit /B %E%
:END
