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

assert_equal(self.to_s, "main")
assert_equal(self.class, Object)

module Simple
    assert_equal(self.to_s, "Simple")
    assert_equal(self.class, Module)
    
    class My
        assert_equal(self.to_s, "Simple::My")
        assert_equal(self.class, Class)
        
        def My.check
            assert_equal(self.to_s, "Simple::My")
            assert_equal(self.class, Class)
        end 
        
        def check
            assert_true { self.to_s.include? "#<Simple::My" }
            assert_equal(self.class, My)
        end 
    end
end 


Simple::My.check

x = Simple::My.new
x.check

class << x
    #assert_true { self.to_s.include? "#<Class:#<Simple::My:" }   (15652)
    assert_equal(self.class, Class)
end    

def x.method
    assert_true { self.to_s.include? "#<Simple::My:" }  # object x
    assert_equal(self.class, Simple::My)
end 
x.method