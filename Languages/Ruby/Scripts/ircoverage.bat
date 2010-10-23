set SRC=%DLR_ROOT%\Bin\Debug
set DST=%DLR_ROOT%\Bin\CodeCoverage
set PERF_TOOLS=%VSINSTALLDIR%\Team Tools\Performance Tools
set VSINSTR="%PERF_TOOLS%\vsinstr.exe" /coverage /outputpath:"%DST%"

rmdir /s /y "%DST%"
mkdir "%DST%"

xcopy /s /y "%SRC%\ir.*" "%DST%"
xcopy /s /y "%SRC%\IronRuby*" "%DST%"
xcopy /s /y "%SRC%\IronPython*" "%DST%"
xcopy /s /y "%SRC%\Microsoft.*.dll" "%DST%"

rem Instrumenting DetectFileAccessPermissions fails. See TFS bug #380474.
%VSINSTR% "%SRC%\IronRuby.dll" /EXCLUDE:IronRuby.Runtime.RubyStackTraceBuilder::DetectFileAccessPermissions
%VSINSTR% "%SRC%\IronRuby.Libraries.dll"
%VSINSTR% "%SRC%\IronRuby.Libraries.Yaml.dll"

"%PERF_TOOLS%\vsperfcmd.exe" /start:coverage /OUTPUT:"%DST%\IronRuby"

"%DST%\IronRuby.Tests.exe" %*

"%PERF_TOOLS%\vsperfcmd.exe" /shutdown

@echo Code Coverage results generated to
@echo %DST%\IronRuby.coverage