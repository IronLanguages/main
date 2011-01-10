#=============================================================================
# Folder navigation
#

function global:bin { Set-Location "${env:DLR_ROOT}\Bin" }
function global:ip { Set-Location "${env:DLR_ROOT}\Languages\IronPython" }
function global:rb { Set-Location "${env:DLR_ROOT}\Languages\Ruby" }
function global:rt { Set-Location "${env:DLR_ROOT}\Runtime" }
function global:r { Set-Location "${env:DLR_ROOT}" }
function global:mspc { Set-Location "${env:DLR_ROOT}\Languages\Ruby\Tests\mspec" }
function global:ipl { Set-Location "${env:DLR_ROOT}\External.LCA_RESTRICTED\Languages\IronPython\27\Lib" }
function global:cpl { Set-Location "${env:DLR_ROOT}\External.LCA_RESTRICTED\Languages\CPython\27\Lib" }

function global:irk { Set-Location "`"${env:DLR_ROOT}\Hosts\IronRuby.Rack\`"" }
function global:rbs { Set-Location "`"${env:DLR_ROOT}\Languages\Ruby\Samples\`"" }
function global:msl { Set-Location "`"${env:PROGRAM_FILES_32}\Microsoft Silverlight\`"" }
function global:mss { Set-Location "`"${env:DLR_ROOT}\Hosts\Silverlight\`"" }
function global:ch { Set-Location "`"${env:DLR_ROOT}\Hosts\Silverlight\Chiron\`"" }
function global:sls { Set-Location "`"${env:DLR_ROOT}\Hosts\Silverlight\Samples\`"" }

#=============================================================================
# Build commands
#

function global:brbd { cmd /C "msbuild.exe ${env:DLR_ROOT}\Solutions\Ruby.sln /p:Configuration=`"Debug`" $args" }
function global:brbr { cmd /C "msbuild.exe ${env:DLR_ROOT}\Solutions\Ruby.sln /p:Configuration=`"Release`" $args" }
function global:bipd { cmd /C "msbuild.exe ${env:DLR_ROOT}\Solutions\IronPython.sln /p:Configuration=`"Debug`" $args" }
function global:bipr { cmd /C "msbuild.exe ${env:DLR_ROOT}\Solutions\IronPython.sln /p:Configuration=`"Release`" $args" }
function global:bmsd { cmd /C "msbuild.exe ${env:DLR_ROOT}\Runtime\Microsoft.Scripting\Microsoft.Scripting.csproj /p:Configuration=`"Debug`" $args" }
function global:geninit { cmd /C "${env:DLR_ROOT}\Languages\Ruby\Libraries\GenerateInitializers.cmd" }
function global:geninity { cmd /C "${env:DLR_ROOT}\Languages\Ruby\Libraries.Yaml\GenerateInitializers.cmd" }
function global:gencache { cmd /C "${env:DLR_ROOT}\Languages\Ruby\Ruby\Compiler\GenerateReflectionCache.cmd" }

#=============================================================================
# Silverlight Build Commands
#

function global:bsrbd { cmd /C "msbuild ${env:DLR_ROOT}\Solutions\Ruby.sln /p:Configuration=`"Silverlight Debug`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.50106.0`"" }
function global:bsrbr { cmd /C "msbuild ${env:DLR_ROOT}\Solutions\Ruby.sln /p:Configuration=`"Silverlight Release`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.50106.0`"" }
function global:bsd { cmd /C "msbuild ${env:DLR_ROOT}\Hosts\Silverlight\Silverlight.sln /p:Configuration=`"Silverlight Debug`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.50106.0`"" }
function global:bsr { cmd /C "msbuild ${env:DLR_ROOT}\Hosts\Silverlight\Silverlight.sln /p:Configuration=`"Silverlight Release`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\3.0.50106.0`"" }
function global:bsd4 { cmd /C "msbuild ${env:DLR_ROOT}\Hosts\Silverlight\Silverlight4.sln /p:Configuration=`"Silverlight 4 Debug`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\4.0.41108.0 `"" }
function global:bsr4 { cmd /C "msbuild ${env:DLR_ROOT}\Hosts\Silverlight\Silverlight4.sln /p:Configuration=`"Silverlight 4 Release`" /p:SilverlightPath=`"C:\Program Files\Microsoft Silverlight\4.0.41108.0 `"" }

#=============================================================================
# [Iron]Python program aliases
#

function global:ipy { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ipy.exe`" $args" }
function global:ipyr { cmd /C "`"${env:DLR_ROOT}\Bin\Release\ipy.exe`" -X:TabCompletion $args" }
function global:ipyd { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ipy.exe`" -D -X:TabCompletion $args" }
function global:ipy4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Debug\ipy.exe`" $args" }
function global:ipyr4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Release\ipy.exe`" -X:TabCompletion $args" }
function global:ipyd4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Debug\ipy.exe`" -D -X:TabCompletion $args" }
function global:ipym { cmd /C "mono `"${env:DLR_ROOT}\Bin\Debug\ipy.exe`" $args" }
function global:ipyrm { cmd /C "mono `"${env:DLR_ROOT}\Bin\Release\ipy.exe`" -X:TabCompletion $args" }
function global:ipydm { cmd /C "mono `"${env:DLR_ROOT}\Bin\Debug\ipy.exe`" -D -X:TabCompletion $args" }
function global:ipw { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ipyw.exe`" $args" }
function global:ipwr { cmd /C "`"${env:DLR_ROOT}\Bin\Release\ipyw.exe`" $args" }
function global:ipwd { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ipyw.exe`" -D $args" }
function global:ipi { cmd /C "`"${env:DLR_ROOT}\Bin\Release\ipy.exe`" -D -X:TabCompletion -X:AutoIndent $args" }
function global:msip { cmd /C "`"${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe`" measure-command { %DLR_ROOT%\Bin\Release\ipy.exe $args }" }

#=============================================================================
# [Iron]Ruby program aliases
#

function global:rbx { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ir.exe`" $args" }
function global:rbr { cmd /C "`"${env:DLR_ROOT}\Bin\Release\ir.exe`" $args" }
function global:rbd { cmd /C "`"${env:DLR_ROOT}\Bin\Debug\ir.exe`" -D $args" }
function global:rbx4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Debug\ir.exe`" $args" }
function global:rbr4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Release\ir.exe`" $args" }
function global:rbd4 { cmd /C "`"${env:DLR_ROOT}\Bin\V4 Debug\ir.exe`" -D $args" }
function global:rbxm { cmd /C "mono `"${env:DLR_ROOT}\Bin\Debug\ir.exe`" $args" }
function global:rbrm { cmd /C "mono `"${env:DLR_ROOT}\Bin\Release\ir.exe`" $args" }
function global:rbdm { cmd /C "mono `"${env:DLR_ROOT}\Bin\Debug\ir.exe`" -D $args" }
function global:msir { cmd /C "${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe measure-command { %DLR_ROOT%\Bin\Release\ir.exe $args }" }

#=============================================================================
# Chiron aliases
#

function global:chd { cmd /C "`"${env:DLR_ROOT}\Bin\Silverlight Debug\Chiron.exe`" $args" }
function global:chr { cmd /C "`"${env:DLR_ROOT}\Bin\Silverlight Release\Chiron.exe`" $args" }
function global:chd4 { cmd /C "`"${env:DLR_ROOT}\Bin\Silverlight 4 Debug\Chiron.exe`" $args" }
function global:chr4 { cmd /C "`"${env:DLR_ROOT}\Bin\Silverlight 4 Release\Chiron.exe`" $args" }

#=============================================================================
# Miscellaneous utilities
#

function global:n { cmd /C "notepad.exe $args" }
function global:bc { cmd /C "`"${env:PROGRAM_FILES_32}\Beyond Compare 2\Bc2.exe`" $args" }
function global:scite { cmd /C "C:\programs\ruby\scite\scite.exe $args" }
function global:ps { cmd /C "${env:windir}\system32\WindowsPowerShell\v1.0\powershell.exe $args" }

#=============================================================================
#
