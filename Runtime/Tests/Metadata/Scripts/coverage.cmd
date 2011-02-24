set SRC=%DLR_ROOT%\Bin\v4Debug
set DST=%DLR_ROOT%\Bin\CodeCoverage
set PERF_TOOLS=%VSINSTALLDIR%\Team Tools\Performance Tools
set VSINSTR="%PERF_TOOLS%\vsinstr.exe" /coverage /outputpath:"%DST%"

rmdir /s /y "%DST%"
mkdir "%DST%"

xcopy /s /y "%SRC%\Metadata.exe" "%DST%"
xcopy /s /y "%SRC%\Microsoft.Dynamic.dll" "%DST%"
xcopy /s /y "%SRC%\Microsoft.Scripting.dll" "%DST%"

%VSINSTR% "%SRC%\Microsoft.Dynamic.dll"

"%PERF_TOOLS%\vsperfcmd.exe" /start:coverage /OUTPUT:"%DST%\Metadata"

"%DST%\Metadata.exe" /f "%DLR_ROOT%\Runtime\Tests\Metadata\TestFiles\1.exe"
"%DST%\Metadata.exe" /u /d ponetmd > NUL
 
"%PERF_TOOLS%\vsperfcmd.exe" /shutdown

@echo Code Coverage results generated to
@echo %DST%\Metadata.coverage