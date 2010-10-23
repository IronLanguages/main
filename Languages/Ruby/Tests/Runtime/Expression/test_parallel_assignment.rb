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

def test_basic
    a, b = 4, 5
    assert_equal(a, 4)
    assert_equal(b, 5)
end     

def test_swap
    a, b = 4, 5
    # swap
    a, b = b, a
    assert_equal(a, 5)
    assert_equal(b, 4)
end 

# the values on the right side are evaluated in the order in which they apprear before any assignment
# is made to variables or attributes on the left

def test_eval_order
    x = 1
    a, b = x, x+=2
    assert_equal(a, 1)
    assert_equal(b, 3)

    a, b = x+=2, x
    assert_equal(a, 5)
    assert_equal(b, 5)
end 

# same variable
def test_assign_to_same_var
    a, a = 4, 7
    assert_equal(a, 7)

    a = 10
    a, a = 6, a+9
    assert_equal(a, 19)
end

# when an assignment has more than one lvalue, the assignment expression returns an array of the rvalues

def test_unmatch
    # if an assignment contains more lvalues than rvalues, the excess lvalues are set to nil
    a, b, c = 12
    assert_equal(a, 12)
    assert_nil(b)
    assert_nil(c)

    # if the assignment contains more rvalues than lvalues, the extra rvalues are ignored.

    a, b = 3, 4, 5
    assert_equal(a, 3)
    assert_equal(b, 4)
end 

# if an assignment has just one lvalue and multiple rvalues, the rvalues are converted to an array and
# assigned to the lvalue: see Tests\Runtime\Expression\test_assignment.rb.

#
# collapse/expand arrays
#
# if the last lvalue is preceded by *, all the remaining rvalues will be collected and assigned to that
# lvalue as an array.
    
def test_left_star
    x = [3, 5, 7, 9]
    
    *b = x
    assert_equal(b, [[3, 5, 7, 9]])

    a, *b = x
    assert_equal(a, 3)
    assert_equal(b, [5, 7, 9])

    *b = x, 20
    assert_equal(b, [[3, 5, 7, 9], 20])

    a, *b = x, 20
    assert_equal(a, [3, 5, 7, 9])
    assert_equal(b, [20])

    *b = 30, x
    assert_equal(b, [30, [3, 5, 7, 9]])

    a, *b = 30, x
    assert_equal(a, 30)
    assert_equal(b, [[3, 5, 7, 9]])
end 

# similarly, if the last rvalue is an array, you can prefix it with *, which effectively expands it into
# its constituent values in place.
def test_right_star
    x = [3, 5, 7, 9]
    
    b = *x
    assert_equal(b, [3, 5, 7, 9])

    a, b = *x
    assert_equal(a, 3)
    assert_equal(b, 5)

    b = 40, *x
    assert_equal(b, [40, 3, 5, 7, 9])

    a, b = 40, *x
    assert_equal(a, 40)
    assert_equal(b, 3)
end 

## a, b = *x, 40, syntax error

# both last rvalue/lvalue have *
def test_both_side_star
    x = [3, 5, 7, 9]

    *b = *x
    assert_equal(b, [3, 5, 7, 9])

    *b = 40, *x
    assert_equal(b, [40, 3, 5, 7, 9])

    a, *b = *x
    assert_equal(a, 3)
    assert_equal(b, [5, 7, 9])

    a, *b = 50, *x
    assert_equal(a, 50)
    assert_equal(b, [3, 5, 7, 9])

    # this (having * ahead of rvalue) is not necessary if the rvalue is the only thing in the right side.

    a, b = x
    assert_equal(a, 3)
    assert_equal(b, 5)
end     

def test_eval_order2
    def f
        $g += 10
        $a
    end 
    
    # simple assignment
    $g = 1
    $x = $y = $z = 0
    $a = [1, 2, 3]

    f[begin $g+=100; $x = $g; 0 end] = ($g+=1000; $y = $g; 5)
    assert_equal($a, [5, 2, 3])
    assert_equal($x, 111)
    assert_equal($y, 1111)
    
    # a,b = 1
    $g = 1
    $x = $y = $z = 0
    $a = [1, 2, 3]
    f[begin $g+=100; $x = $g; 0 end], f[begin $g+=1000; $y = $g; 1 end] = ($g+=10000; $z = $g; 5)
    
    assert_equal($a, [5, nil, 3])
    assert_equal($z, 10001)     # z -> x -> y
    assert_equal($x, 10111)
    assert_equal($y, 11121)    
    
    # (a,) = 1
    $g = 1
    $x = $y = $z = 0
    $a = [1, 2, 3]
    (f[begin $g+=100; $x = $g; 0 end],) = ($g+=1000; $y = $g; 5)
    assert_equal($a, [5, 2, 3])
    assert_equal($y, 1001)
    assert_equal($x, 1111)
end 

def test_exception
    # throw while eval'ing right side
    $x = $y = $z = $w = 0
    begin $x, $y, $z = 1, divide_by_zero, 3; rescue; $w=1; end 
    assert_equal([$x, $y, $z, $w], [0, 0, 0, 1])
    
    # throw while eval'ing left side
    $x = $y = $z = 0
    def f; divide_by_zero; end 
    begin $x, f[2], $y = 1, 2, 3; rescue; $z =1; end
    assert_equal([$x, $y, $z], [1, 0, 1])
    
    $x, f, $y = 4, 5, 6  # f is variable
    assert_equal(f, 5)
end 

test_basic
test_swap
test_eval_order
test_assign_to_same_var
test_unmatch
test_left_star
test_right_star
test_both_side_star
test_eval_order2
test_exception
