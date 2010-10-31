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

# only one arg, and it is vararg
def test_1_vararg
    def m(*args)
        args
    end

    assert_return([]) { m }
    assert_return([1]) { m 1 }
    assert_return([2, 3]) { m 2, 3 }
    assert_return([[4]]) { m [4] }
    assert_return([5, [6]]) { m 5, [6] }
    assert_return([[]]) { m(m) }

    # expanding array as arg
    assert_return([]) { m *[] }
    assert_return([1]) { m *[1,] }
    assert_return([1, 2, 3]) { m 1, *[2, 3,] }
end 

# two args, the last one is vararg
def test_1_normal_1_vararg
    def m(arg1, *arg2) 
        [arg1, arg2]
    end

    assert_raise(ArgumentError) { m }
    assert_return([1, []]) { m 1 }
    assert_return([2, [3]]) { m 2, 3 }
    assert_return([4, [5, 6]]) { m 4, 5, 6 }
    assert_return([7, [[8, 9]]]) { m 7, [8, 9] }
    assert_return([[10], [11]]) { m [10], 11 }

    # expanding array as arg
    assert_raise(ArgumentError) { m *[] }
    assert_return([1, []]) { m *[1,] }

    assert_return([1, [2, 3]]) { m *[1, 2, 3] }
    assert_return([1, [2, 3]]) { m 1, *[2, 3] }
    assert_return([1, [2, 3]]) { m 1, 2, *[3] }
end 

test_1_vararg
test_1_normal_1_vararg