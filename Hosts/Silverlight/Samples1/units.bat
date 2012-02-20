@echo off
call %~dp0build.bat
%DLR_ROOT%"\Bin\Debug\Chiron.exe" /n /b:units/index.html
