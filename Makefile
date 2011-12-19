.PHONY: all clean ironpython

all: ironpython

ironpython:
	xbuild Build.proj /t:Build /p:Mono=true

testrunner:
	xbuild Test/TestRunner/TestRunner.sln	

test-ipy: ironpython testrunner
	mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe Test/IronPython.tests /all

clean:
	xbuild Build.proj /t:Clean /p:Mono=true

