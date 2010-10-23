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

# When a method is called, Ruby searches for it in a number of places in the following order:
#
# - Among the methods defined in that object (i.e., singleton methods).
# - Among the methods defined by that object's class.
# - Among the methods of the modules included by that class.
# - Among the methods of the superclass.
# - Among the methods of the modules included by that superclass.
# - Repeats Steps 4 and 5 until the top-level object is reached.
    
class My
    def method1; 10; end
    def method2; 20; end
end 

x = My.new
assert_equal(x.method1, 10)
assert_raise(NoMethodError) { x.method_not_exists }

class << x
    def method2; 30; end
    def method3; 40; end
end

assert_equal(x.method1, 10)
assert_equal(x.method2, 30)
assert_equal(x.method3, 40)
assert_raise(NoMethodError) { x.method_not_exists }

# include module
module Simple
    def method1; -10; end
    def method2; -20; end
end 

class My_with_module
    include Simple
    def method2; 20; end
    def method3; 30; end
end 

x = My_with_module.new
assert_equal(x.method1, -10)
assert_equal(x.method2, 20)
assert_equal(x.method3, 30)
assert_raise(NoMethodError) { x.method_not_exists }

class << x
    def method4; 40; end
end 
assert_equal(x.method1, -10)
assert_equal(x.method2, 20)
assert_equal(x.method3, 30)
assert_equal(x.method4, 40)
assert_raise(NoMethodError) { x.method_not_exists }

# with superclass

class My_base
    def method6; -600; end
    def method7; -700; end
end 

class My_derived < My_base
    def method7; 700; end
    def method8; 800; end 
end 

x = My_derived.new
assert_equal(x.method6, -600)
assert_equal(x.method7, 700)
assert_equal(x.method8, 800)
assert_raise(NoMethodError) { x.method_not_exists }

# base with included module

class My_base_with_module
    include Simple
    def method6; -600; end
    def method7; -700; end
end 

class My_derived2 < My_base_with_module
    def method2; 200; end 
    def method7; 700; end
    def method8; 800; end 
end

x = My_derived2.new
assert_equal(x.method1, -10)
assert_equal(x.method2, 200)
assert_equal(x.method6, -600)
assert_equal(x.method7, 700)
assert_equal(x.method8, 800)
assert_raise(NoMethodError) { x.method_not_exists }

class << x
end 
assert_equal(x.method1, -10)
assert_equal(x.method2, 200)
assert_equal(x.method6, -600)
assert_equal(x.method7, 700)
assert_equal(x.method8, 800)
assert_raise(NoMethodError) { x.method_not_exists }

# multiple levels

class My_level1
    include Simple
    def method1; 100; end 
end

class My_level2 < My_level1
    def method2; 200; end
end

class My_level3 < My_level2
    def method3; 300; end 
end 

x = My_level3.new
assert_equal(x.method1, 100)
assert_equal(x.method2, 200)
assert_equal(x.method3, 300)
assert_raise(NoMethodError) { x.method_not_exists }

# access control related to inheritance: the public override method in superclass 

class My_base_with_public_method
    public
    def method; 100; end
end 

class My_derived_with_private_method < My_base_with_public_method
    private 
    def method; 200; end 
end 

x = My_derived_with_private_method.new 
#assert_raise(NoMethodError) { x.method }