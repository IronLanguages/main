if NOT EXIST "%DLR_ROOT%" (
  echo DLR_ROOT environment variable not set
  exit /b 1
)

cd /D %~dp0
set PYTHONPATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\CPython\27\Lib;%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\Doc
set IRONPYTHONPATH=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\Lib;%DLR_ROOT%\External.LCA_RESTRICTED\Languages\IronPython\27\Doc
set CPY=%DLR_ROOT%\External.LCA_RESTRICTED\Languages\CPython\27\python.exe

:GenerateHtmlHelp
rem mkdir Output\Html
rem %CPY% IronPythonDocs\tools\sphinx-build.py IronPythonDocs\ Output\Html\

:GenerateCHMHelp
set HTMLHELP=%PROGRAMFILES%
if not "%PROGRAMFILES(x86)%" == "" set HTMLHELP=%PROGRAMFILES(x86)%
set HTMLHELP=%HTMLHELP%\HTML Help Workshop\hhc.exe

if EXIST "%HTMLHELP%" (
    mkdir Output\CHtml
    "%CPY%" IronPythonDocs\tools\sphinx-build.py -bhtmlhelp IronPythonDocs\ Output\CHtml
    "%HTMLHELP%" Output\CHtml\IronPython.hhp
    if not EXIST "%1" mkdir %1
    copy Output\CHtml\IronPython.chm %1
    exit /b 0
) else (
    echo You must install HTML Help Workshop to produce a chm
    echo You can do this by running HtmlHelp.exe from %~dp0
    exit /b 1
)
    

