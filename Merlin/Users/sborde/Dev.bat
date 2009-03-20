color 47

path=%path%;c:\vsl\Merlin\Main\External\Tools;%ProgramFiles%\Windows Resource Kits\Tools

doskey n2=Notepad2.exe $*

REM This is required for mspec-run (for VS debugging, etc). It is normally set by mspec
REM but mspec launches mspec-run as a separate process
set RUBY_EXE=%MERLIN_ROOT%\bin\Debug\ir.exe

set GEM_PATH=c:\ruby\lib\ruby\gems\1.8