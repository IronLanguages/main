@echo off
pushd %~dp0
rake tutorial:silverlight
set E=%ERRORLEVEL%
popd
exit /B %E%
:END
