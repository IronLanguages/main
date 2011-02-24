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

def test_simple_call
    $g = $c = 0
    $r = take_block do
        $g += 1
        $c += 1
        redo if $c < 5
        $g += 10
    end 
    
    assert_equal($r, 15)
    assert_equal($g, 1015)
end 

def test_simple_call_in_loop
    $g = $c = 0
    $r = take_block_in_loop do
        $g += 1
        $c += 1
        redo if $c < 5
        $g += 10
    end 
    
    assert_equal($r, 2037)
    assert_equal($g, 3037)
end 

def test_nested_call
    $g = $c = 0
    $r = call_method_which_take_block do 
        $g += 1
        $c += 1
        redo if $c < 5
        $g += 10
    end
    assert_equal($r, 15)
    assert_equal($g, 11015)
end 

def test_evaluate_args
    $g = $c = 0
    $r = take_arg_and_block( ($g+=10; 1) )  do   # not evaluated again
        $g += 1
        $c += 1
        redo if $c < 4
        $g += 100
    end 
    
    assert_equal($r, 114)
    assert_equal($g, 1114)
end 

def test_evaluate_args_in_upper_level
    $g = $c = 0
    $r = call_method_which_take_arg_and_block( ($g+=10; 1) )  do
        $g += 1
        $c += 1
        redo if $c < 4
        $g += 100
    end 
    
    assert_equal($r, 114)
    assert_equal($g, 11114)
end 

test_simple_call
test_simple_call_in_loop
test_nested_call
test_evaluate_args
test_evaluate_args_in_upper_level