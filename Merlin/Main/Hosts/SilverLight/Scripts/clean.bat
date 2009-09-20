@echo off
echo removing dlr.js
del %~dp0dlr.js
del %~dp0..\Tests\dlr.js
echo removing chiron.log
del %~dp0..\Tests\chiron.log
echo removing dlr folder
rmdir /S /Q %~dp0..\Tests\dlr
