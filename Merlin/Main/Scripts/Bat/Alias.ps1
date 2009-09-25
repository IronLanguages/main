#=============================================================================
# Folder navigation
#

function global:bin { Set-Location "${env:MERLIN_ROOT}\Bin" }
function global:ip { Set-Location "${env:MERLIN_ROOT}\Languages\IronPython" }
function global:rb { Set-Location "${env:MERLIN_ROOT}\Languages\Ruby" }
function global:rt { Set-Location "${env:MERLIN_ROOT}\Runtime" }
function global:r { Set-Location "${env:MERLIN_ROOT}" }
function global:mspc { Set-Location "${env:MERLIN_ROOT}\..\External.LCA_RESTRICTED\Languages\IronRuby\mspec" }

function global:irk { Set-Location "`"${env:MERLIN_ROOT}\Hosts\IronRuby.Rack\`"" }
function global:rbs { Set-Location "`"${env:MERLIN_ROOT}\Languages\Ruby\Samples\`"" }
function global:msl { Set-Location "`"${env:PROGRAM_FILES_32}\Microsoft Silverlight\`"" }
function global:mss { Set-Location "`"${env:MERLIN_ROOT}\Hosts\Silverlight\`"" }
function global:ch { Set-Location "`"${env:MERLIN_ROOT}\Hosts\Silverlight\Chiron\`"" }
function global:sls { Set-Location "`"${env:MERLIN_ROOT}\Hosts\Silverlight\Samples\`"" }

#=============================================================================
# Build commands
#

function global:brbd { cmd /C "msbuild.exe ${env:MERLIN_ROOT}\Languages\Ruby\Ruby.sln /p:Configuration=`"Debug`" $args" }
function global:brbr { cmd /C "msbuild.exe ${env:MERLIN_ROOT}\Languages\Ruby\Ruby.sln /p:Configuration=`"Release`" $args" }
function global:bipd { cmd /C "msbuild.exe ${env:MERLIN_ROOT}\Languages\IronPython\IronPython.sln /p:Configuration=`"Debug`" $args" }
function global:bipr { cmd /C "msbuild.exe ${env:MERLIN_ROOT}\Languages\IronPython\IronPython.sln /p:Configuration=`"Release`" $args" }
function global:bmsd { cmd /C "msbuild.exe ${env:MERLIN_ROOT}\Runtime\Microsoft.Scripting\Microsoft.Scripting.csproj /p:Configuration=`"Debug`" $args" }
function global:geninit { cmd /C "${env:MERLIN_ROOT}\Languages\Ruby\Libraries.LCA_RESTRICTED\GenerateInitializers.cmd" }
function global:geninity { cmd /C "${env:MERLIN_ROOT}\..\External.LCA_RESTRICTED\Languages\IronRuby\yaml\IronRuby.Libraries.Yaml\GenerateInitializers.cmd" }
function global:gencache { cmd /C "${env:MERLIN_ROOT}\Languages\Ruby\Ruby\Compiler\GenerateReflectionCache.cmd" }

#=============================================================================
# Silverlight Build Commands
#

function global:bsrbd { cmd /C "msbuild ${env:MERLIN_ROOT}\Languages\Ruby\Ruby.sln /p:Configuration=`"Silverlight Debug`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.40723.0`"" }
function global:bsrbr { cmd /C "msbuild ${env:MERLIN_ROOT}\Languages\Ruby\Ruby.sln /p:Configuration=`"Silverlight Release`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.40723.0`"" }
function global:bsd { cmd /C "msbuild ${env:MERLIN_ROOT}\Hosts\Silverlight\Silverlight.sln /p:Configuration=`"Silverlight Debug`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.40723.0`"" }
function global:bsr { cmd /C "msbuild ${env:MERLIN_ROOT}\Hosts\Silverlight\Silverlight.sln /p:Configuration=`"Silverlight Release`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.40723.0`"" }

#=============================================================================
# [Iron]Python program aliases
#

function global:ipy { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ipy.exe`" $args" }
function global:ipyr { cmd /C "`"${env:MERLIN_ROOT}\Bin\Release\ipy.exe`" -X:TabCompletion $args" }
function global:ipyd { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ipy.exe`" -D -X:TabCompletion $args" }
function global:ipy4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Debug\ipy.exe`" $args" }
function global:ipyr4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Release\ipy.exe`" -X:TabCompletion $args" }
function global:ipyd4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Debug\ipy.exe`" -D -X:TabCompletion $args" }
function global:ipw { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ipyw.exe`" $args" }
function global:ipwr { cmd /C "`"${env:MERLIN_ROOT}\Bin\Release\ipyw.exe`" $args" }
function global:ipwd { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ipyw.exe`" -D $args" }
function global:ipi { cmd /C "`"${env:MERLIN_ROOT}\Bin\Release\ipy.exe`" -D -X:TabCompletion -X:AutoIndent $args" }
function global:msip { cmd /C "`"${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe`" measure-command { %MERLIN_ROOT%\Bin\Release\ipy.exe $args }" }

#=============================================================================
# [Iron]Ruby program aliases
#

function global:rbx { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ir.exe`" $args" }
function global:rbr { cmd /C "`"${env:MERLIN_ROOT}\Bin\Release\ir.exe`" $args" }
function global:rbd { cmd /C "`"${env:MERLIN_ROOT}\Bin\Debug\ir.exe`" -D $args" }
function global:rbx4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Debug\ir.exe`" $args" }
function global:rbr4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Release\ir.exe`" $args" }
function global:rbd4 { cmd /C "`"${env:MERLIN_ROOT}\Bin\V4 Debug\ir.exe`" -D $args" }
function global:irb19 { cmd /C "`"${env:MERLIN_ROOT}\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.1p129\bin\irb.bat`" $args" }
function global:ruby19 { cmd /C "`"${env:MERLIN_ROOT}\..\External.LCA_RESTRICTED\Languages\Ruby\ruby-1.9.1p129\bin\ruby.exe`" $args" }
function global:msir { cmd /C "${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe measure-command { %MERLIN_ROOT%\Bin\Release\ir.exe $args }" }

#=============================================================================
# Chiron aliases
#

function global:chd { cmd /C "`"${env:MERLIN_ROOT}\Bin\Silverlight Debug\Chiron.exe`" $args" }
function global:chr { cmd /C "`"${env:MERLIN_ROOT}\Bin\Silverlight Release\Chiron.exe`" $args" }

#=============================================================================
# Miscellaneous utilities
#

function global:n { cmd /C "notepad.exe $args" }
function global:bc { cmd /C "`"${env:PROGRAM_FILES_32}\Beyond Compare 2\Bc2.exe`" $args" }
function global:scite { cmd /C "C:\programs\ruby\scite\scite.exe $args" }
function global:ps { cmd /C "${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe $args" }

#=============================================================================
#
