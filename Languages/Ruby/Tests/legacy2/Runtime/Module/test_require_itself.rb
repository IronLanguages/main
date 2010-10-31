# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************


if defined? $global_var
    $global_var += 10
else
    $global_var = 1
end

$global_var += 100

puts $global_var

require "test_require_itself.rb"

$global_var += 1000

puts $global_var

require "./test_require_itselF.rb"

$global_var += 10000

puts $global_var

