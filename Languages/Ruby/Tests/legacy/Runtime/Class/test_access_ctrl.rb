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

# status: complete

require '../../util/assert.rb'

# to check whether the caller can access public/protected/private methods.
#  - class, instance methods?  // class methods always public?
#  - access the defined methods inside the class, derived class, and outside the class

class My_base
    public 
    def My_base.pub_sm; -100; end
    def pub_im; 100; end 
    
    protected 
    def My_base.prot_sm; -200; end
    def prot_im; 200; end 
    
    private 
    def My_base.priv_sm; -300; end 
    def priv_im; 300; end 
    
    public 
    def instance_check_from_base
        assert_equal(pub_im, 100)
        assert_equal(prot_im, 200)
        assert_equal(priv_im, 300)
        
        # accessing via self
        assert_equal(self.pub_im, 100)
        assert_equal(self.prot_im, 200)
        assert_raise(NoMethodError) { self.priv_im }
        
        assert_equal(My_base.pub_sm, -100)
        assert_equal(My_base.prot_sm, -200)
        assert_equal(My_base.priv_sm, -300)
    end
    
    def My_base.static_check_from_base
        assert_equal(pub_sm, -100)
        assert_equal(prot_sm, -200)
        assert_equal(priv_sm, -300)
        
        assert_equal(self.pub_sm, -100)
        assert_equal(self.prot_sm, -200)
        assert_equal(self.priv_sm, -300)
        
        assert_equal(My_base.pub_sm, -100)
        assert_equal(My_base.prot_sm, -200)
        assert_equal(My_base.priv_sm, -300)
    end 
    
    def instance_check_as_arg_from_base arg
        assert_equal(arg.pub_im, 100)
        assert_equal(arg.prot_im, 200)
        assert_raise(NoMethodError) { arg.priv_im }
    end 
    
    def My_base.static_check_as_arg_from_base arg
        assert_equal(arg.pub_im, 100)
        assert_raise(NoMethodError) { arg.prot_im }
        assert_raise(NoMethodError) { arg.priv_im }
    end 
end 

x = My_base.new

# calling outside
assert_equal(My_base.pub_sm, -100)
assert_equal(My_base.prot_sm, -200)
assert_equal(My_base.priv_sm, -300)
        
assert_equal(x.pub_im, 100)
assert_raise(NoMethodError) { x.prot_im } # protected method `prot_im' called for #<My_base:0x769f878> (NoMethodError)
assert_raise(NoMethodError) { x.priv_im } # private method `priv_im' called for #<My:0x75e0450> (NoMethodError)

My_base.static_check_from_base
x.instance_check_from_base

My_base.static_check_as_arg_from_base x
x.instance_check_as_arg_from_base x

class My_derived < My_base
    public 
    def instance_check_from_derived
        assert_equal(pub_im, 100)
        assert_equal(prot_im, 200)
        assert_equal(priv_im, 300)
        
        assert_equal(self.pub_im, 100)
        assert_equal(self.prot_im, 200)
        assert_raise(NoMethodError) { self.priv_im }
        
        assert_equal(My_derived.pub_sm, -100)
        assert_equal(My_derived.prot_sm, -200)
        assert_equal(My_derived.priv_sm, -300)
    end 
    
    def My_base.static_check_from_derived
        assert_equal(pub_sm, -100)
        assert_equal(prot_sm, -200)
        assert_equal(priv_sm, -300)
        
        assert_equal(self.pub_sm, -100)
        assert_equal(self.prot_sm, -200)
        assert_equal(self.priv_sm, -300)
        
        assert_equal(My_derived.pub_sm, -100)
        assert_equal(My_derived.prot_sm, -200)
        assert_equal(My_derived.priv_sm, -300)
    end 
    
    def instance_check_as_arg_from_derived arg
        assert_equal(arg.pub_im, 100)
        assert_equal(arg.prot_im, 200)
        assert_raise(NoMethodError) { arg.priv_im }
    end 
    
    def My_derived.static_check_as_arg_from_derived arg
        assert_equal(arg.pub_im, 100)
        assert_raise(NoMethodError) { arg.prot_im }
        assert_raise(NoMethodError) { arg.priv_im }
    end 
end 

y = My_derived.new

y.instance_check_from_derived
My_derived.static_check_from_derived
y.instance_check_as_arg_from_derived y
My_derived.static_check_as_arg_from_derived y

assert_equal(y.pub_im, 100)
assert_raise(NoMethodError) { y.prot_im } 
assert_raise(NoMethodError) { y.priv_im } 
