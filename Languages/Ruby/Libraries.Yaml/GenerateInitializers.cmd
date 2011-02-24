@echo off
if "%1" == "-2" (
  set DIR=%DLR_ROOT%\Bin\v2Debug
) else (
  set DIR=%DLR_ROOT%\Bin\Debug
)

"%DIR%\ClassInitGenerator" "%DIR%\IronRuby.Libraries.Yaml.dll" /libraries:IronRuby.StandardLibrary.Yaml /out:%~dp0\Initializer.Generated.cs
