@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"ruby.exe" "c:/vsl/m2/Merlin/External.LCA_RESTRICTED/Languages/Ruby/Ruby-1.8.6p287/bin/dm" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
@"ruby.exe" "%~dpn0" %*
