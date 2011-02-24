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
setup-sampletests $CURRPATH\..\..\DynamicWebServiceHelpers
copy $env:DLR_BIN\*.dll .
PS_MSBUILD sources\DynamicWebServiceHelpers.csproj
if (! $?) {
    echo "Failed to build DynamicWebServiceHelpers.csproj!"
    exit 1
}

sanitycheck-sample
#------------------------------------------------------------------------------
echo "--Running bing.py...--"
IPY_CMD bing.py FD9BAB4CBDA9B737BE80F1BBE10A30AD20780743
if (! $?) {
    #Give it one more chance
    sleep 10
    IPY_CMD bing.py FD9BAB4CBDA9B737BE80F1BBE10A30AD20780743
    if (! $?) {
        echo "Failed to run bing.py!"
        exit 1
    }
}

$test_list = @( #"flickr.py", - Need a Yahoo ID to run this one
                "injectors.py", "mathservice.py", "rss.py", "stocks.py", "weather.py")
foreach($test_name in $test_list) {
    echo "--Running $test_name...--"
    IPY_CMD $test_name
    if (! $?) {
        #Give it one more chance
        sleep 10
        IPY_CMD $test_name
        if (! $?) {
            echo "Failed to run $test_name!"
            exit 1
        }
    }
    echo ""
}

#--Cleanup---------------------------------------------------------------------
$dlr_executables = ls -Name $env:DLR_BIN\*.dll
foreach ($exe in $dlr_executables) {
	if (test-path "$PWD\$exe") {
		rm -force "$PWD\$exe"
	}
}
rm -recurse -force $PWD\sources\bin
rm -recurse -force $PWD\sources\obj
takedown-sampletests $CURRPATH\..\..\DynamicWebServiceHelpers
