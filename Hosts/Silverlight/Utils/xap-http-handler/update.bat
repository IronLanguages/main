pushd "%DLR_ROOT%\Bin\Silverlight3Release"
msbuild
popd
copy "%DLR_ROOT%\Bin\Silverlight3Release\*" "%~dp0XapHttpHandler.SampleSite\Bin"
