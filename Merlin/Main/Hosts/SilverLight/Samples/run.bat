@echo off
mkdir %~dp0dlr
copy %~dp0..\Scripts\dlr.js .
%~dp0..\..\..\Bin\"Silverlight Release"\Chiron.exe /b:index.html
