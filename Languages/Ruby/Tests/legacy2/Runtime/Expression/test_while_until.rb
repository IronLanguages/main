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

# page 344

require '../../util/assert.rb'

# executes body zero or more times as long as bool-expression is true (for while), false (for until)

def test_zero_times
    g = 1
    x = while false
        g += 10
    end
    assert_equal(g, 1)
    assert_nil(x)
    
    y = until true 
        g += 100
    end 
    assert_equal(g, 1)
    assert_nil(y)
end 

def test_more_times
    x = true
    g = 0
    a = while x
        g += 1
        if g > 2
            x = false
        end
    end 
    assert_equal(g, 3)
    assert_nil(a)
    
    x = false
    g = 0
    b = until x
        g += 1
        if g > 5
            x = true
        end
    end 
    assert_equal(g, 6)
    assert_nil(b)
end

# while and until modifier
def test_modifer
    # bad thing won't happen
    divide_by_zero while false
    divide_by_zero until true
    
    #
    x = 0 
    x += 1 while x > 5
    assert_equal(x, 0)
    x = 0
    x += 1 while x < 5
    assert_equal(x, 5)
    
    x = 0
    x += 1 until x > 5
    assert_equal(x, 6)
    x = 0
    x += 1 until x < 5
    assert_equal(x, 0)
end

def test_evaluate_condition
    g = c = 1 
    x = true
    while (g+=10; x)
        c += 1
        x = c < 3
    end
    assert_equal(g, 31)
    
    g = c = 1 
    x = false
    until (g+=10; x)
        c += 1
        x = c > 3
    end
    assert_equal(g, 41)    
end 

# TODO
def test_expr_result
    x = while true; return 3; end 
    assert_equal(x, 3)
    x = while true; break 4; end
    assert_equal(x, 4)
    
    
end 

test_zero_times
test_more_times
test_modifer
test_evaluate_condition
test_expr_result
