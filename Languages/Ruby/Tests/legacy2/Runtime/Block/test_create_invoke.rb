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
require 'block_common.rb'

def test_0_arg
    b = lambda { 1 }
    assert_equal(b.call, 1)
    assert_equal(b.call(1, 2, 4), 1)
#    assert_raise(ArgumentError) { b.call(1) }
    
    b = lambda { divide_by_zero }
    assert_raise(ZeroDivisionError) { b.call }
end    

def test_1_arg
    b = lambda { |x| x }
    assert_equal(b.call(9), 9)
    b.call(3, 4)  # warning
end 

test_0_arg
test_1_arg