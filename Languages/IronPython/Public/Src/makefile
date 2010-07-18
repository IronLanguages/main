CSC=csc
CONFIG=Debug

all:..\Bin\$(CONFIG)\Microsoft.Scripting.dll ..\Bin\$(CONFIG)\IronPython.dll ..\Bin\$(CONFIG)\IronPython.Modules.dll ..\Bin\$(CONFIG)\IronPythonTest.dll ..\Bin\$(CONFIG)\ipy.exe ..\Bin\$(CONFIG)\ipyw.exe 

..\Bin\$(CONFIG)\Microsoft.Scripting.dll:
    $(CSC) -t:library -t:library -out:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -r:System.Configuration.dll -recurse:Microsoft.Scripting\*.cs

..\Bin\$(CONFIG)\IronPython.dll: ..\Bin\$(CONFIG)\Microsoft.Scripting.dll
    $(CSC) -t:library -t:library -r:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -out:..\Bin\$(CONFIG)\IronPython.dll -recurse:IronPython\*.cs

..\Bin\$(CONFIG)\IronPython.Modules.dll: ..\Bin\$(CONFIG)\Microsoft.Scripting.dll ..\Bin\$(CONFIG)\IronPython.dll
    $(CSC) -t:library -t:library -r:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -r:..\Bin\$(CONFIG)\IronPython.dll-out:..\Bin\$(CONFIG)\IronPython.Modules.dll -recurse:IronPython.Modules\*.cs

..\Bin\$(CONFIG)\IronPythonTest.dll: ..\Bin\$(CONFIG)\Microsoft.Scripting.dll ..\Bin\$(CONFIG)\IronPython.dll
    $(CSC) -t:library -t:library -out:..\Bin\$(CONFIG)\IronPythonTest.dll -r:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -r:..\Bin\$(CONFIG)\IronPython.dll -recurse:IronPythonTest\*.cs

..\Bin\$(CONFIG)\ipy.exe:  ..\Bin\$(CONFIG)\IronPython.dll ..\Bin\$(CONFIG)\Microsoft.Scripting.dll
    $(CSC) -t:library -r:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -r:..\Bin\$(CONFIG)\IronPython.dll -out:..\Bin\$(CONFIG)\ipy.exe -recurse:IronPythonConsole\*.cs

..\Bin\$(CONFIG)\ipyw.exe: ..\Bin\$(CONFIG)\IronPython.dll ..\Bin\$(CONFIG)\Microsoft.Scripting.dll
    $(CSC) -t:library -r:..\Bin\$(CONFIG)\Microsoft.Scripting.dll -r:..\Bin\$(CONFIG)\IronPython.dll -define:IRONPYTHON_WINDOW  -out:..\Bin\$(CONFIG)\ipy.exe -recurse:IronPythonConsole\*.cs
