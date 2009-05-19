@echo off
setlocal

if "%1" == "-?" (
    echo irtests.bat [-p] [-nocompile] [-?]
    exit /b 0
)

if "%1" == "-p" (
    shift
    set PARALLEL_IRTESTS=1
)

set IRTESTS_ERRORS=Results:

time /t > %TEMP%\irtests_start_time.log

:==============================================================================
: Builds

for /L %%i in (1,1,3) do kill /f ir.exe
for /L %%i in (1,1,3) do kill /f ipy.exe

if "%1" == "-nocompile" (
    shift
    echo Skipping compile step...
) else (
    msbuild.exe /verbosity:minimal %MERLIN_ROOT%\Languages\Ruby\Ruby.sln /p:Configuration="Debug"
    if not %ERRORLEVEL%==0 exit /b 1
    REM IronPython needs to be in sync for the language interop tests
    msbuild.exe /verbosity:minimal %MERLIN_ROOT%\Languages\IronPython\IronPython.sln /p:Configuration="Debug"
    if not %ERRORLEVEL%==0 exit /b 1

    if exist %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py (
        %MERLIN_ROOT%\Bin\Debug\ipy.exe %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py
        if not %ERRORLEVEL%==0 (
            set IRTESTS_ERRORS=%IRTESTS_ERRORS% Dev10 build failed!!!
            echo %IRTESTS_ERRORS%
        )
    )
)

time /t

:==============================================================================
: IronRuby tests

if defined PARALLEL_IRTESTS (
    start "Smoke Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat
) else (
    call %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat
    if not %ERRORLEVEL%==0 (
        set IRTESTS_ERRORS=%IRTESTS_ERRORS% Smoke tests failed!!!
        echo %IRTESTS_ERRORS%
    )
)

time /t

if defined PARALLEL_IRTESTS (
    start "Legacy Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat
) else (
    call %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat
    if not %ERRORLEVEL%==0 (
        set IRTESTS_ERRORS=%IRTESTS_ERRORS% Legacy tests failed!!! 
        echo %IRTESTS_ERRORS%
    )
)

time /t

:==============================================================================
: RubySpecs

REM We use mspec-run instead of mspec so that we can specify "-G thread" to disable the volatile thread tests

set MSPEC_RUN=%MERLIN_ROOT%\..\External.LCA_RESTRICTED\Languages\IronRuby\mspec\mspec\bin\mspec-run

if defined PARALLEL_IRTESTS (
    start "RubySpec A tests" cmd.exe /k %MERLIN_ROOT%\bin\Debug\ir.exe %MSPEC_RUN% -G fails -G unstable -G thread -G critical -fd :lang :core :cli :netinterop
) else (
    %MERLIN_ROOT%\bin\Debug\ir.exe %MSPEC_RUN% -G fails -G unstable -G thread -G critical -fd :lang :core :cli :netinterop
    if not %ERRORLEVEL%==0 (
        set IRTESTS_ERRORS=%IRTESTS_ERRORS% RubySpec A tests failed!!! 
        echo %IRTESTS_ERRORS%
    )
)

time /t

if defined PARALLEL_IRTESTS (
    start "RubySpec B tests" mspec ci -fd -V :lib
) else (
    call mspec ci -fd :lib
    if not %ERRORLEVEL%==0 (
        set IRTESTS_ERRORS=%IRTESTS_ERRORS% RubySpec B tests failed!!! 
        echo %IRTESTS_ERRORS%
    )
)

time /t

:==============================================================================
: RubyGems

if defined PARALLEL_IRTESTS (
    start "RubyGems tests" cmd.exe /k %MERLIN_ROOT%\bin\Debug\ir.exe %MERLIN_ROOT%\Languages\Ruby\Scripts\RubyGemsTests.rb
) else (
    %MERLIN_ROOT%\bin\Debug\ir.exe %MERLIN_ROOT%\Languages\Ruby\Scripts\RubyGemsTests.rb 
    if not %ERRORLEVEL%==0 (
        set IRTESTS_ERRORS=%IRTESTS_ERRORS% RubyGems tests failed!!!
        echo %IRTESTS_ERRORS%
   )
)

:==============================================================================

if "%IRTESTS_ERRORS%"=="Results:" (
    echo Start and end times:
    more %TEMP%\irtests_start_time.log
    time /t
    
    if defined PARALLEL_IRTESTS (
        echo All builds succeeded...
    ) else (
        REM The errorlevels do not get set correctly somehow. Hence the next line is commented out for now
        REM echo All tests passed (...
    )
) else (
    echo ...
    echo %IRTESTS_ERRORS%
)
