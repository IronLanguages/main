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

#Assumptions: Word is installed

#------------------------------------------------------------------------------
$CURRPATH = split-path -parent $MyInvocation.MyCommand.Definition
. $CURRPATH\..\Common.ps1
if (! $?) {
    exit 1
}

#------------------------------------------------------------------------------
setup-sampletests $CURRPATH\..\..\CommentChecker
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
#Startup the sample
echo "--Starting CommentChecker...--"
IPYW_CMD main.py misspelled.py

#Give the UI a chance to pop up
foreach($i in @(1..6)) {
    echo ...
    sleep 10
    $proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
    if ($proc_list.Length -eq 1) {
        break
    }
}
sleep 15

$proc_list = @(get-process | where-object { $_.ProcessName -match "^ipyw" })
if ($proc_list.Length -ne 1) 
{
    write-error "Failed: the CommentChecker sample never started or has already exited!"
    exit 1
}
else {
    echo "CommentChecker has started..."
}

#Now run the actual test
echo "Now running automated tests..."
.\UIRunner.exe $CURRPATH\UIMap.uitest
if (! $?) {
    echo "UIRunner.exe $CURRPATH\UIMap.uitest failed!"
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
takedown-sampletests $CURRPATH\..\..\CommentChecker