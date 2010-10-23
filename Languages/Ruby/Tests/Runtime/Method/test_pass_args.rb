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

def m1(arg); arg; end 
def m3(arg1, arg2, arg3); return arg1, arg2, arg3; end

# different types of value as arg
def test_arg_type
    assert_return(3) { m1((3)) }

    assert_return(6) { m1(any_arg = 6) } # no keyword argument
    assert_raise(NameError) { any_arg }

    assert_return(5) { m1(arg = 5) } 
    assert_raise(NameError) { arg }  

    assert_return({'arg' => 3 }) { m1('arg' => 3) }
    assert_return({'arg' => 3 }) { m1({'arg' => 3}) }

    assert_return({:arg => 3 }) { m1(:arg => 3) }
    assert_return({:arg => 3 }) { m1({:arg => 3}) }
end 


# order to evaluate the arg
def test_evaluate_order
    x  = 0
    assert_return([1, 11, 111]) { m3(x+=1, x+=10, x+=100) }
end

def test_raise_when_evaluating
    x = 0
    assert_raise(ZeroDivisionError) { m3(x+=1, (divide_by_zero;x+=10), x+=100) }
    assert_equal(x, 1)
end 

#test_arg_type
test_evaluate_order
test_raise_when_evaluating
