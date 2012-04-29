.PHONY: all clean ironpython

all: ironpython

ironpython:
	xbuild Build.proj /t:Build /p:Mono=true;BaseConfiguration=Debug
	xbuild Build.proj /t:Build /p:Mono=true;BaseConfiguration=Debug

ironpython-release:
	xbuild Build.proj /t:Build /p:Mono=true;BaseConfiguration=Release
	xbuild Build.proj /t:Build /p:Mono=true;BaseConfiguration=Release

testrunner:
	xbuild Test/TestRunner/TestRunner.sln	

test-ipy: ironpython testrunner
	mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe Test/IronPython.tests /all

clean:
	xbuild Build.proj /t:Clean /p:Mono=true

