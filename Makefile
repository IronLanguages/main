.PHONY: all clean ironpython

PREFIX=/usr/local

EXES= \
	ipy.exe \
	ipy64.exe \
	ipyw.exe \
	ipyw64.exe

LIBS= \
	IronPython \
	IronPython.Modules \
	Microsoft.Dynamic \
	Microsoft.Scripting \
	Microsoft.Scripting.AspNet \
	Microsoft.Scripting.Metadata 

DLLS_LIBS = \
	IronPython.SQLite \
	IronPython.Wpf

PYLIBS= \
	__future__.py \
	runpy.py \
	site.py

all: ironpython

ironpython:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Debug"

ironpython-release:
	xbuild Build.proj /t:Build "/p:Mono=true;BaseConfiguration=Release"

testrunner:
	xbuild Test/TestRunner/TestRunner.sln

test-ipy: ironpython testrunner
	mono Test/TestRunner/TestRunner/bin/Debug/TestRunner.exe Test/IronPython.tests /all

clean:
	xbuild Build.proj /t:Clean /p:Mono=true

install:
	mkdir -p $(PREFIX)/lib/mono/ironpython/lib
	for f in $(LIBS); do \
		cp bin/Debug/$$f.dll $(PREFIX)/lib/mono/ironpython; \
		cp bin/Debug/$$f.dll.mdb $(PREFIX)/lib/mono/ironpython; \
		cp bin/Debug/$$f.xml $(PREFIX)/lib/mono/ironpython; \
        done
	for f in $(DLLS_LIBS); do \
		cp bin/Debug/DLLs/$$f.dll $(PREFIX)/lib/mono/ironpython; \
		cp bin/Debug/DLLs/$$f.dll.mdb $(PREFIX)/lib/mono/ironpython; \
		cp bin/Debug/DLLs/$$f.xml $(PREFIX)/lib/mono/ironpython; \
        done
	for f in $(EXES); do \
		cp bin/Debug/$$f $(PREFIX)/lib/mono/ironpython; \
		cp bin/Debug/$$f.mdb $(PREFIX)/lib/mono/ironpython; \
	done
	for f in $(PYLIBS); do \
		cp bin/Debug/Lib/$$f $(PREFIX)/lib/mono/ironpython/lib; \
	done
	mkdir -p $(PREFIX)/bin
	echo mono $$MONO_OPTIONS $(PREFIX)/lib/mono/ironpython/ipy.exe $$@ > $(PREFIX)/bin/ipy
	chmod +x $(PREFIX)/bin/ipy

uninstall:
	rm -f $(PREFIX)/bin/ipy
	for f in $(LIBS) $(DLLS_LIBS); do \
		rm -f $(PREFIX)/lib/mono/ironpython/$$f.dll; \
		rm -f $(PREFIX)/lib/mono/ironpython/$$f.dll.mdb; \
		rm -f $(PREFIX)/lib/mono/ironpython/$$f.xml; \
	done
	for f in $(PYLIBS); do \
		rm -f $(PREFIX)/lib/mono/ironpython/lib/$$f; \
	done
	for f in $(EXES); do \
		rm -f $(PREFIX)/lib/mono/ironpython/$$f; \
		rm -f $(PREFIX)/lib/mono/ironpython/$$f.mdb; \
	done

