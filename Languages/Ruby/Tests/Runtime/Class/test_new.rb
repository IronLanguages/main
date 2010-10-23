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

# The visibility of methods named initialize is automatically made private
class My_with_explicit_public_initialize
    public                          # even with public "explicitly" here
    def initialize
    end 
end  
#assert_raise(NoMethodError) { My_with_explicit_public_initialize.new.initialize } 

class My_with_implicit_public_initialize
    def initialize
    end 
end  
#assert_raise(NoMethodError) { My_with_implicit_public_initialize.new.initialize } 
assert_raise(ArgumentError) { My_with_implicit_public_initialize.new 1 }

# self defined "new", now you have no way to instantiate such object
class My_with_new
    def My_with_new.new; 90; end
end
assert_equal(My_with_new.new, 90)

# new with block args
class My_with_initialize
    def initialize x, y
        @x = x
        @y = y
        @z = yield
    end
    def check
        [@x, @y, @z]
    end 
end 

x = My_with_initialize.new(*[1, 2]) { 3 }
assert_equal(x.check, [1, 2, 3])
assert_raise(ArgumentError) { My_with_initialize.new 1 }

# call superclass's initialize

class My_base
    def initialize
        @x = 10
        @y = 20
    end 
    def check
        [@x, @y, @z]
    end     
end 

class My_derived_without_super < My_base
    def initialize
        @y = 30
        @z = 40
    end
end 

x = My_derived_without_super.new
assert_equal(x.check, [nil, 30, 40])

class My_derived_with_super < My_base
    def initialize
        @y = 30
        @z = 40
        super       # order! @y is re-assigned.
    end
end 

x = My_derived_with_super.new
assert_equal(x.check, [10, 20, 40])
