@echo off
call %~dp0build.bat
%~dp0..\..\..\Bin\Release\Chiron.exe /n /b:%~dp0index.html
