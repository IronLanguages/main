@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"C:\Ruby\bin\ruby.exe" "C:\Ruby\bin\erb" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
"%~d0%~p0ruby.exe" "%~d0%~p0%~n0" %*
