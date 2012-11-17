pushd %~dp0..\..\..\Bin\Release
ngen uninstall iruby.exe
if exist Microsoft.Scripting.Core.dll ngen uninstall Microsoft.Scripting.Core.dll
ngen uninstall IronRuby.Libraries.dll
ngen uninstall IronRuby.Libraries.Yaml.dll
popd
