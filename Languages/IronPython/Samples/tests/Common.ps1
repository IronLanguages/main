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

$COMMONPATH = split-path -parent $MyInvocation.MyCommand.Definition

#------------------------------------------------------------------------------
#--Sanity
if ($env:DLR_BIN -eq $null) {
    echo "Cannot run this test without DLR_BIN being set!"
    exit 1
}
if (! (test-path "$env:DLR_BIN\ipy.exe")) {
    echo "Cannot run this test without ipy.exe being built!"
    exit 1
}

#------------------------------------------------------------------------------
$env:IPY_CMD = "$env:DLR_BIN\ipy.exe"
$env:IPYW_CMD = "$env:DLR_BIN\ipyw.exe"
set-alias IPY_CMD $env:IPY_CMD
set-alias IPYW_CMD $env:IPYW_CMD


$PS_MSBUILD = (dir $env:WINDIR\Microsoft.NET\Framework\v4* | Select-Object -First 1).FullName + "\msbuild.exe"
if (! (test-path $PS_MSBUILD)) {
    echo "Cannot use msbuild when it doesn't exist!"
    exit 1
}
set-alias PS_MSBUILD $PS_MSBUILD

$PS_MSTEST = "$env:DevEnvDir\mstest.exe"
if (! (test-path $PS_MSTEST)) {
    echo "Cannot use mstest when it doesn't exist!"
    exit 1
}
set-alias PS_MSTEST $PS_MSTEST

$UITEST_COMMON_ASSEMBLIES = dir -Name $env:DevEnvDir\PublicAssemblies\Microsoft.VisualStudio.TestTools.UITest*
$UITEST_PRIVATE_ASSEMBLIES = dir -Name $env:DevEnvDir\PrivateAssemblies\Microsoft.VisualStudio.TestTools.UITest*
$UITEST_PRIVATE_ASSEMBLIES += @("Microsoft.VisualStudio.QualityTools.Sqm.dll")
                               

#------------------------------------------------------------------------------
function setup-sampletests($sample_dir) {
    copy -recurse -force $env:DLR_ROOT\Util\Internal\Mita\*.dll $sample_dir\
    copy -force *.exe $sample_dir\
    copy -force *.dll $sample_dir\

    copy -force $COMMONPATH\UIRunner\UIRunner.exe $sample_dir\
    foreach ($x in $UITEST_COMMON_ASSEMBLIES) {
        copy -force $env:DevEnvDir\PublicAssemblies\$x $sample_dir\$x
    }
    foreach ($x in $UITEST_PRIVATE_ASSEMBLIES) {
        copy -force $env:DevEnvDir\PrivateAssemblies\$x $sample_dir\$x
    }

    
    pushd $sample_dir
}

function takedown-sampletests($sample_dir) {
	popd
	$mita_assemblies = ls -Name $env:DLR_ROOT\Util\Internal\Mita\*.dll
	foreach ($assembly in $mita_assemblies) {
		if (test-path "$sample_dir\$assembly") {
			rm -force "$sample_dir\$assembly"
		}
	}
    
	$test_executables = ls -Name $PWD\*.exe
	if ($test_executables -eq $null) {
        $test_executables = @()
    }
    foreach ($exe in $test_executables) {
		if (test-path "$sample_dir\$exe") {
			rm -force "$sample_dir\$exe"
		}
	}
    
    $test_executables = ls -Name $PWD\*.dll
	if ($test_executables -eq $null) {
        $test_executables = @()
    }
    foreach ($exe in $test_executables) {
		if (test-path "$sample_dir\$exe") {
			rm -force "$sample_dir\$exe"
		}
	}

    rm -force "$sample_dir\UIRunner.exe"
    foreach ($exe in $UITEST_COMMON_ASSEMBLIES) {
		if (test-path "$sample_dir\$exe") {
			rm -force "$sample_dir\$exe"
		}
	}
    foreach ($exe in $UITEST_PRIVATE_ASSEMBLIES) {
		if (test-path "$sample_dir\$exe") {
			rm -force "$sample_dir\$exe"
		}
	}
}

function sanitycheck-sample() {
	if (! (test-path "$PWD\readme.htm")) {
		echo "$PWD\readme.htm is missing!"
		exit 1
	}
}


