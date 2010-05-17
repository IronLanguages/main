@echo off
if "%1" == "-4" (
  set DIR=%DLR_ROOT%\Bin\V4 Debug
) else (
  set DIR=%DLR_ROOT%\Bin\Debug
)

"%DIR%\ClassInitGenerator" "%DIR%\IronRuby.Libraries.Yaml.dll" /libraries:IronRuby.StandardLibrary.Yaml /out:%~dp0\Initializer.Generated.cs
