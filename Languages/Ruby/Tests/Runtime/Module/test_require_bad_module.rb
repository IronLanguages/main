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

assert_raise(LoadError) { require "not_existing.rb" }
assert_raise(LoadError) { require "not_existing" }

assert_raise(SyntaxError) { require "module_with_syntax_error" }
# require again the same file, no exception raised
assert_false { require "module_with_syntax_error" }
assert_raise(NameError) { variable_in_module_with_syntax_error_one } 
assert_raise(NameError) { variable_in_module_with_syntax_error_two } 

assert_raise(ZeroDivisionError) { require "module_with_divide_by_zero" }
assert_false { require "module_with_divide_by_zero" }
assert_equal(Module_With_Divide_By_Zero::CONST_ONE, 1)
assert_raise(NameError) { Module_With_Divide_By_Zero::CONST_TWO } 

assert_raise(ZeroDivisionError) { require "module_require_unloaded_bad_module" }
assert_equal(CONST_X, 77)
assert_raise(NameError)  { CONST_Y }

require "module_require_loaded_bad_module"
assert_equal(CONST_U, 83)
assert_equal(CONST_V, 94)

