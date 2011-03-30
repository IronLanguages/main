REM Assumes Release, Silverlight3Release and Silverligh4Release binaries built.

set SRC_BIN=%DLR_ROOT%\Bin\Release
set SRC_SL_BIN=%DLR_ROOT%\Bin\Silverlight4Release
set SRC_WP_BIN=%DLR_ROOT%\Bin\Silverlight3Release

set ZIPTEMPDIR=%TEMP%\ZipRelease
:retry
if EXIST "%ZIPTEMPDIR%" (
	set ZIPTEMPDIR=%TEMP%\ZipRelease%RANDOM%
	goto retry
)

set ZIPTEMPDIR=%ZIPTEMPDIR%\IronRuby

mkdir  %ZIPTEMPDIR%
pushd %ZIPTEMPDIR%

REM bin

mkdir bin
copy %DLR_ROOT%\Languages\Ruby\Scripts\bin\* bin

copy %SRC_BIN%\Microsoft.Dynamic.dll bin
copy %SRC_BIN%\Microsoft.Scripting.dll bin
copy %SRC_BIN%\Microsoft.Scripting.Metadata.dll bin

copy %SRC_BIN%\ir.exe bin
copy %SRC_BIN%\ir64.exe bin
copy %SRC_BIN%\irw.exe bin
copy %SRC_BIN%\irw64.exe bin
copy %SRC_BIN%\IronRuby.dll bin
copy %SRC_BIN%\IronRuby.Libraries.dll bin
copy %SRC_BIN%\IronRuby.Libraries.Yaml.dll bin

REM Silverlight

mkdir Silverlight\bin
pushd Silverlight\bin

copy %SRC_SL_BIN%\Microsoft.Dynamic.dll .
copy %SRC_SL_BIN%\Microsoft.Scripting.dll .
copy %SRC_SL_BIN%\Microsoft.Scripting.Silverlight.dll .
copy %SRC_SL_BIN%\Chiron.exe .
copy %SRC_SL_BIN%\Chiron.exe.config .

copy %SRC_SL_BIN%\IronRuby.dll .
copy %SRC_SL_BIN%\IronRuby.Libraries.dll .
copy %SRC_SL_BIN%\IronRuby.Libraries.Yaml.dll .

copy %DLR_ROOT%\Util\Silverlight\SDK\4.0\System.Numerics.dll .
popd

mkdir Silverlight\script
xcopy %DLR_ROOT%\Hosts\Silverlight\Public\script\* Silverlight\script

mkdir Silverlight\script\templates\ruby
xcopy /s %DLR_ROOT%\Hosts\Silverlight\Public\script\templates\ruby Silverlight\script\templates\ruby

REM Windows Phone 7

mkdir "Windows Phone 7"
pushd "Windows Phone 7"

copy %SRC_WP_BIN%\Microsoft.Dynamic.dll .
copy %SRC_WP_BIN%\Microsoft.Scripting.dll .
copy %SRC_WP_BIN%\Microsoft.Scripting.Core.dll .
copy %SRC_WP_BIN%\Microsoft.Scripting.Silverlight.dll .
copy %SRC_WP_BIN%\IronRuby.dll .
copy %SRC_WP_BIN%\IronRuby.Libraries.dll .
copy %SRC_WP_BIN%\IronRuby.Libraries.Yaml.dll .

popd

REM Misc

mkdir Samples
xcopy /s %DLR_ROOT%\Languages\Ruby\Samples\* Samples

mkdir Lib
xcopy /s %DLR_ROOT%\Languages\Ruby\StdLib\* Lib

copy %DLR_ROOT%\Languages\Ruby\Public\CHANGELOG.txt .
copy %DLR_ROOT%\Languages\Ruby\Public\LICENSE.APACHE.html .
copy %DLR_ROOT%\Languages\Ruby\Public\LICENSE.CPL.txt .
copy %DLR_ROOT%\Languages\Ruby\Public\LICENSE.Ruby.txt .
copy %DLR_ROOT%\Languages\Ruby\Public\README.txt .

REM Zip

del /Q %SRC_BIN%\IronRuby-Bin.zip
cd ..
%DLR_ROOT%\Util\Misc\zip -9 -r %SRC_BIN%\IronRuby-Bin.zip IronRuby
cd /D %SRC_BIN%\

dir %SRC_BIN%\IronRuby-Bin.zip

popd
:EXIT