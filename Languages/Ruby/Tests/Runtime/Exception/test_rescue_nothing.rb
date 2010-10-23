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

# rescue clause with no paramter is treated as if it had a parameter of StandardError

# ZeroDivisionError is StandardError
def test_rescue_zeroerror
    $g = 1
    begin 
        divide_by_zero
    rescue 
        $g += 10
    end 
    assert_equal($g, 11)
end 

# LoadError is NOT StandardError
def test_rescue_loaderror
    $g = 1
    def f
        begin 
            require "not_existing"
        rescue 
            $g += 10
        end 
    end 
    begin 
        f
    rescue LoadError
        $g += 100
    end     
    assert_equal($g, 101)
end 

# the behavior when such cluase in mutliple rescue sequences

# empty before ZeroDivisionError
def test_rescued_by_nothing_before_exact_match
    $g = 1
    begin 
        divide_by_zero
    rescue 
        $g += 10                        # hit
    rescue ZeroDivisionError
        $g += 100
    end 
    assert_equal($g, 11)
end

# empty after ZeroDivisionError
def test_rescued_by_exact_match_before_nothing
    $g = 1
    begin 
        divide_by_zero
    rescue ZeroDivisionError
        $g += 10                        # hit
    rescue 
        $g += 100
    end 
    assert_equal($g, 11)
end 

# empty after NameError
def test_rescued_by_nothing_after_no_match
    $g = 1
    begin 
        divide_by_zero
    rescue NameError
        $g += 10
    rescue 
        $g += 100
    end 
    assert_equal($g, 101)
end 

# empty before LoadError
def test_rescued_not_by_nothing_but_by_loaderror
    $g = 1
    begin 
        require "another_not_existing"
    rescue 
        $g += 10
    rescue LoadError
        $g += 100                       # hit
    end             
    assert_equal($g, 101)
end 

# rescue nothing twice, hit only once
def test_rescued_by_nothing_twice
    $g = 1
    begin 
        divide_by_zero
    rescue 
        $g += 10                       # hit
    rescue 
        $g += 100
    end             
    assert_equal($g, 11)
end

# rescue nothing but save the ex
def test_save_exception_to_variable
    begin 
        empty_func
    rescue => ex    # not raise
    end 
    #assert_nil(ex)  # bug 269526
    
    begin 
        divide_by_zero
    rescue => ex    # raised
    end
    assert_equal(ex.class, ZeroDivisionError)
end 

test_rescue_zeroerror
test_rescue_loaderror
test_rescued_by_nothing_before_exact_match
test_rescued_by_exact_match_before_nothing
test_rescued_by_nothing_after_no_match
test_rescued_not_by_nothing_but_by_loaderror
test_rescued_by_nothing_twice
test_save_exception_to_variable
