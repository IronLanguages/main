#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

#Assumptions: 
# - DLR_BIN is based on .NET 4.0

$BUILD_TYPE = $env:DLR_BIN.Split("\")
$BUILD_TYPE = $BUILD_TYPE[$BUILD_TYPE.Length-1] #Debug

if (! (@("Release", "Debug") -contains $BUILD_TYPE)) {
    echo "Cannot run this test against the '$BUILD_TYPE' build configuration."
    echo "You must have DLR_BIN pointing to .NET 4.0 based assemblies."
    exit 0
}

#------------------------------------------------------------------------------
$CURRPATH = split-path -parent $MyInvocation.MyCommand.Definition
. $CURRPATH\..\Common.ps1
if (! $?) {
    exit 1
}

#------------------------------------------------------------------------------
setup-sampletests $CURRPATH\..\..\BadPaint
sanitycheck-sample
PS_MSBUILD /p:configuration=Release BadPaint.sln /p:ReferencePath=$env:DLR_BIN
if (! $?) {
    echo "Failed to build BadPaint.sln!"
    exit 1
}

stop-process -name demo 2> $null
sleep 3
$proc_list = @(get-process | where-object { $_.ProcessName -match "^demo" })
if ($proc_list.Length -ne 0) 
{
    write-error "Failed: this test can't be run with other demo processes running - $proc_list"
    exit 1
} 
   
#------------------------------------------------------------------------------
#Startup the sample
echo "--Starting BadPaint...--"
.\bin\Release\Demo.exe

#Give the UI a chance to pop up
foreach($i in @(1..6)) {
    echo ...
    sleep 10
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^demo" })
    if ($proc_list.Length -eq 1) {
        break
    }
}
sleep 15

$proc_list = @(get-process | where-object { $_.ProcessName -match "^demo" })
if ($proc_list.Length -ne 1) 
{
    write-error "Failed: the BadPaint sample never started or has already exited!"
    exit 1
}
else {
    echo "BadPaint has started..."
}

#Now run the actual test
echo "Now running automated tests..."
PS_MSTEST /testcontainer:$CURRPATH\BadPaint.dll
if (! $?) {
    echo "mstest /testcontainer:BadPaint.dll failed!"
    exit 1
}
else {
    echo "Finished!"
}
#------------------------------------------------------------------------------
sleep 3
$proc_list = @(get-process | where-object { $_.ProcessName -match "^demo" })
if ($proc_list.Length -ne 0) 
{
    write-error "Failed: demo still running - $proc_list"
    stop-process -name demo 2> $null
    exit 1
}    
takedown-sampletests $CURRPATH\..\..\BadPaint
rm -recurse -force $CURRPATH\..\..\BadPaint\bin
rm -recurse -force $CURRPATH\..\..\BadPaint\obj