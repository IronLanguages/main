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
#--Initial Setup

param($TESTDIR)
if ($TESTDIR.EndsWith("\")) {
	$TESTDIR = $TESTDIR.Remove($TESTDIR.LastIndexOf("\"))
}
pushd $TESTDIR
rm slmod_*
rm slpkg_*
popd

if ($env:DLR_ROOT -eq $null) {
	echo "DLR_ROOT is not set.  Cannot continue!"
	exit 1
}

$package_list = @("modules", 
				  "hosting")

$exclude_list = @("hosting\stress\engine.py")

#------------------------------------------------------------------------------
#--Generate test modules for every package

pushd $env:DLR_ROOT\Languages\IronPython\Tests
foreach($pkg_name in $package_list) {
	$sl_test_name = "$TESTDIR\slpkg_$pkg_name.py"
	echo "#  Auto-generated Silverlight test for '$pkg_name' package" > $sl_test_name
	echo "from sys import exit" >> $sl_test_name
	echo "" >> $sl_test_name
	
	$test_list = dir -recurse $pkg_name | where-object {$_.Name -ne "__init__.py"}
	
	foreach($test_name in $test_list) {
		$test_name = $test_name.FullName
		$test_name = $test_name.Replace("$PWD\", "")
		if ((! $test_name.EndsWith(".py")) -or ($exclude_list -contains $test_name)) {
			echo "$test_name has been excluded."
			continue
		}
		
		$new_test_name = "slmod_" + $test_name.Replace("\", "_")
		$mod_name = $new_test_name.Remove($new_test_name.LastIndexOf(".py"))
		
		cp $test_name "$TESTDIR\$new_test_name"
		echo "try:" >> $sl_test_name
		echo "    import $mod_name" >> $sl_test_name
		echo "except SystemExit, e:" >> $sl_test_name
		echo "    if e.code!=0:" >> $sl_test_name
		echo "        print '$mod_name failed!'" >> $sl_test_name
		echo "        exit(e.code)" >> $sl_test_name
		echo "" >> $sl_test_name
	}
}

#Change permissions on sl* files (PS makes them readonly for some
#reason
pushd $TESTDIR
foreach ($x in &{dir slmod_*} ) { $x.Attributes = @("Archive")}
foreach ($x in &{dir slpkg_*} ) { $x.Attributes = @("Archive")}
popd