@echo off

REM "mspec -tr19" looks for "ruby19" in the path. This batch file is needed for that purpose

if not defined RUBY19_EXE (
    echo RUBY19_EXE should be set
    echo Consider setting it in %DLR_ROOT%\..\Users\%USERNAME%\Dev.bat
    goto END
)

%RUBY19_EXE% %*

:END