rem produces an MSI for IronPython w/ IronPython Tools
pushd %DLR_ROOT%\Scripts\Python
%DLR_ROOT%\Util\IronPython\ipy.exe gopackage\main.py --ironpython --version 2.7 --toolsbin --test_buildable --bail_on_failure --dlr_enlist %DLR_ROOT%

xcopy /y %TEMP%\gopackage\Msi\IronPython.msi .

popd