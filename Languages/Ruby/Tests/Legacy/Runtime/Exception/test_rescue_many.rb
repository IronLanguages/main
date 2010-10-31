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

# first in the row
def test_first_match
    $g = 1
    begin
        divide_by_zero
    rescue ZeroDivisionError, NameError, RuntimeError
        $g += 10
    end
    assert_equal($g, 11)
end 

# second in the row
def test_second_match
    $g = 1
    begin
        divide_by_zero
    rescue NameError, ZeroDivisionError, RuntimeError
        $g += 10
    end
    assert_equal($g, 11)
end 

# last in the row
def test_last_match
    $g = 1
    begin
        divide_by_zero
    rescue NameError, RuntimeError, ZeroDivisionError
        $g += 10
    end
    assert_equal($g, 11)
end 

# not in the row
def test_no_match
    $g = 1
    def f
        begin
            divide_by_zero
        rescue NameError, RuntimeError, TypeError
            $g += 10
        end
    end 

    begin 
        f
    rescue ZeroDivisionError
        $g += 100
    end

    assert_equal($g, 101)
end 

# rescued by parent exception
def test_match_to_parent
    $g = 1
    begin 
        divide_by_zero
    rescue NameError, StandardError
        $g += 10
    end 
    assert_equal($g, 11)
end

# exact exception and its parent in the same row. parent first
def test_parent_before_exact_match
    $g = 1
    begin 
        divide_by_zero
    rescue NameError, StandardError, ZeroDivisionError
        $g += 10
    end 
    assert_equal($g, 11)
end 

# exact exception and its parent in the same row. parent last
def test_exact_match_before_parent
    $g = 1
    begin 
        divide_by_zero
    rescue NameError, ZeroDivisionError, StandardError
        $g += 10
    end 
    assert_equal($g, 11)
end 

# order of the parm get evaluated, and when?

# rescue does not happen, the parm should not be evaluated. 
def test_no_need_rescue
    $g = 1
    begin 
        empty_func
    rescue (l = 10, NameError)
        $g += 10
    end
    assert_equal($g, 1)
#    assert_nil(l)
end 

# rescue does happen
def test_parm_evaluate_order
    $g = 1
    $g1, $g2, $g3 = 10, 100, 1000

    begin 
        divide_by_zero
    rescue ($g1 = $g2 + 1; NameError), ($g2 = $g3 + 1; ZeroDivisionError), ($g3 = $g1 + 1; TypeError)
        $g += 10
    rescue ($g += 100; StandandError)
        $g += 100
    end 
    assert_equal($g, 11)
    assert_equal($g1, 101)
    assert_equal($g2, 1001)
    assert_equal($g3, 102)  # if the 3rd is not evaluated, $g3 would remain 1000
end 

# multiple rescue clauses
def test_multiple_clauses
    $g = 1
    begin 
        divide_by_zero
    rescue NameError, TypeError
        $g += 10
    rescue TypeError, ZeroDivisionError # TypeError is repeated
        $g += 100
    end 
    assert_equal($g, 101)
end 

# rescue parm => var! need more
def test_assign_to_var
    $g = 1
    new_var = nil
    begin
        divide_by_zero
    rescue TypeError, ZeroDivisionError => new_var
        $g += 10
    end
    assert_equal($g, 11)
    assert_isinstanceof(new_var, ZeroDivisionError)
end 

def test_raise_from_param_without_ensure
    $g = 1
    def f
        $g += 10
        raise "something"
    end 

    begin 
        begin 
            f
        rescue TypeError, f, f
            $g += 100
        end 
    rescue RuntimeError
        $g += 1000
    end
    assert_equal($g, 1021)
end 

def test_raise_from_param_with_ensure
    $g = 1
    def f
        $g += 10
        raise "something"
    end 
    
    begin 
        begin 
            f
        rescue TypeError, f, f
            $g += 100
        ensure 
            $g += 1000
        end 
    rescue RuntimeError   
        $g += 10000
    end
    assert_equal($g, 11021)
end 

test_first_match
test_second_match
test_last_match
test_no_match
test_match_to_parent
test_parent_before_exact_match
test_exact_match_before_parent
test_no_need_rescue
test_parm_evaluate_order
test_multiple_clauses
test_assign_to_var
test_raise_from_param_without_ensure
test_raise_from_param_with_ensure