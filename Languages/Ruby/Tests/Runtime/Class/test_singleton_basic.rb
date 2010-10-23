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

# create singleton class, to see how it affect the old object and the extended object.

class My
    CONST1, CONST2 = 10, 20
    @@sv1, @@sv2 = 30, 40
    
    def initialize; @iv1, @iv2 = 50, 60; end
    
    def My.sm1; 70; end 
    def My.sm2; 80; end
    def My.sm4; 85; end  
    
    def im1; 90; end
    def im2; 100; end
    
    class Nested
        def m1; 110; end
        def m2; 120; end
    end 

    # helpers for verification
    def b_check_const_positive; [CONST1, CONST2]; end 
    def b_check_const_negative;  CONST3; end 
    def b_check_instance_var; [@iv1, @iv2, @iv3]; end
    def b_check_static_var; [@@sv1, @@sv2, @@sv3]; end
    def b_check_static_methods; [My.sm1, My.sm2, My.sm3, My.sm4]; end 
    def b_check_instance_method; [im1, im2, im3]; end
end 

x, y = My.new, My.new 

singleton = class << x
    CONST2, CONST3 = -10, -20
    @@sv2, @@sv3 = -30, -40 
    
    def initialize   # will not be called
        @iv2, @iv3 = -50, -60
    end
    
    def My.sm2; -70; end
    def My.sm3; -80; end
    
    def im2; -90; end
    def im3; -100; end
    
    def self.sm4; -106; end
    def self.sm5; -108; end
    
    class Nested
        def m2; -110; end
        def m3; -120; end
    end
    
    # helpers for verification
    def d_check_const; [CONST1, CONST2, CONST3]; end 

    def d_check_instance_var; [50, 60, nil]; end
    def d_check_instance_method; [im1, im2, im3]; end
    
    def d_check_static_var_positive; [@@sv2, @@sv3]; end
    def d_check_static_var_negative; @@sv1; end

    def d_check_static_method_positive; [My.sm1, My.sm2, My.sm3, My.sm4]; end 
    
    def self.check_static_var_positive; [@@sv2, @@sv3]; end
    def self.check_static_var_negative; @@sv1; end
    
    self
end

assert_equal(x.b_check_const_positive, [10, 20])
assert_raise(NameError) { x.b_check_const_negative  }
assert_equal(x.b_check_instance_var, [50, 60, nil])
assert_equal(x.b_check_static_var, [30, 40, -40])
assert_equal(x.b_check_static_methods, [70, -70, -80, 85])
assert_equal(x.b_check_instance_method,[90, -90, -100])

assert_equal(x.d_check_instance_method,[90, -90, -100])
assert_equal(x.d_check_const, [10, -10, -20])
assert_equal(x.d_check_instance_var, [50, 60, nil])
assert_equal(x.d_check_static_var_positive, [-30, -40])
assert_raise(NameError) { x.d_check_static_var_negative }
assert_equal(x.d_check_static_method_positive, [70, -70, -80, 85])

assert_equal(singleton.sm1, 70)
assert_equal(singleton.sm2, -70)
assert_equal(singleton.sm3, -80)
assert_equal(singleton.sm4, -106)
assert_equal(singleton.sm5, -108)
assert_equal(singleton.check_static_var_positive, [-30, -40])
assert_raise(NameError) { singleton.check_static_var_negative }
assert_equal(singleton::CONST1, 10)
assert_equal(singleton::CONST2, -10)
assert_equal(singleton::CONST3, -20)

sn = singleton::Nested.new
assert_raise(NoMethodError) { sn.m1 } 
assert_equal(sn.m2, -110)
assert_equal(sn.m3, -120)

assert_equal(y.im1, 90)
assert_equal(y.im2, 100)
assert_raise(NoMethodError) { y.im3 }

# change My, and the singleton should see the impact
class My
    def My.sm10; 10; end 
    def im11; 20; end 
    def set_var; @iv12 = 30; @@sv13 = 40; end 
    
    def get_var1; [@iv12, @@sv13]; end
    def My.get_var2; @@sv13; end 
end 
assert_equal(singleton.sm10, 10)
assert_equal(x.im11, 20)
x.set_var
assert_equal(x.get_var1, [30, 40])
assert_equal(singleton.get_var2, 40)

## Another way to create the singleton
## by creating methods for individual objects

class My_again
    def im1; 10; end 
    def im2; 20; end 
end 

x = My_again.new
y = My_again.new

def x.im2; 200; end 
def x.im3; 300; end 

assert_equal(x.im1, 10)
assert_equal(x.im2, 200)
assert_equal(x.im3, 300)

assert_equal(y.im1, 10)
assert_equal(y.im2, 20)
assert_raise(NoMethodError) { y.im3 }

# re-open singleton
class C100; end 
x = C100.new 
class << x
    def m1; 1; end 
end 
assert_equal(x.m1, 1)
class << x
    def m2; 2; end
end 
assert_equal([x.m1, x.m2], [1, 2])
def x.m3; 3; end 
assert_equal([x.m1, x.m2, x.m3], [1, 2, 3])

# ...