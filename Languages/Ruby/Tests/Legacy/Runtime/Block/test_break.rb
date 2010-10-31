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
        break
        $g += 10
    end 
    
    assert_equal($r, nil)
    assert_equal($g, 1)
end 

def test_flow_with_arg
    $g = 0
    $r = take_block do
        $g += 1
        break 1
        $g += 10
    end 

    assert_equal($r, 1)
    assert_equal($g, 1)
end 

def test_nested_call_with_arg
    $g = 0
    $r = call_method_which_take_block do 
        $g += 1
        break 2
        $g += 10
    end
    assert_equal($r, 2)
    assert_equal($g, 1)
end 

def test_flow_with_arg_in_loop
    $g = 0
    $r = take_block_in_loop do
        $g += 1
        break 1
        $g += 10
    end 
    
    assert_equal($r, 1)
    assert_equal($g, 1)
end 

def test_proc_lambda
    $g = 0
    p = lambda do 
            $g += 1
            break 8
            $g += 10
        end

    assert_raise(LocalJumpError) { take_block &p }
    assert_equal($g, 1)

    $g = 0    
    assert_equal(p.call, 8) # ??
    assert_equal($g, 1)
end

def test_proc_new
    $g = 0
    p = Proc.new do 
        $g += 1
        break 8
        $g += 10
    end
    
    assert_raise(LocalJumpError) { take_block &p }
    assert_equal($g, 1)
    
    $g = 0    
    assert_raise(LocalJumpError) { p.call }
    assert_equal($g, 1)
end

test_flow_with_no_arg
test_flow_with_arg
test_nested_call_with_arg
test_flow_with_arg_in_loop
test_proc_lambda
test_proc_new