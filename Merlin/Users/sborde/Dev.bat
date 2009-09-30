color 47

if "%COMPUTERNAME%" == "SBORDE1" (
    set MERLIN_TFS=d:\vs_langs01_s\Merlin\Main
) else (
    set MERLIN_TFS=c:\vs_langs01_s\Merlin\Main
)
path=%path%;%MERLIN_TFS%\External\Tools
path=%path%;%ProgramFiles%\Windows Resource Kits\Tools
path=%path%;%MERLIN_TFS%\..\External.LCA_RESTRICTED\languages\ruby\ruby-1.8.6p368\bin
path=%path%;%MERLIN_TFS%\..\External.LCA_RESTRICTED\languages\ruby\jruby-1.1.6\bin

doskey n2=Notepad2.exe $*

REM This is required for mspec-run (for VS debugging, etc). It is normally set by mspec
REM but mspec launches mspec-run as a separate process
set RUBY_EXE=%MERLIN_ROOT%\bin\Debug\ir.exe
set RUBY19_EXE=%MERLIN_TFS%\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.1p129\bin\ruby.exe

REM Set GEM_HOME so that "gem install" will install to a different location, and not in source tree
REM Note that GEM_PATH points to the gems included in the source tree.
set GEM_HOME=%TEMP%\gems

set JAVA_HOME=C:\Progra~1\Java\jre6