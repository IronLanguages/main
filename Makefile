.PHONY: all clean ironpython

all: ironpython

ironpython:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Debug"

ironpython-release:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Release"

testrunner:
	xbuild Test/ClrAssembly/ClrAssembly.csproj /t:Rebuild /p:Configuration=Debug
	xbuild Test/TestRunner/TestRunner.sln /p:Configuration=Debug
	rm -rf bin/Debug/DLLs/PresentationFramework.dll \
		   bin/Debug/DLLs/IronPython.Wpf.dll \
		   bin/Debug/DLLs/WindowsBase.dll \
		   bin/Debug/DLLs/System.Xaml.dll

testrunner-release:
	xbuild Test/ClrAssembly/ClrAssembly.csproj /t:Rebuild /p:Configuration=Release
	xbuild Test/TestRunner/TestRunner.sln /p:Configuration=Release
	rm -rf bin/Release/DLLs/PresentationFramework.dll \
		   bin/Release/DLLs/IronPython.Wpf.dll \
		   bin/Release/DLLs/WindowsBase.dll \
		   bin/Release/DLLs/System.Xaml.dll

test-ipy: ironpython testrunner
	CONFIGURATION=Debug DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Debug \
		      mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe \
		      Test/IronPython.tests /binpath:bin/Debug /all /runlong


test-ipy-release: ironpython-release testrunner-release
	CONFIGURATION=Release DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Release \
		      mono Test/TestRunner/TestRunner/bin/Release/TestRunner.exe \
		      Test/IronPython.tests /binpath:bin/Release /all /runlong


test-ipy-disabled: ironpython testrunner
	CONFIGURATION=Debug DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Debug \
		      mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe \
		      Test/IronPython.tests /binpath:bin/Debug /all /runlong /rundisabled

test-ipy-disabled-release: ironpython-release testrunner-release
	CONFIGURATION=Release DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Release \
		      mono Test/TestRunner/TestRunner/bin/Release/TestRunner.exe \
		      Test/IronPython.tests /binpath:bin/Release /all /runlong /rundisabled

package-release: 
	xbuild Build.proj /t:Package "/p:Mono=true;BaseConfiguration=Release" /nologo

clean:
	xbuild Build.proj /t:Clean /p:Mono=true /verbosity:minimal /nologo

