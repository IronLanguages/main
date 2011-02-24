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

# status: almost complete, few nice-to-have improvements:
#  - try other members types in the "CHANGE BASE TYPE" scenario

require '../../util/assert.rb'

# CHANGE THE MEMBERS
# - redefining the class with members of the new or same names

class My
    CONST1 = 10
    CONST2 = 20
    @@sv1 = 30
    @@sv2 = 40
    
    def initialize
        @iv1 = 50
        @iv2 = 60
    end 
    
    def My.sm1; 70; end 
    def My.sm2; 80; end 
    
    def im1; 90; end
    def im2; 100; end
    
    def check 
        return @iv1, @iv2, @iv3, @@sv1, @@sv2, @@sv3
    end 
    
    class Nested
        def m1; 110; end
        def m2; 120; end
    end 
end 

x_before = My.new
y_before = My::Nested.new

# try those to-be-added/updated members
assert_equal(My::CONST2, 20)
assert_raise(NameError) { My::CONST3 }

assert_equal(My::sm2, 80)
assert_raise(NoMethodError) { My::sm3 }

assert_equal(x_before.im2, 100)
assert_raise(NoMethodError) { x_before.im3 }

assert_equal(y_before.m2, 120)
assert_raise(NoMethodError) { y_before.m3 }

# let us create something else in the middle
class Other; end 

class My
    CONST2 = -10   # warning issued
    CONST3 = -20
    @@sv2 = -30 
    @@sv3 = -40 
        
    def initialize  
        @iv2 = -50
        @iv3 = -60
    end
    
    def My.sm2; -70; end
    def My.sm3; -80; end
    def im2; -90; end
    def im3; -100; end
    
    class Nested
        def m2; -110; end
        def m3; -120; end
    end
end   

x_after = My.new
y_after = My::Nested.new

assert_equal(My::CONST1, 10)
assert_equal(My::CONST2, -10)
assert_equal(My::CONST3, -20)

assert_equal(My.sm1, 70)
assert_equal(My.sm2, -70)
assert_equal(My.sm3, -80)

assert_equal(x_after.im1, 90)
assert_equal(x_after.im2, -90)
assert_equal(x_after.im3, -100)

assert_equal(x_after.check, [nil, -50, -60, 30, -30, -40])

assert_equal(y_after.m1, 110)
assert_equal(y_after.m2, -110)
assert_equal(y_after.m3, -120)

# let us then check the object created before the re-design/open
assert_equal(x_before.im1, 90)
assert_equal(x_before.im2, -90)
assert_equal(x_before.im3, -100)

assert_equal(x_before.check, [50, 60, nil, 30, -30, -40])  # difference is those variables assigned in "initialize"

assert_equal(y_before.m1, 110)
assert_equal(y_before.m2, -110)
assert_equal(y_before.m3, -120)

# CHANGE THE BASE TYPE
# - if a class of the same name already exists, the class and superclass must match.

class My_base1; 
    def m0; 10; end
end 
class My_base2; end 

class My_derived < My_base1; 
    def m1; 100; end
    def m2; 200; end
end

assert_raise(TypeError) { class My_derived < My_base2; end } # superclass mismatch for class My_derived (TypeError)
assert_raise(TypeError) { class My_derived < Object; end } 
assert_raise(TypeError) { class My_base1 < My_base2; end }   # My_base1 superclass was not specified

x = My_derived.new
assert_equal(x.m2, 200)
assert_raise(NoMethodError) { x.m3 }

class My_derived   # able to change it WITHOUT superclass specified
    def m2; -200; end
    def m3; -300; end
end

assert_equal(x.m0, 10)
assert_equal(x.m1, 100)
assert_equal(x.m2, -200)
assert_equal(x.m3, -300)

class My_base1;
    def m0; 1000; end 
    def m4; 4000; end
end 

assert_equal(x.m0, 1000)
assert_equal(x.m4, 4000)

# CHANGE TYPE BY INCLUDING MODULE
module Mod
    def helper; 24; end
end 
assert_raise(NameError) { My_base8 }
class My_base8; end 
class My_derived8 < My_base8; end 
x = My_base8.new
assert_raise(NoMethodError) { x.helper }

class My_base8;
    include Mod
end 
assert_equal(x.helper, 24)
