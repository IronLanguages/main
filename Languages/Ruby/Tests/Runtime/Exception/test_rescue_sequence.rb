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

# first match in the sequence
def test_match_the_first
    $g = 1 
    begin 
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10
    rescue NameError
        $g += 100
    rescue TypeError
        $g += 1000    
    end 
    assert_equal($g, 11)    
end 

# second match in the sequence
def test_match_the_middle
    $g = 1 
    begin 
        divide_by_zero
    rescue NameError
        $g += 10
    rescue ZeroDivisionError
        $g += 100
    rescue TypeError
        $g += 1000    
    end 
    assert_equal($g, 101)
end 

# last match in the sequence
def test_match_the_last
    $g = 1 
    begin 
        divide_by_zero
    rescue NameError
        $g += 10
    rescue TypeError
        $g += 100
    rescue ZeroDivisionError
        $g += 1000    
    end 
    assert_equal($g, 1001)
end 

# parent/child relation

# child before parent in the sequence
def test_match_exact_before_parent
    $g = 1 
    begin 
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10
    rescue TypeError
        $g += 100
    rescue StandardError
        $g += 1000    
    end 
    assert_equal($g, 11)
end 

# child after parent in the sequence
def test_match_parent_before_exact
    $g = 1 
    begin 
        divide_by_zero
    rescue StandardError
        $g += 10
    rescue TypeError
        $g += 100
    rescue ZeroDivisionError
        $g += 1000    
    end 
    assert_equal($g, 11)
end 

# unusual cases

# rescue twice by the exact exception
def test_exact_match_twice
    $g = 1
    begin 
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10
    rescue ZeroDivisionError
        $g += 100
    end    
    assert_equal($g, 11)
end 

# rescue twice by the parent exception
def test_parent_match_twice
    $g = 1
    begin 
        divide_by_zero
    rescue StandardError
        $g += 10
    rescue StandardError
        $g += 100
    end    
    assert_equal($g, 11)
end 

test_match_the_first
test_match_the_middle
test_match_the_last
test_match_exact_before_parent
test_match_parent_before_exact
test_exact_match_twice
test_parent_match_twice

