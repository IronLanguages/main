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

def test_always_next
    $x = $y = 0
    bools = [true, true, true, false]
    while bools[$x]
        $x += 1 
        next                  # always next
        $y += 1
    end 
    assert_equal($x, 3)
    assert_equal($y, 0)
end 

def test_next_in_the_middle
    $x = $y = 0
    bools = [true, true, true, false]
    while bools[$x]
        $x += 1 
        next if $x == 2       # next in the middle loop
        $y += 1
    end 
    assert_equal($x, 3)
    assert_equal($y, 2)
end 

def test_next_at_last
    $x = $y = 0
    bools = [true, true, true, false]
    while bools[$x]
        $x += 1 
        next if $x == 3       # next at the last loop
        $y += 1
    end 
    assert_equal($x, 3)
    assert_equal($y, 2)
end 

# "next" may take optional arguments

# the values are ignored 
def test_next_with_arg
    $x = 0
    $y = while [true, false][$x]
        $x += 1
        next 1, 2
    end 
    assert_equal($x, 1)
    assert_equal($y, nil)
end 

# whether they do get evaluated
def test_evaluate_args
    $x = 0
    a, b = 1, 2
    while [true, false][$x]
        $x += 1
        next a += b, b += a
    end 
    assert_equal(a, 3)
    assert_equal(b, 5)
end

# should next to the inner loop
def test_nested_loop
    $x = $y = 0
    $g = 1
    while [true, true, false][$x]
        $x += 1
        $g += 10
        while [true, false][$y]
            $y += 1
            $g += 100    
            next
            $g += 1000
        end 
        $g += 10000
    end 
    
    assert_equal($g, 20121)
end 

test_always_next
test_next_in_the_middle
test_next_at_last
test_next_with_arg
test_evaluate_args
test_nested_loop