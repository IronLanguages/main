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

require '../../util/assert.rb'
require 'block_common.rb'

def test_flow_with_no_arg
    $g = 0
    $r = take_block do
        $g += 1
        next
        $g += 10
    end 
    
    assert_equal($r, nil)
    assert_equal($g, 1001)
end 

def test_flow_with_arg
    $g = 0
    $r = take_block do
        $g += 1
        next 1
        $g += 10
    end 

    assert_equal($r, 1)
    assert_equal($g, 1001)
end 

def test_nested_call_with_arg
    $g = 0
    $r = call_method_which_take_block do 
        $g += 1
        next 2
        $g += 10
    end
    assert_equal($r, 2)
    assert_equal($g, 11001)
end 

def test_flow_with_arg_in_loop
    $g = 0
    $r = take_block_in_loop do
        $g += 1
        next 1
        $g += 10
    end 
    
    assert_equal($r, 1)
    assert_equal($g, 3003)
end 

test_flow_with_no_arg
test_flow_with_arg
test_nested_call_with_arg
test_flow_with_arg_in_loop