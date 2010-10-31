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

# recursive require

# good
assert_true { require "module_a_require_b.rb" }
assert_false { require "module_b_require_a.rb" }
assert_equal([A::X, B::Y, A::Z], [100, 200, 300])

# bad
assert_raise(NameError) { require "module_c_require_d" }
assert_false { require "module_d_require_c" }   ## ??
assert_raise(NameError) { C }
D                                               ## ??
assert_raise(NameError) { D::Y }                ## ??


# reach the same module


