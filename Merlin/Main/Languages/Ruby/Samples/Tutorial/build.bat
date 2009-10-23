@echo off
setlocal ENABLEDELAYEDEXPANSION

echo Copy any required Ruby libraries to a subfolder for use in the tutorial

set RUBY_STDLIBS=%~dp0..\..\lib\ruby\1.8
set IRONRUBY_LIBS=%RUBY_STDLIBS%
set GEMS=%~dp0..\..\lib\IronRuby\gems\1.8\gems
if defined MERLIN_ROOT (
  REM - This is a dev environment. See http://wiki.github.com/ironruby/ironruby
  set RUBY_STDLIBS=%MERLIN_ROOT%\..\External.LCA_RESTRICTED\Languages\Ruby\redist-libs\ruby\1.8
  set IRONRUBY_LIBS=%MERLIN_ROOT%\Languages\Ruby\Libs
  set GEMS=%MERLIN_ROOT%\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.8.6p368\lib\ruby\gems\1.8\gems
) else (
  if not EXIST %GEMS%\minitest-1.4.2 (igem install minitest --version 1.4.2 --no-rdoc --no-ri)
)

if not EXIST "%~dp0app\Libs" (mkdir "%~dp0app\Libs")
for %%f in (erb.rb) do copy /y "%RUBY_STDLIBS%\%%f" "%~dp0app\Libs\" > %~dp0build.log
for %%f in (rdoc) do xcopy /s /y "%RUBY_STDLIBS%\%%f" "%~dp0app\Libs\%%f\" > %~dp0build.log
for %%f in (stringio.rb, bigdecimal.rb) do copy /y "%IRONRUBY_LIBS%\%%f" "%~dp0app\Libs\" > %~dp0build.log
for %%f in (minitest-1.4.2) do xcopy /s /y "%GEMS%\%%f" "%~dp0app\Libs\%%f\" > %~dp0build.log

echo Generating dlr.js for Silverlight version

if not EXIST "%~dp0js" (mkdir "%~dp0js")
set SLPATH=%~dp0..\..\..\..\Hosts\Silverlight
if not EXIST %SLPATH%\Scripts\dlr.js (ruby %SLPATH%\Scripts\generate_dlrjs.rb)
for %%f in (dlr.js) do copy /y "%SLPATH%\Scripts\%%f" "%~dp0js" > %~dp0build.log
del %~dp0build.log
