@echo off

pushd "%MERLIN_ROOT%\Bin\Silverlight Debug"

REM Create temporary folders for compressing
mkdir __IronPython.slvx
mkdir __IronRuby.slvx
mkdir __Microsoft.Scripting.slvx

REM Copy assemblies to the appropriate folder
copy /Y IronRuby.dll __IronRuby.slvx
copy /Y IronRuby.Libraries.dll __IronRuby.slvx
copy /Y IronPython.dll __IronPython.slvx
copy /Y IronPython.Modules.dll __IronPython.slvx
copy /Y Microsoft.Scripting.dll __Microsoft.Scripting.slvx
copy /Y Microsoft.Dynamic.dll __Microsoft.Scripting.slvx
copy /Y Microsoft.Scripting.Core.dll __Microsoft.Scripting.slvx
copy /Y Microsoft.Scripting.ExtensionAttribute.dll __Microsoft.Scripting.slvx
copy /Y Microsoft.Scripting.Silverlight.dll __Microsoft.Scripting.slvx
REM copy /Y System.Xml.Linq.dll __Microsoft.Scripting.slvx

REM Create the SLVX files
chiron /d:__IronRuby.slvx /x:IronRuby.slvx /s
chiron /d:__IronPython.slvx /x:IronPython.slvx /s
chiron /d:__Microsoft.Scripting.slvx /x:Microsoft.Scripting.slvx /s

REM Remove temporary folders
rmdir /S /Q __IronRuby.slvx
rmdir /S /Q __IronPython.slvx
rmdir /S /Q __Microsoft.Scripting.slvx

echo DLR slvx files created

REM Deploy to local webserver
if exist C:\inetpub\wwwroot goto EXISTSINETDIR

echo IIS is not installed, aborting.
exit 1

:EXISTSINETDIR

if exist C:\inetpub\wwwroot\dlr-slvx goto EXISTSDLRSLVX

mkdir C:\inetpub\wwwroot\dlr-slvx
echo C:\inetpub\wwwroot\dlr-slvx created

:EXISTSDLRSLVX
move IronRuby.slvx C:\inetpub\wwwroot\dlr-slvx
move IronPython.slvx C:\inetpub\wwwroot\dlr-slvx
move Microsoft.Scripting.slvx C:\inetpub\wwwroot\dlr-slvx

REM Set permissions so IIS will serve them
pushd C:\inetpub\wwwroot\dlr-slvx
cacls *.slvx /e /p Everyone:R
popd

popd

REM "Done"

echo DLR slvx files deployed
