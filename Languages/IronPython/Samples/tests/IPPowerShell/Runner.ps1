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
setup-sampletests $CURRPATH\..\..\IPPowerShell
sanitycheck-sample
#------------------------------------------------------------------------------
echo "--Testing minsysreq.py...--"
IPY_CMD minsysreq.py 2>&1 > $env:TMP\ps_sample_test.out
if (! $?) {
    echo "minsysreq.py failed!"
    exit 1
}
type $env:TMP\ps_sample_test.out
$temp_stuff = [string](get-content $env:TMP\ps_sample_test.out)
if (! $temp_stuff.Contains("Have a nice day!")) {
    echo "minsysreq.py didn't product the correct output!"
    exit 1
}
echo ""


echo "--Testing minsysreq_ps.py...--"
IPY_CMD minsysreq_ps.py 2>&1 > $env:TMP\ps_sample_test.out
if (! $?) {
    echo "minsysreq_ps.py failed!"
    exit 1
}
type $env:TMP\ps_sample_test.out
$temp_stuff = [string](get-content $env:TMP\ps_sample_test.out)
if (! $temp_stuff.Contains("Have a nice day!")) {
    echo "minsysreq_ps.py didn't product the correct output!"
    exit 1
}
echo ""


echo "--Testing powershell.py...--"
IPY_CMD powershell.py 2>&1 > $env:TMP\ps_sample_test.out
if (! $?) {
    echo "powershell.py failed!"
    exit 1
}
type $env:TMP\ps_sample_test.out
$temp_stuff = [string](get-content $env:TMP\ps_sample_test.out)
if (! $temp_stuff.Contains("Run 'dir(shell)' to get a list of available PowerShell commands")) {
    echo "powershell.py didn't product the correct output!"
    exit 1
}
echo ""

#------------------------------------------------------------------------------
takedown-sampletests $CURRPATH\..\..\IPPowerShell