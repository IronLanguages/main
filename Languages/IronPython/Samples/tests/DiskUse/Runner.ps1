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

#------------------------------------------------------------------------------
$CURRPATH = split-path -parent $MyInvocation.MyCommand.Definition
. $CURRPATH\..\Common.ps1
if (! $?) {
    exit 1
}

#------------------------------------------------------------------------------
setup-sampletests $CURRPATH\..\..\DiskUse
sanitycheck-sample

stop-process -name ipyw 2> $null
sleep 3
$proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
if ($proc_list.Length -ne 0) 
{
    write-error "Failed: this test can't be run with other ipyw processes running - $proc_list"
    exit 1
}    

#------------------------------------------------------------------------------
echo "--Starting app.py...--"
IPYW_CMD app.py
#Give the UI a chance to pop up
sleep 15
$proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
if ($proc_list.Length -ne 1) 
{
    write-error "Failed: the DiskUse sample never started or has already exited!"
    exit 1
}    

echo "Now running automated tests..."
.\UIRunner.exe $CURRPATH\UIMap.uitest
if (! $?) {
    echo "DiskUse test failed!"
    exit 1
}
else {
    echo "Finished!"
}

#------------------------------------------------------------------------------
sleep 3
$proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
if ($proc_list.Length -ne 0) 
{
    write-error "Failed: ipyw still running - $proc_list"
    stop-process -name ipyw 2> $null
    exit 1
}    
takedown-sampletests $CURRPATH\..\..\DiskUse