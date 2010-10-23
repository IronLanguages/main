@echo off
if exist %~dp0..\..\..\bin\Release (
  copy /Y %~dp0..\..\..\bin\Release\*.dll %~dp0..\examples\bin\
  copy /Y %~dp0..\..\..\bin\Release\*.exe %~dp0..\examples\bin\
  copy /Y %~dp0..\..\..\bin\Release\*.config %~dp0..\examples\bin\
) else (
  echo Release build does not exist
)