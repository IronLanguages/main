@echo off

echo removing dlr directory, dlr.js, and dlr.xap
rmdir /S /Q %~dp0dlr > %~dp0temp.log
del /Q %~dp0dlr.js > %~dp0temp.log
del /Q %~dp0dlr.xap > %~dp0temp.log
del %~dp0temp.log
