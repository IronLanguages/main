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

# argument contains default value, and var-arg list

require '../../util/assert.rb'

def test_defaultvalue_vararg
    def m(arg1=10, *arg2)
        [arg1, arg2]
    end 

    assert_return([10, []]) { m }
    assert_return([1, []]) { m 1 }
    assert_return([2, [3]]) { m 2, 3 }
    assert_return([4, [5, 6]]) { m 4, 5, 6 }
    assert_return([7, [8]]) { m *[7, 8] }
end 

def test_normal_defaultvalue_vararg
    def m(arg1, arg2=10, *arg3)
        [arg1, arg2, arg3]
    end

    assert_raise(ArgumentError) { m }
    assert_return([1, 10, []]) { m 1 }
    assert_return([2, 3, []]) { m 2, 3 }
    assert_return([4, 5, [6]]) { m 4, 5, 6 }
    assert_return([7, 8, [9, 10]]) { m 7, 8, 9, 10 }
    assert_return([11, 12, [13, 14, 15]]) { m *[11, 12, 13, 14, 15] }
end 


test_defaultvalue_vararg
test_normal_defaultvalue_vararg