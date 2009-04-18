color 47

path=%path%;c:\vsl\Merlin\Main\External\Tools
path=%path%;%ProgramFiles%\Windows Resource Kits\Tools
path=%path%;c:\Ruby\bin
path=%path%;c:\vsl\Merlin\External.LCA_RESTRICTED\languages\ruby\jruby-1.1.6\bin

doskey n2=Notepad2.exe $*

REM This is required for mspec-run (for VS debugging, etc). It is normally set by mspec
REM but mspec launches mspec-run as a separate process
set RUBY_EXE=%MERLIN_ROOT%\bin\Debug\ir.exe

set GEM_PATH=c:\ruby\lib\ruby\gems\1.8
set JAVA_HOME=C:\Progra~1\Java\jre6