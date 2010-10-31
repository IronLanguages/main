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

# how the match works

$g = 0

# threequal always return true
class Module
    def === other
        $g += 10
        true
    end 
end 

def test_always_match
    $g = 1
    begin
        divide_by_zero
    rescue LoadError
        $g += 100
    end 
    assert_equal($g, 111)
end

test_always_match

class Module
    def === other
        $g += 10
        false
    end
end 

# I thought "rescue" will fail, but apparently "===" did not happen
def test_always_not_match_but_rescue_by_nothing
    $g = 1
    begin 
        divide_by_zero
    rescue
        $g += 100
    end 
    assert_equal($g, 101)
end 

test_always_not_match_but_rescue_by_nothing

def test_always_not_match
    $g = 1
    begin
        divide_by_zero
    rescue ZeroDivisionError
        $g += 100       # not hit, although this looks be the expected exception
    rescue 
        $g += 1000
    end 
    assert_equal($g, 1011)
end 

test_always_not_match


# the threequal will be called twice
class Module
    def === other
        $g += 10
        self == ZeroDivisionError
    end 
end

def test_match_zero_error
    $g = 1
    begin 
        divide_by_zero
    rescue ArgumentError, ZeroDivisionError, TypeError
        $g += 100
    end
    assert_equal($g, 121)
end 

test_match_zero_error

# update $! during the === operation
class Module
    def === other
        $g += 10
        $! = TypeError.new
        true
    end 
end     

def test_match_changing_exception
    $g = 1
    begin 
        divide_by_zero
    rescue LoadError
        assert_isinstanceof($!, TypeError)
        $g += 100
    end 
    assert_nil($!)
    assert_equal($g, 111)
end 

test_match_changing_exception