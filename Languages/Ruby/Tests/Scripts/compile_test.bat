@echo off

%DLR_ROOT%\Test\Scripts\ir.cmd -save %*
%DLR_ROOT%\Test\Scripts\ir.cmd -load %*

if NOT "%ERRORLEVEL%" == "0" (
  echo ""
  echo PreCompilation failed
  exit /b 1
)
