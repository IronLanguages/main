@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"ruby.exe" "c:/github/ironruby/dlr/External.LCA_RESTRICTED/Languages/Ruby/ruby-1.8.6p368/lib/ruby/gems/1.8/bin/dbi" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
@"ruby.exe" "%~dpn0" %*
