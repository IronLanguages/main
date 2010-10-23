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

def test_not_iterator
    assert_raise(NoMethodError) { for var1 in 1 do end  }
end 

def test_basic
    $g = 0
    for var1 in [1]
        $g += var1
    end 
    assert_equal($g, 1)

    $g = 0
    for var1 in [2, 3]
        $g += var1
    end 
    assert_equal($g, 5)
end 

def test_assign
    c = d = 0
    for var1, var2 in [5, 6] 
        c += var1
        assert_nil(var2)
    end 
    assert_equal(c, 11)

    e = f = 0
    for var1, var2 in [ 7, [8, 9], [10, 11, 12] ]
        e += var1
        if var1 == 7
            assert_nil(var2)
        else 
            f += var2
        end 
    end 
    assert_equal(e, 25)
    assert_equal(f, 20)
end 

# the local variables defined in the body of the for loop will be available outside the loop
def test_local_variable
    for var1 in [1, 2]
        var3 = 100
    end 
    assert_equal(var3, 100)
end 

def test_expr
    expr = [1, 10, 100]
    $g = 0
    for var1 in expr
        $g += var1
        expr = [1000, 10000]  # no impact
    end 
    assert_equal($g, 111)
    assert_equal(expr, [1000, 10000])
end 

test_not_iterator
test_basic
test_assign
test_local_variable
test_expr