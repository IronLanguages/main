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

# more like unit test, see Compat/test_ctrl_flow for more complete coverage
def test_hit_break
    def my_break(gate)
        $g = 1
        while true
            $g += 10
            if $g > gate
                $g += 100
                break
                $g += 1000
            end 
            $g += 10000
        end 
    end 

    my_break(10); assert_equal($g, 111)
    my_break(200); assert_equal($g, 10121)
    my_break(20000); assert_equal($g, 20131)
end 

# never hit break
def test_break_not_hit
    $g = 1
    while $g < 40
        $g += 10
        break if $g >= 50
        $g += 100
    end
    assert_equal($g, 111)
end

# break and next may optionally take one or more arguments
# if used within a block, they are returned as the value of the yield.
# if used within while/until/for, the value given to break is returned, to next is ignored,.

def test_break_with_arg
    x = while true
        break 
    end 
    assert_equal(x, nil)

    x = while true
        break 1
    end 
    assert_equal(x, 1)

    x = while true
        break 2, 3
    end 
    assert_equal(x, [2, 3])

    a, b = 1, 2
    x = while true
        break a+=b, b+=a
    end 
    assert_equal(x, [3, 5])
end 

# only break the inner loop
def test_nested_loop
    $g = 1
    x = 0
    while x < 5
        x += 1
        $g += 10
        while true
            $g += 100
            break
            $g += 1000
        end 
        $g += 10000
    end 
    assert_equal($g, 50551)
end 

test_hit_break
test_break_not_hit
test_break_with_arg
test_nested_loop
