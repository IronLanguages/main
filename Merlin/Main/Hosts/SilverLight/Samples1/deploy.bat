call %~dp0build.bat
%~dp0..\..\..\Bin\"Silverlight Release"\Chiron.exe /d:%~dp0dlr /z:%~dp0dlr.xap
if not exist C:\inetpub\wwwroot\gestalt ( mkdir C:\inetpub\wwwroot\gestalt )
xcopy /E /Q /Y %~dp0* C:\inetpub\wwwroot\gestalt
del C:\inetpub\wwwroot\gestalt\*.bat
del C:\inetpub\wwwroot\gestalt\README
if not exist C:\inetpub\wwwroot\dlr-slvx ( mkdir C:\inetpub\wwwroot\dlr-slvx )
xcopy /Q /Y %~dp0..\..\..\Bin\"Silverlight Release"\*.slvx C:\inetpub\wwwroot\dlr-slvx
