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

# TODO:
# assert_equal(foo(&p)) were replaced by x = foo(&p); assert_equal(x)
# known DLR bug (entering try-catch with non-empty stack)


require '../../util/assert.rb'

def take_block 
    yield
end 

def test_use_local
    local_var = 10
    p = lambda { 1 + local_var }
    x = take_block(&p)
    assert_equal(x, 11)
    local_var = 100
    x = take_block(&p)
    assert_equal(x, 101)
end 

def test_use_global
    $g = 10
    p = lambda { 1 + $g }
    x = take_block(&p)
    assert_equal(x, 11)
    $g = 100
    x = take_block(&p)
    assert_equal(x, 101)    
end 

def test_use_arg
    def _take_arg(arg)
        lambda { 1 + arg  }
    end 
    p = _take_arg(10)
    x = take_block(&p)
    assert_equal(x, 11)
    p = _take_arg(100)
    x = take_block(&p)
    assert_equal(x, 101)    
end 

def test_use_misc
    $g = 1
    def _take_arg(arg1, arg2)
        l1 = arg1
        l2 = 7
        lambda { $g + l1 + l2 + arg2 }
    end 
    p = _take_arg(10, 100)
    assert_equal(p.call, 118)
    
    $g = 2
    p = _take_arg(20, 200)
    x = take_block(&p)
    assert_equal(x, 229)
end

def test_write_to_local
    local_var = 1
    p = lambda { |x| local_var += x }

    p.call(10)
    assert_equal(local_var, 11)
    p.call(100)
    assert_equal(local_var, 111)
end 

def test_write_to_global
    $g = 1
    p = lambda { |x| $g += x }
    
    p.call(10)
    assert_equal($g, 11)
    p.call(100)
    assert_equal($g, 111)
end 

test_use_local
test_use_global
test_use_arg
test_use_misc
test_write_to_local
test_write_to_global