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

# basic usage of methods in class

class My_methods
    def My_methods.sm; 10; end
    def im; 20; end
end   

x = My_methods.new

assert_equal(x.im, 20)
assert_equal(x::im, 20)
assert_equal(My_methods.sm, 10)
assert_equal(My_methods::sm, 10)

assert_raise(NoMethodError) { x.sm }
assert_raise(NoMethodError) { My_methods.im }

class My_methods_with_same_name
    def My_methods_with_same_name.m; 10; end
    def m; 20; end
end

x = My_methods_with_same_name.new
assert_equal(My_methods_with_same_name.m, 10)
assert_equal(x.m, 20)


# override method from mixin


# special methods: ==, eql?

class C600
    def initialize x; @x = x; end 
    def x; @x; end 
    def ==(other); @v == other.x; end 
end 
a, b = C600.new(2), C600.new(2)
hash = {}

#hash[a] = 3
#hash[b] = 4
#assert_equal(hash[a], 3)
class C600
    alias eql? ==
end 
hash[a] = 3
hash[b] = 4

print hash[a]
print hash[b]
#assert_equal(hash[a], 3)
