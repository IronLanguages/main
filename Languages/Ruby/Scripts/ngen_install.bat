pushd %~dp0..\..\..\Bin\Release
ngen install iruby.exe
if exist Microsoft.Scripting.Core.dll ngen install Microsoft.Scripting.Core.dll
ngen install IronRuby.Libraries.dll
ngen install IronRuby.Libraries.Yaml.dll
popd
