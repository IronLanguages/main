@echo off

set _LOG="%TEMP%\test_rack.log"
set _CURRENT_IR="%DLR_VM%" "%DLR_ROOT%\bin\Eelease\ir.exe"
set _SAFE_IR="%DLR_VM%" "%DLR_ROOT%\Util\IronRuby\bin\ir.exe"

if exist %_LOG% del /Q %_LOG%

<nul (set/p z=Building ... )
msbuild %DLR_ROOT%\Solutions\Ruby.sln /p:Configuration=Release /clp:ErrorsOnly /nologo 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=IronRuby Rack ... )
%_CURRENT_IR% %~dp0test.rb rack 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=IronRuby Sinatra ... )
%_CURRENT_IR% %~dp0test.rb sinatra 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=IronRuby Rails ... )
%_CURRENT_IR% %~dp0test.rb rails 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=MRI Rack ... )
%_SAFE_IR% %~dp0test.rb rack 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=MRI Sinatra ... )
%_SAFE_IR% %~dp0test.rb sinatra 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

<nul (set/p z=MRI Rails ... )
%_SAFE_IR% %~dp0test.rb rails 1>> %_LOG% 2>&1
if "%ERRORLEVEL%" equ "0" ( echo [pass] ) else ( echo [fail] )

echo DONE. See the "%_LOG%" for results.
