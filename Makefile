.PHONY: all clean ironpython

all: ironpython

ironpython:
	xbuild Solutions/IronPython.Mono.sln

testrunner:
	xbuild Test/TestRunner/TestRunner.sln	

test-ipy: ironpython testrunner
	mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe Test/IronPython.tests /all

clean:
	xbuild Solutions/IronPython.Mono.sln /t:Clean

