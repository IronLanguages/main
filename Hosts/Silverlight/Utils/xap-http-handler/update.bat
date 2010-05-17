pushd "%DLR_ROOT%\Bin\Silverlight Release"
msbuild
popd
copy "%DLR_ROOT%\Bin\Silverlight Release\*" "%~dp0XapHttpHandler.SampleSite\Bin"
