@echo off

echo removing dlr directory, dlr.js, and dlr.xap
if exist %~dp0dlr rmdir /S /Q %~dp0dlr > %~dp0temp.log
if exist %~dp0dlr.js del /Q %~dp0dlr.js > %~dp0temp.log
if exist %~dp0temp.log del %~dp0temp.log
