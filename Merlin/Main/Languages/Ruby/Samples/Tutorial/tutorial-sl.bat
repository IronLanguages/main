@echo off
setlocal ENABLEDELAYEDEXPANSION

REM Copy any required Ruby libraries to a subfolder so that they will be included in the xap file

set RUBY_STDLIBS=%~dp0..\..\lib\ruby\1.8
set IRONRUBY_LIBS=%RUBY_STDLIBS%
set GEMS=%~dp0..\..\lib\IronRuby\gems\1.8\gems
if defined MERLIN_ROOT (
  set RUBY_STDLIBS=%MERLIN_ROOT%\..\External.LCA_RESTRICTED\Languages\Ruby\redist-libs\ruby\1.8
  set IRONRUBY_LIBS=%MERLIN_ROOT%\Languages\Ruby\Libs
  set GEMS=%MERLIN_ROOT%\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.8.6p287\lib\ruby\gems\1.8\gems
)

if not EXIST Libs (mkdir Libs)
for %%f in (erb.rb) do copy /y "%RUBY_STDLIBS%\%%f" "%~dp0Libs\"
for %%f in (stringio.rb) do copy /y "%IRONRUBY_LIBS%\%%f" "%~dp0Libs\"
for %%f in (minitest-1.3.1) do xcopy /s /y "%GEMS%\%%f" "%~dp0Libs\%%f\"

set CHIRON="%~dp0..\..\silverlight\bin\Chiron.exe"
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  set CHIRON="%~dp0..\..\..\..\Bin\Silverlight Release\Chiron.exe"
  if not EXIST !CHIRON! (
    set CHIRON="%~dp0..\..\..\..\Bin\Silverlight Debug\Chiron.exe"
    if not EXIST !CHIRON! (
      echo Could not find Chiron.exe. Please build it using the bsd alias
      goto END
    )
  )
  
  set QUERY_STRING=?test
)

%CHIRON% /b:Tutorial/index.html%QUERY_STRING% /d:"%~dp0.."
:END
