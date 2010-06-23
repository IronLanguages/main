@echo off

if exist %~dp0test.log del /Q %~dp0test.log

<nul (set/p z=Building ... )
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Release /clp:ErrorsOnly /nologo 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

set ___RUBY___=%RUBY%
set RUBY=%~dp0..\..\bin\release\ir.exe -X:CompilationThreshold 1000000000

<nul (set/p z=IronRuby Rack ... )
%RUBY% %~dp0test.rb rack 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=IronRuby Sinatra ... )
%RUBY% %~dp0test.rb sinatra 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=IronRuby Rails ... )
%RUBY% %~dp0test.rb rails 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

set RUBY=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.8.6p368\bin\ruby.exe
if not exist %RUBY% ( set RUBY=C:\Ruby\bin\ruby.exe )

<nul (set/p z=MRI Rack ... )
%RUBY% %~dp0test.rb rack 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=MRI Sinatra ... )
%RUBY% %~dp0test.rb sinatra 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=MRI Rails ... )
%RUBY% %~dp0test.rb rails 1>> test.log 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

set RUBY=%___RUBY___%
echo DONE. See the "test.log" for results.
