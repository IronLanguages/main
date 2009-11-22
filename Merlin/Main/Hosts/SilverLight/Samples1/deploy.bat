@echo off

echo Building samples
call %~dp0build.bat

echo Building dlr.xap
%~dp0..\..\..\Bin\"Silverlight Release"\Chiron.exe /s /d:%~dp0dlr\dlr /z:%~dp0dlr\dlr.xap /e:"/dlr-slvx"

echo Deploying samples
if not exist C:\inetpub\wwwroot\gestalt ( mkdir C:\inetpub\wwwroot\gestalt )
xcopy /E /Q /Y %~dp0* C:\inetpub\wwwroot\gestalt
del C:\inetpub\wwwroot\gestalt\*.bat
del C:\inetpub\wwwroot\gestalt\README

echo Deploying SLVX files
if not exist C:\inetpub\wwwroot\dlr-slvx ( mkdir C:\inetpub\wwwroot\dlr-slvx )
xcopy /Q /Y %~dp0..\..\..\Bin\"Silverlight Release"\*.slvx C:\inetpub\wwwroot\dlr-slvx
