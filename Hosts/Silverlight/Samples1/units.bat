@echo off
call %~dp0build.bat
%DLR_ROOT%"\Bin\Silverlight Debug\Chiron.exe" /n /b:units/index.html
