.PHONY: all clean ironpython

all: ironpython

ironpython:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Debug"

ironpython-release:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Release"

testrunner:
	xbuild Test/TestRunner/TestRunner.sln

testrunner-release:
	xbuild Test/TestRunner/TestRunner.sln /p:Configuration=Release

test-ipy: ironpython testrunner
	CONFIGURATION=Debug DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Debug \
		      mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe \
		      Test/IronPython.tests /verbose /binpath:bin/Debug /all


test-ipy-release: ironpython-release testrunner-release
	CONFIGURATION=Release DLR_ROOT=`pwd` DLR_BIN=`pwd`bin/Release \
		      mono Test/TestRunner/TestRunner/bin/Release/TestRunner.exe \
		      Test/IronPython.tests /verbose /binpath:bin/Release /all

clean:
	xbuild Build.proj /t:Clean /p:Mono=true

