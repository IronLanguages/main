@echo off
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Debug /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Release /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=v2Debug /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=v2Release /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Silverlight4Debug /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Silverlight4Release /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Silverlight3Debug /t:Clean
msbuild %~dp0..\..\Solutions\Ruby.sln /p:Configuration=Silverlight3Release /t:Clean
msbuild %~dp0..\..\Solutions\IronPython.sln /p:Configuration=Debug /t:Clean
msbuild %~dp0..\..\Solutions\IronPython.sln /p:Configuration=Release /t:Clean
msbuild %~dp0..\..\Solutions\IronPython.sln /p:Configuration=Silverlight4Debug /t:Clean
msbuild %~dp0..\..\Solutions\IronPython.sln /p:Configuration=Silverlight4Release /t:Clean
set dlrjs=%~dp0..\..\Hosts\Silverlight\Scripts\dlr.js
if exist dlrjs del /Q %dlrjs%
if exist %~dp0..\..\bin rmdir /S /Q %~dp0..\..\bin