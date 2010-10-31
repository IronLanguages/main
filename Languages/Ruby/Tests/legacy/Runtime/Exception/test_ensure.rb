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

# no exception happen in try-block
def test_no_exception
    $g = 1
    begin
        empty_func
    ensure 
        $g += 10
    end 
    assert_equal($g, 11)
end 

# exception does happen in try-block, and get rescued
def test_exception_raised_and_rescued
    $g = 1
    begin 
        divide_by_zero  
    rescue ZeroDivisionError
        $g += 10
    ensure 
        $g += 100
    end 
    assert_equal($g, 111)
end 

# exception does happen in try-block, and does not get rescued
def test_exception_raised_but_not_rescued
    $g = 1
    def f
        begin 
            divide_by_zero
        ensure
            $g += 10
        end
    end 

    begin 
        f
    rescue ZeroDivisionError
        $g += 100
    end 

    assert_equal($g, 111)
end

# exception happen in rescue block, we should still hit ensure block
def test_exception_raised_in_rescue
    $g = 1
    def f
        begin 
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
            raise "string"
        ensure 
            $g += 100
        end 
    end 

    begin 
        f
    rescue RuntimeError
        $g += 1000
    end
    
    assert_nil($!)
    assert_equal($g, 1111)
end 

# exception happen in else block, we should still hit ensure block
def test_exception_raised_in_else
    $g = 1
    def f
        begin 
            empty_func
        rescue ZeroDivisionError
            $g = 10
        else
            $g += 100
            raise "string"
            $g += 100
        ensure 
            $g += 1000
        end 
    end 

    begin
        f
    rescue RuntimeError
        $g += 10000
    end
    
    assert_equal($g, 11101)
end 

# exception happen in ensure block
def test_exception_raised_in_ensure
    $g = 1
    def f
        begin 
            empty_func
        ensure
            $g += 10
            raise "string"
            $g += 100
        end 
    end 

    begin 
        f
    rescue RuntimeError
        $g += 1000
    end 
    
    assert_nil($!)
   
    assert_equal($g, 1011)
end


def test_exception_raised_in_ensure2
    begin
        begin
            raise "3"
        ensure
            raise "4"
        end
    rescue
    ensure
        assert_nil($!)
    end
end

test_exception_raised_in_ensure2

# exception thrown in parm
def test_exception_raised_implicitly_when_evaluating_rescue
    $g = 1
    def f
        begin 
            divide_by_zero
        rescue "abc"   # raise here
            $g += 10
        ensure 
            $g += 100
        end 
    end
    begin 
        f
    rescue TypeError
        $g += 1000
    end 
    
    assert_equal($g, 1101)
end 

def test_exception_raised_explicitly_when_evaluating_rescue
    $g = 1
    def f
        begin 
            1 + "1"   # raise TypeError  
        rescue (1/0)  # raise ZeroDivisionError 
            $g += 10
        ensure 
            assert_isinstanceof($!, ZeroDivisionError)
            $g += 100
        end 
    end
    begin 
        f
    rescue ZeroDivisionError
        $g += 1000
    end
    
    assert_nil($!)
    
    assert_equal($g, 1101)
end 

test_no_exception
test_exception_raised_and_rescued
test_exception_raised_but_not_rescued
test_exception_raised_in_rescue
test_exception_raised_in_else
test_exception_raised_in_ensure
test_exception_raised_implicitly_when_evaluating_rescue
test_exception_raised_explicitly_when_evaluating_rescue