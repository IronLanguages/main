color 47

set MERLIN_TFS=c:\vsl\Merlin\Main
path=%path%;%MERLIN_TFS%\External\Tools
path=%path%;%ProgramFiles%\Windows Resource Kits\Tools
path=%path%;%MERLIN_TFS%\..\External.LCA_RESTRICTED\languages\ruby\ruby-1.8.6p287\bin
path=%path%;%MERLIN_TFS%\..\External.LCA_RESTRICTED\languages\ruby\jruby-1.1.6\bin

doskey n2=Notepad2.exe $*

REM This is required for mspec-run (for VS debugging, etc). It is normally set by mspec
REM but mspec launches mspec-run as a separate process
set RUBY_EXE=%MERLIN_ROOT%\bin\Debug\ir.exe
set RUBY19_EXE=%MERLIN_TFS%\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.1p0\bin\ruby.exe

set GEM_PATH=%MERLIN_TFS%\..\External.LCA_RESTRICTED\languages\ruby\ruby-1.8.6p287\lib\ruby\gems\1.8
set JAVA_HOME=C:\Progra~1\Java\jre6