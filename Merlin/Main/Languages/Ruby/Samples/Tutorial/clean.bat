@echo off

echo Removing dlr.js, app\libs, and generated html files
del %~dp0js\dlr.js > %~dp0clean.log
del %~dp0app.xap > %~dp0clean.log
rmdir /S /Q %~dp0app\Libs > %~dp0clean.log
del %~dp0app\Tutorials\*_tutorial.generated.html > %~dp0clean.log
del %~dp0clean.log
