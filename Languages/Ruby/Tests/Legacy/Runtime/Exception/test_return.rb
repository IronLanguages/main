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

def test_return_from_try
    def t
        $g = 1
        begin 
            empty_func
            return 10
            $g += 1
        rescue 
            $g += 10
        else 
            $g += 100              # won't hit
        ensure
            $g += 1000
        end 
    end
    assert_equal(t, 10)
    assert_equal($g, 1001)
end 

def test_return_from_else
    def t
        $g = 1
        begin 
            empty_func
        rescue
            $g += 10
        else 
            $g += 100
            return 20
            $g += 100
        ensure
            $g += 1000
        end 
    end
    assert_equal(t, 20)
    assert_equal($g, 1101)
end 

def test_return_from_ensure_no_exception_raised
    def t
        $g = 1
        begin 
            empty_func
        ensure 
            $g += 10
            return 30
            $g += 10
        end 
    end
    assert_equal(t, 30)
    assert_equal($g, 11)
end 

def test_return_from_ensure_when_exception_raised
    def t
        $g = 1
        begin 
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
        ensure 
            $g += 100
            return 40
            $g += 100
        end 
    end
    assert_equal(t, 40)
    assert_equal($g, 111)
end 

# never called return / sanity test codegen
def test_return_from_rescue_but_not_used
    def t
        $g = 1
        begin 
            empty_func
        rescue ZeroDivisionError
            $g += 10
            return 50
            $g += 10
        end 
    end
    assert_nil(t)
    assert_equal($g, 1)
end 

def test_return_from_rescue
    def t
        $g = 1
        begin 
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
            return 60
            $g += 10
        ensure 
            $g += 100
        end 
    end
    assert_equal(t, 60)
    assert_equal($g, 111)
end 

def test_return_from_rescue_after_retry
    def t
        $g = 1
        begin 
            $g += 1
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
            if $g < 30
                retry 
            end
            return 70
            $g += 10
        ensure 
            $g += 100
        end
    end
    assert_equal(t, 70)
    assert_equal($g, 134)
end 

def test_return_from_ensure_after_raise_in_begin
    def t
        $g = 1
        begin
            $g += 10
            divide_by_zero
            $g += 100
        ensure
            $g += 100
            return 80
        end 
    end 
    
    assert_equal(t, 80)
    assert_equal($g, 111)
end 

def test_return_from_ensure_after_raise_in_rescue
    def t
        $g = 1
        begin 
            $g += 10
            divide_by_zero
            $g += 10
        rescue
            $g += 100
            raise "again"
            $g += 100
        ensure 
            return 90
        end 
    end 
    
    assert_equal(t, 90) # no need to rescue
    assert_equal($g, 111)
end 

test_return_from_try
test_return_from_else
test_return_from_ensure_no_exception_raised
test_return_from_ensure_when_exception_raised
test_return_from_rescue_but_not_used
test_return_from_rescue
test_return_from_rescue_after_retry
test_return_from_ensure_after_raise_in_begin  
test_return_from_ensure_after_raise_in_rescue