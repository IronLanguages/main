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

# set the variable or attribute on its left side (the lvalue) to refer to the value
# on the right (the rvalue). 

a = 3
assert_equal(a, 3)

a = nil
assert_nil(a)

a = 4.5
assert_equal(a, 4.5)

def f; -10; end 
a = f
assert_equal(a, -10)

a = []
assert_equal(a, [])

a = *[]
assert_nil(a)

a = [5.78]
assert_equal(a, [5.78])

a = *[7]
assert_equal(a, 7)

a = *[8, 9]
assert_equal(a, [8, 9])

# multiple rvalues, convert to array
a = 7, 9
assert_equal(a, [7, 9])

a = nil, 8
assert_equal(a, [nil, 8])

a = nil, nil
assert_equal(a, [nil, nil])

# It then returns that value as the result of the assignment expression

a = b = 10
assert_equal(a, 10)
assert_equal(b, 10)

a = b = 20 + 30
assert_equal(a, 50)
assert_equal(b, 50)

a = (b = 30 + 40) + 50
assert_equal(a, 120)
assert_equal(b, 70)

# order!
a = b = 99, 100
assert_equal(a, [99, 100])
assert_equal(b, 99)

a = (b = 101, 102)
assert_equal(a, [101, 102])
assert_equal(b, [101, 102])

s = "hello world"
assert_equal(s, "hello world")

# object attribute or element reference (todo)
s[0] = 'H'
assert_equal(s, "Hello world")
