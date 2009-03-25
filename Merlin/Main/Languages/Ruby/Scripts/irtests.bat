@setlocal

start "Smoke Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat

start "Legacy Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat

start "Core RubySpec tests" mspec ci -fd -V :core

start "Language RubySpec tests" mspec ci -fd -V :lang

start "Library RubySpec tests" mspec ci -fd -V :lib

start "Command Line RubySpec tests" mspec ci -fd -V :cli :netinterop

@if exist %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py (
  echo Dev10 build test:
  %MERLIN_ROOT%\Bin\Debug\ipy.exe %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py
)

