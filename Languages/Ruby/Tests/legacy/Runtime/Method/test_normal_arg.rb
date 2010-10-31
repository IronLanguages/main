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

# no arg required
def test_0_arg
    def m
        (9)  # parenthesis around 9 - ok
    end 

    assert_return(9) { m }
    assert_raise(ArgumentError) { m 1 }
    assert_raise(ArgumentError) { m nil }
    assert_raise(ArgumentError) { m [] }
    assert_return(9) { m() }

    # expanding array
    assert_return(9) { m(*[]) }
    assert_raise(ArgumentError) { m(*nil) } 
    assert_raise(ArgumentError) { m(*[nil]) }
    assert_raise(ArgumentError) { m(*[1]) }
end 

# one arg required
def test_1_arg
    def m (arg)
        arg
    end 

    assert_raise(ArgumentError) { m }
    assert_return(99) { m 99 }
    assert_return(nil) { m nil }
    assert_return([]) { m [] }
    assert_raise(ArgumentError) { m 1, 2 }

    # expanding array
    assert_raise(ArgumentError) { m *[] }
    assert_return(78) { m *[78] }
    assert_return(79) { m *79 }
    assert_return("b") { m *"b" }
    assert_return(nil) { m *[nil] }
    assert_return(nil) { m *nil }

    assert_raise(ArgumentError) { m *[80, 81] }
    assert_return([82, 83]) { m *[[82, 83]] }
end 

# several args required
def test_5_args
    def m arg1, arg2, arg3, arg4, arg5
        [arg1, arg2, arg3, arg4, arg5]
    end 

    assert_raise(ArgumentError) { m }
    assert_raise(ArgumentError) { m 1 }
    assert_raise(ArgumentError) { m 1, 2, 3, 4 }
    assert_raise(ArgumentError) { m 1, 2, 3, 4, 5, 6 }

    # expanding array
    assert_return([5,6,7,8,9]) { m *[5,6,7,8,9] }
    assert_return([5,6,7,8,9]) { m 5, *[6,7,8,9] }
    assert_return([5,6,7,8,9]) { m 5,6,7,8,*[9] }
    assert_return([5,6,7,8,9]) { m 5,6,7,8,9,*[] }

    assert_raise(ArgumentError) { m 1, *[2, 3] }
    assert_raise(ArgumentError) { m 1, *[2, 3, 4, 5, 6] }
    # syntax error if nested expanding, more than 1 expanding, or not stay at last 
end 

test_0_arg
test_1_arg
test_5_args