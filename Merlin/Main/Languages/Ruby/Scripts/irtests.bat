@setlocal

start "Smoke Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\Scripts\irtest.bat

start "Legacy Tests" %MERLIN_ROOT%\Languages\Ruby\Tests\run.bat

set RUBY_SPEC_CMD=%MERLIN_ROOT%\Languages\Ruby\Scripts\RunRspec.cmd

start "Core RubySpec tests" %RUBY_SPEC_CMD% .

start "Language RubySpec tests" %RUBY_SPEC_CMD% ..\language

start "Library RubySpec tests" %RUBY_SPEC_CMD% ..\library

@if exist %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py (
  echo Dev10 build test:
  %MERLIN_ROOT%\Bin\Debug\ipy.exe %MERLIN_ROOT%\Scripts\Python\GenerateSystemCoreCsproj.py
)

