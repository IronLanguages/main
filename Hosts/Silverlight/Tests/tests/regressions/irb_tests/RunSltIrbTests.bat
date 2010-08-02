@echo OFF
SETLOCAL

SET OPT=
SET TEST=

if "%1" == "" goto START

SET TEST=%1
shift /1
SET OPT=%*

:START

SET DRV=RunSilverlightTests.bat
SET TEST=irb_tests irb_tests.html /querystring:?list=%TEST% %OPT%

pushd %DLR_ROOT%\Hosts\Silverlight\Testsuites

rem pick a random browser
SET R=%RANDOM:~-1%
if %R% == 0 goto IE
if %R% == 1 goto IE
if %R% == 2 goto IE
if %R% == 3 goto IE
if %R% == 4 goto IE
if %R% == 5 goto IE
if %R% == 6 goto IE
if %R% == 7 goto IE
if %R% == 8 goto IE
if %R% == 9 goto IE
echo ERROR: failed to pick browser for random number: %R%
exit /b -1

:IE
call %DRV% DefaultBrowser %TEST%
exit /b %ERRORLEVEL%

:FF
call %DRV% FireFox %TEST%
exit /b %ERRORLEVEL%

:END
popd