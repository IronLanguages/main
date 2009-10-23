@echo off
call %~dp0build.bat
%MERLIN_ROOT%"\Bin\Silverlight Debug\Chiron.exe" /n /b:units/index.html
