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

# exception handled by inner rescue (with else)
def test_handled_by_inner_rescue_with_else
    $g = 1
    begin 
        begin 
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
        else
            $g += 100
        end 
    rescue ZeroDivisionError
        $g += 1000
    else
        $g += 10000
    end

    assert_equal($g, 10011)
end 

# handled by inner rescue (no else)
def test_handled_by_inner_rescue_without_else
    $g = 1
    begin 
        begin 
            divide_by_zero
        rescue ZeroDivisionError
            $g += 10
        end 
    rescue ZeroDivisionError
        $g += 1000
    end

    assert_equal($g, 11)
end 

# handled by outer rescue (with else)
def test_handled_by_outer_rescue_with_else
    $g = 1
    begin 
        begin 
            divide_by_zero
        rescue TypeError
            $g += 10
        else
            $g += 100        
        end 
    rescue ZeroDivisionError
        $g += 1000
    else
        $g += 10000    
    end

    assert_equal($g, 1001)
end 

# handled by outer rescue (no else)
def test_handled_by_outer_rescue_without_else
    $g = 1
    begin 
        begin 
            divide_by_zero
        rescue TypeError
            $g += 10
        end 
    rescue ZeroDivisionError
        $g += 1000
    end

    assert_equal($g, 1001)
end

# exception throw by inner else
def test_raised_by_inner_else
    $g = 1
    begin 
        begin 
            empty_func
        rescue 
            $g += 10
        else
            $g += 100
            raise "string"
        end
    rescue RuntimeError
        $g += 1000
    end 
    assert_equal($g, 1101)
end 

# both ensure block should be hit
def test_ensure
    $g = 1
    begin
        begin 
            divide_by_zero
        rescue 
            $g += 10
        ensure 
            $g += 100
        end 
    rescue 
        $g += 1000
    ensure 
        $g += 10000
    end 
    assert_equal($g, 10111)
end


def test_raise_inside_ensure
    $g = 1
    def m
        begin 
            divide_by_zero
        ensure  
            assert_isinstanceof($!, ZeroDivisionError)
            begin 
                raise IOError.new
            ensure 
                assert_isinstanceof($!, IOError)
            end 
            $g += 10
        end 
     end 
     
     begin 
        m
     rescue 
        assert_isinstanceof($!, IOError)
     end 
     
     assert_equal($g, 1)
end 

def test_not_raise_inside_ensure
    $g = 1
    def m
        begin 
            divide_by_zero
        ensure  
            assert_isinstanceof($!, ZeroDivisionError)
            begin 
                1 + 1 # valid, not throw
            ensure 
                assert_isinstanceof($!, ZeroDivisionError)
            end 
            
            assert_isinstanceof($!, ZeroDivisionError)  #!!
            $g += 10
        end 
    end 
    
    begin 
        m
    rescue 
        assert_isinstanceof($!, ZeroDivisionError)
    end 
    
    assert_equal($g, 11)
end 

def test_not_raise_inside_ensure2
    $g = 1
    def m
        begin 
            divide_by_zero
        ensure  
            assert_isinstanceof($!, ZeroDivisionError)
            begin 
                1 + 1 # valid, not throw
            rescue 
                $g += 10
            else
                $g += 100
            ensure 
                $g += 1000
                assert_isinstanceof($!, ZeroDivisionError)
            end 
            
            assert_isinstanceof($!, ZeroDivisionError)  
            $g += 10000
        end 
    end 
    
    begin 
        m
    rescue 
        assert_isinstanceof($!, ZeroDivisionError)
    end 
    
    assert_equal($g, 11101)
end 


test_handled_by_inner_rescue_with_else
test_handled_by_inner_rescue_without_else
test_handled_by_outer_rescue_with_else
test_handled_by_outer_rescue_without_else
test_raised_by_inner_else
test_ensure

test_raise_inside_ensure
test_not_raise_inside_ensure
test_not_raise_inside_ensure2