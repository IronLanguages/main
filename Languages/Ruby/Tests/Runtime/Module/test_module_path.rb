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

# "require" load any given file only once
$global_var = 88
assert_true { require "module_with_global.rb" }
assert_equal($global_var, 90)
assert_false { require "module_with_global" }
assert_equal($global_var, 90)
assert_true { require "module_with_Global" }  # cap "G"
assert_equal($global_var, 92)
assert_true { require "./module_with_global" }  # ??
assert_equal($global_var, 94)

# page: 124
# accept relative and absolute paths; if given a relative path (or just a plain name), 
# they will search every directory in the current load path ($:) for the file

# todo