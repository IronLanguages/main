@echo off
if "%1" == "-4" (
  set DIR=%DLR_ROOT%\Bin\Debug
) else (
  set DIR=%DLR_ROOT%\Bin\v2Debug
)

"%DIR%\ClassInitGenerator" "%DIR%\IronRuby.Libraries.Yaml.dll" /libraries:IronRuby.StandardLibrary.Yaml /out:%~dp0\Initializer.Generated.cs
