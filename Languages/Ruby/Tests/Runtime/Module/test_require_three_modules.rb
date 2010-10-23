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

# page: 117
# a way of grouping together methods, classes, and constants

# module define a namespace

require "module_a"

assert_raise(NoMethodError) { Pacific.SIZE }
assert_equal(Pacific.listen, 101)
assert_raise(NoMethodError) { Pacific.Ocean }
assert_raise(NoMethodError) { Pacific.Hawaii }

assert_equal(Pacific::SIZE, 100)
assert_equal(Pacific::listen, 101)
assert_equal(Pacific::Ocean::FLAG, 102)
assert_equal(Pacific::Hawaii::KIND, 103)

assert_equal(TOP_SIZE, 200)
assert_equal(top_method, 201)
assert_equal(Top_Class::FLAG, 202)
#assert_raise(NameError) { var }

require "module_b"

# unchanged
assert_equal(Pacific::SIZE, 100)
assert_equal(Pacific::listen, 101)
assert_equal(Pacific::Ocean::FLAG, 102)
assert_equal(Pacific::Hawaii::KIND, 103)

# new 
assert_equal(Pacific::SIZE2, 300)
assert_equal(Pacific::listen2, 301)
assert_equal(Pacific::Ocean::FLAG2, 302)
assert_equal(Pacific::Hawaii::KIND2, 303)

# unchanged
assert_equal(TOP_SIZE, 200)
assert_equal(top_method, 201)
assert_equal(Top_Class::FLAG, 202)
#assert_raise(NameError) { var }

# new
assert_equal(TOP_SIZE2, 400)
assert_equal(top_method2, 401)
assert_equal(Top_Class2::FLAG, 402)
#assert_raise(NameError) { var2 }

require "module_c"  # runtime warning: already initialized constant

# unchanged
assert_equal(Pacific::SIZE2, 300)
assert_equal(Pacific::listen2, 301)
assert_equal(Pacific::Ocean::FLAG2, 302)
assert_equal(Pacific::Hawaii::KIND2, 303)

# changed
assert_equal(Pacific::SIZE, 500)
assert_equal(Pacific::listen, 501)
assert_equal(Pacific::Ocean::FLAG, 502)
assert_equal(Pacific::Hawaii::KIND, 503)

# changed
assert_equal(TOP_SIZE, 600)
assert_equal(top_method, 601)
assert_equal(Top_Class::FLAG, 602)
#assert_raise(NameError) { var }

# unchanged
assert_equal(TOP_SIZE2, 400)
assert_equal(top_method2, 401)
assert_equal(Top_Class2::FLAG, 402)
#assert_raise(NameError) { var2 }
