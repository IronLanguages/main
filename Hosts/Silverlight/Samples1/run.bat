@echo off
call %~dp0build.bat
%~dp0..\..\..\Bin\"Silverlight Release"\Chiron.exe /n /b:%~dp0index.html
