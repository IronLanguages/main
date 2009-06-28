pushd "%MERLIN_ROOT%\Bin\Silverlight Release"
msbuild
popd
copy "%MERLIN_ROOT%\Bin\Silverlight Release\*" "%~dp0XapHttpHandler.SampleSite\Bin"
