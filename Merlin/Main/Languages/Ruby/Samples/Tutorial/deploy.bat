call %~dp0build.bat

%MERLIN_ROOT%\Bin\"Silverlight Release"\Chiron.exe /d:%~dp0app /z:%~dp0app.xap

if exist C:\inetpub\wwwroot\tutorial ( rmdir /S /Q C:\inetpub\wwwroot\tutorial )
mkdir C:\inetpub\wwwroot\tutorial
xcopy /E /Q /Y %~dp0* C:\inetpub\wwwroot\tutorial

if exist C:\inetpub\wwwroot\dlr-slvx ( rmdir /S /Q C:\inetpub\wwwroot\dlr-slvx )
mkdir C:\inetpub\wwwroot\dlr-slvx
xcopy /Q /Y %MERLIN_ROOT%\Bin\"Silverlight Release"\*.slvx C:\inetpub\wwwroot\dlr-slvx
