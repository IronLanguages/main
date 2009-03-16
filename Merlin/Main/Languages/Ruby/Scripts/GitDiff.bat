@echo off

REM There is some problem with environment variables because of the recursive inocation of the batch file.
REM The current workaround is to explicitly unset variables. However, setlocal should be used instead.
REM
REM setlocal

if "%1" == "-?" (
    echo GitDiff - enables diffing of file lists, instead of having to serially
    echo diff files without being able to go back to a previous file.
    echo Command-line options are passed through to git diff.
    echo If GIT_FOLDER_DIFF is set, it is used to diff the file lists. Default is windff.
    goto END
)

if "%GIT_DIFF_COPY_FILES%"=="" (
    rd /s /q %TEMP%\GitDiff
    mkdir %TEMP%\GitDiff
    mkdir %TEMP%\GitDiff\old
    mkdir %TEMP%\GitDiff\new

    REM This batch file will be called by git diff. This env var indicates whether it is
    REM being called directly, or inside git diff
    set GIT_DIFF_COPY_FILES=1
    
    set GIT_DIFF_OLD_FILES=%TEMP%\GitDiff\old
    set GIT_DIFF_NEW_FILES=%TEMP%\GitDiff\new

    set OLD_GIT_EXTERNAL_DIFF=%GIT_EXTERNAL_DIFF%
    set GIT_EXTERNAL_DIFF=%~dp0\GitDiff.bat
    echo "Press q and wait (git requirement) ..."
    git diff %*

    set GIT_DIFF_COPY_FILES=

    set OLD_GIT_FOLDER_DIFF=%GIT_FOLDER_DIFF%
    if "%GIT_FOLDER_DIFF%"=="" (
        set GIT_FOLDER_DIFF=windiff
        if exist "%ProgramFiles%\Beyond Compare 2\BC2.exe" (
            set GIT_FOLDER_DIFF="%ProgramFiles%\Beyond Compare 2\BC2.exe"
        )
    )
    
    windiff %TEMP%\GitDiff\old %TEMP%\GitDiff\new

    set GIT_FOLDER_DIFF=%OLD_GIT_FOLDER_DIFF%
    set GIT_EXTERNAL_DIFF=%OLD_GIT_EXTERNAL_DIFF%
    goto END
)

REM diff is called by git with 7 parameters:
REM     path old-file old-hex old-mode new-file new-hex new-mode
%RUBY18_EXE% %~dp0\GitDiff.rb %1 %2 %5

:END