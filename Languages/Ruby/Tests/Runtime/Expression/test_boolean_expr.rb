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

# page: 341

require '../../util/assert.rb'

# globals false/nil are treated being false in a boolean context; all other values are treated as being true
def test_as_if_condition
    def being_self(x)
        if x then $g = 1 else $g = -1 end
    end 
    def being_self_not(x)
        if not x then $g = 1 else $g = -1 end 
    end 
    
    $g = 0; being_self(nil); assert_equal($g, -1); 
    $g = 0; being_self(false); assert_equal($g, -1)
       
    $g = 0; being_self_not(nil); assert_equal($g, 1)
    $g = 0; being_self_not(false); assert_equal($g, 1)

    $g = 0; being_self(true); assert_equal($g, 1)
    $g = 0; being_self(1); assert_equal($g, 1)
    $g = 0; being_self("hello"); assert_equal($g, 1)
    $g = 0; being_self(/abc/); assert_equal($g, 1)
    $g = 0; being_self(1..3); assert_equal($g, 1)

    $g = 0; being_self_not(true); assert_equal($g, -1)
    $g = 0; being_self_not("hello"); assert_equal($g, -1)
end 

# and
def test_and
    assert_equal( (true and true), true)
    assert_equal( (true and false), false)
    assert_equal( (false and true), false)
    assert_equal( (false and false), false)

    # 2nd expression is not evaluated if 1st is false
    assert_equal( (false and divide_by_zero), false)

    assert_equal( (true and true and false), false)
end 

# or
def test_or
    assert_equal( (true or true), true)
    assert_equal( (true or false), true)
    assert_equal( (false or true), true)
    assert_equal( (false or false), false)

    # 2nd expression is not evaluated if 1st is true
    assert_equal( (true or divide_by_zero), true)

    assert_equal( (false or false or true), true)
end

# not
def test_not
    assert_equal( (not nil), true)
    assert_equal( (not true), false)
    assert_equal( (not 12), false)
end 

# todo
# precedence
# defined?

test_as_if_condition
test_and
test_or
test_not