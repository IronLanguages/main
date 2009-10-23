setlocal ENABLEEXTENSIONS

set START_DIR=%1
set _WIX=%MERLIN_ROOT%\External\Wix

if "%PKG_MSIFILE%"=="" set PKG_MSIFILE=IronRuby.msi
if "%ROWAN_BIN%"=="" set ROWAN_BIN=%MERLIN_ROOT%\Bin\Release

%_WIX%\tallow  -nologo -d %START_DIR%\Lib > %~dp0StdLibTemp.wxs
if not "%ERRORLEVEL%" == "0" (
	echo tallow  standard lib failed!
	exit /b %ERRORLEVEL%
)

%_WIX%\tallow  -nologo -d %START_DIR%\Samples > %~dp0SamplesTemp.wxs
if not "%ERRORLEVEL%" == "0" (
	echo tallow IronRuby Samples failed!
	exit /b %ERRORLEVEL%
)

%MERLIN_ROOT%\Bin\Debug\ir.exe %~dp0transform_wix.rb %~dp0StdLibTemp.wxs %~dp0Feature_Lib.wxs Feature_Lib
if not "%ERRORLEVEL%" == "0" (
	echo ir.exe transform_wix.py Feature_Lib failed!
	exit /b %ERRORLEVEL%
)

%MERLIN_ROOT%\Bin\Debug\ir.exe %~dp0transform_wix.rb %~dp0Samplestemp.wxs %~dp0Feature_Samples.wxs Feature_Samples
if not "%ERRORLEVEL%" == "0" (
	echo ir.exe transform_wix.rb Feature_Samples failed!
	exit /b %ERRORLEVEL%
)

pushd %~dp0ui
%_WIX%\candle *.wxs
if not "%ERRORLEVEL%" == "0" (
	echo candle all wxs files failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\lit -out myuilib.wixlib *.wixobj
if not "%ERRORLEVEL%" == "0" (
	echo lit all wixobj files failed!
	exit /b %ERRORLEVEL%
)
popd

%_WIX%\candle %~dp0IronRuby.wxs %~dp0Core.wxs %~dp0Feature_Lib.wxs %~dp0Feature_Samples.wxs -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension"
if not "%ERRORLEVEL%" == "0" (
	echo candle IronRuby.wxs failed!
	exit /b %ERRORLEVEL%
)
%_WIX%\light -b %START_DIR% -out %PKG_MSIFILE% -loc %~dp0ui\WixUI_en-us.wxl IronRuby.wixobj Core.wixobj Feature_Lib.wixobj Feature_Samples.wixobj %~dp0ui\myuilib.wixlib -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension" %_WIX%\netfx.wixlib
if not "%ERRORLEVEL%" == "0" (
	echo light IronRuby.wixobj failed!
	exit /b %ERRORLEVEL%
)

endlocal
