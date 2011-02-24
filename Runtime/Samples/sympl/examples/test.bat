@setlocal enableextensions

%~1 norepl > new.bsl
if not "%errorlevel%"=="0" (
    echo Test failed execution
    exit /b 1
)

fc new.bsl %2
if not "%errorlevel%"=="0" (
    echo Test failed baseline comparison
    exit /b 1
)

echo Pass!
exit /b 0