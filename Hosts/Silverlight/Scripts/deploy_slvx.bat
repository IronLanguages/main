@echo off

set configuration=Debug
if "%1" neq "" set configuration=%1

set build_path="%DLR_ROOT%\Bin\Silverlight %configuration%"

if not exist %build_path% goto END

pushd %build_path%

REM Deploy to local webserver
if exist C:\inetpub\wwwroot goto EXISTSINETDIR

echo IIS is not installed, aborting.
exit 1

:EXISTSINETDIR

if exist C:\inetpub\wwwroot\dlr-slvx goto EXISTSDLRSLVX

mkdir C:\inetpub\wwwroot\dlr-slvx
echo C:\inetpub\wwwroot\dlr-slvx created

:EXISTSDLRSLVX
copy IronRuby.slvx C:\inetpub\wwwroot\dlr-slvx
copy IronPython.slvx C:\inetpub\wwwroot\dlr-slvx
copy Microsoft.Scripting.slvx C:\inetpub\wwwroot\dlr-slvx

REM Set permissions so IIS will serve them
pushd C:\inetpub\wwwroot\dlr-slvx
cacls *.slvx /e /p Everyone:R
popd

popd

REM "Done"

echo DLR slvx files deployed
