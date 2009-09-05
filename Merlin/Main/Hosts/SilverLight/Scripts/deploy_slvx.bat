@echo off

pushd "%MERLIN_ROOT%\Bin\Silverlight Debug"

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
