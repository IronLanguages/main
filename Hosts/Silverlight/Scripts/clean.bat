@echo off
echo removing dlr.js
if exist %~dp0dlr.js del %~dp0dlr.js
if exist %~dp0..\Tests\dlr.js del %~dp0..\Tests\dlr.js
echo removing chiron.log
if exist %~dp0..\Tests\chiron.log del %~dp0..\Tests\chiron.log
echo removing dlr folder
if exist %~dp0..\Tests\dlr rmdir /S /Q %~dp0..\Tests\dlr
echo cleaning samples1
if exist %~dp0..\Samples1\clean.bat call %~dp0..\Samples1\clean.bat
echo Clean release directory
if exist "%~dp0release\dlr" rmdir /S /Q "%~dp0release\dlr"
if exist "%~dp0release\dlr.js" del /Q "%~dp0release\dlr.js"
if exist "%~dp0release\getting-started" rmdir /S /Q "%~dp0release\getting-started"
