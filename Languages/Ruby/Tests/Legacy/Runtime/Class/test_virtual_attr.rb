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

class My
    def virtual_attribute
        @actual_attribute / 2
    end 
    
    def virtual_attribute=(new)
        @actual_attribute = new * 2
    end 
   
end 

x = My.new
assert_raise(NoMethodError) { x.virtual_attribute } # undefined method `/' for nil:NilClass (NoMethodError)

x.virtual_attribute = 10
assert_equal(x.virtual_attribute, 10)