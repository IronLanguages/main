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

def method_1_yield
    yield
    $g += 100
end 

proc_1_yield = lambda do 
    yield
    $g += 100
end 

def test_yield_with_yield
    $g = 0
    method_1_yield { method_1_yield { "a" }}
    assert_equal($g, 200)
end 

test_yield_with_yield

def test_raise_exception
    $g = 0 
    x = nil
    begin 
        x = method_1_yield { divide_by_zero }
    rescue ZeroDivisionError
        $g += 1
    end
    assert_equal($g, 1)  
    assert_nil(x)
end 

def test_raise_exception_from_inner_yield
    $g = 0 
    x = nil
    begin 
        x = method_1_yield { method_1_yield { divide_by_zero } }
    rescue ZeroDivisionError
        $g += 1
    end
    assert_equal($g, 1)  
    assert_nil(x)
end 

def method_2_yields
    $g += 10
    yield 1
    $g += 100
    yield 2
    $g += 1000
end 

def test_raise_exception_from_sequence_yields
    $g = 0
    begin 
        method_2_yields do |x|
            divide_by_zero if x == 1
        end
    rescue ZeroDivisionError
        $g += 10000
    end
    assert_equal($g, 10010)
    
    $g = 0
    begin 
        method_2_yields do |x|
            divide_by_zero if x == 2
        end
    rescue ZeroDivisionError
        $g += 10000
    end
    assert_equal($g, 10110)    
end 

test_raise_exception
test_raise_exception_from_inner_yield
test_raise_exception_from_sequence_yields