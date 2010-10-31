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

# also what could we pass to the block argument? it is necessary
# the rule is: 
#    - &arg is optional, either specify it or skip it.
#    - proc is not taken as block argument, &proc works


def method
    block_given?
end 

empty = lambda {}

assert_equal(method, false)
x = method {}
assert_equal(x, true)
x = method(&empty)
assert_equal(x, true)
assert_raise(ArgumentError) { method(empty) }


def method_with_1_arg arg
    block_given?
end 

assert_equal(method_with_1_arg(1), false)
x = method_with_1_arg(1) {}
assert_equal(x, true)
x = method_with_1_arg(1, &empty)
assert_equal(x, true)

assert_raise(ArgumentError) { method_with_1_arg(1, empty) }
assert_equal(method_with_1_arg(empty), false)

def method_with_explict_block &p
    l = 1
    if p == nil
        l += 10
    else
        l += 100
    end 
    if block_given?
        l += 1000
    else
        l += 10000
    end
    l
end 

assert_equal(method_with_explict_block, 10011)
assert_raise(ArgumentError) { method_with_explict_block(1) }
x = method_with_explict_block {}
assert_equal(x, 1101)
x = method_with_explict_block(&empty)
assert_equal(x, 1101)
assert_raise(ArgumentError) { method_with_explict_block(empty) }

