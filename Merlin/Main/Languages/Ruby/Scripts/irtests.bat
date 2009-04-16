@echo off
setlocal

if "%1" == "-nocompile" (
    shift
    echo Skipping compile step...
) else (
    msbuild.exe %MERLIN_ROOT%\Languages\Ruby\Ruby.sln /p:Configuration="Debug"
    if not %ERRORLEVEL%==0 goto END
    REM IronPython needs to be in sync for the language interop tests
    msbuild.exe %MERLIN_ROOT%\Languages\IronPython\IronPython.sln /p:Configuration="Debug"
    if not %ERRORLEVEL%==0 goto END
)

if "%1" == "-p" (
    shift
    set PARALLEL_IRTESTS=1
)

if defined PARALLEL_IRTESTS (
    start "Smoke Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat
) else (
    call %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat
    if not %ERRORLEVEL%==0 goto END
)

if defined PARALLEL_IRTESTS (
    start "Legacy Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat
) else (
    call %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat
    if not %ERRORLEVEL%==0 goto END
)

if defined PARALLEL_IRTESTS (
    start "Core RubySpec tests" mspec ci -fd -V :core
) else (
    call mspec ci -fd -V :core
    if not %ERRORLEVEL%==0 goto END
)

if defined PARALLEL_IRTESTS (
    start "Language RubySpec tests" mspec ci -fd -V :lang
) else (
    call mspec ci -fd -V :lang
    if not %ERRORLEVEL%==0 goto END
)

if defined PARALLEL_IRTESTS (
    start "Library RubySpec tests" mspec ci -fd -V :lib
) else (
    call mspec ci -fd -V :lib
    if not %ERRORLEVEL%==0 goto END
)

if defined PARALLEL_IRTESTS (
    start "Command Line RubySpec tests" mspec ci -fd -V :cli :netinterop
) else (
    call mspec ci -fd -V :cli :netinterop
    if not %ERRORLEVEL%==0 goto END
)

if "%1" == "-all" (
    if defined PARALLEL_IRTESTS (
        start "RubyGems tests" cmd.exe /k %MERLIN_ROOT%\bin\Debug\ir.exe %MERLIN_ROOT%\Languages\Ruby\Scripts\RubyGemsTests.rb
    ) else (
        %MERLIN_ROOT%\bin\Debug\ir.exe %MERLIN_ROOT%\Languages\Ruby\Scripts\RubyGemsTests.rb 
        if not %ERRORLEVEL%==0 goto END
    )
)

@if exist %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py (
  echo Dev10 build test:
  %MERLIN_ROOT%\Bin\Debug\ipy.exe %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py
)

:END
