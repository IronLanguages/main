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

# get/set const inside Object, class, module

# get 
assert_raise(NameError, "uninitialized constant S1") { S1 } 
assert_raise(NameError, "uninitialized constant S2") { ::S2 } 
assert_raise(NameError, "uninitialized constant S3") { Object::S3 } 

class SpecialClass; end 
module SpecialModule; end 

assert_raise(NameError, "uninitialized constant SpecialClass::S7") { SpecialClass::S7 }
assert_raise(NameError, "uninitialized constant SpecialModule::S8") { SpecialModule::S8 }

# set
S1 = 1
::S2 = 2
Object::S3 = 3

assert_raise(TypeError, "main is not a class/module")  { self::S4 = 4 }
assert_raise(NameError, "uninitialized constant C") { C::S5 = 5 }
assert_raise(NameError, "uninitialized constant M") { M::S6 = 6 }

assert_raise(NoMethodError, "undefined method `S7=' for SpecialClass:Class") { SpecialClass.S7 = 7 }
SpecialClass::S7 = 7
SpecialModule::S8 = 8

# get 
assert_equal(S1, 1) 
assert_equal(::S1, 1) 
assert_equal(Object::S1, 1) 

assert_equal(Object::S2, 2) 
assert_equal(::S3, 3)

sc = SpecialClass.new 
Singleton = class << sc; self; end 
assert_equal(SpecialClass::S1, 1)   # warning: toplevel constant S1 referenced by SpecialClass::S1
assert_equal(Singleton::S1, 1)    # warning: toplevel constant S1 referenced by SpecialClass::S1
assert_equal(Singleton::S7, 7)
assert_raise(TypeError) { sc::S7 }
assert_raise(NameError, "uninitialized constant SpecialClass::S8") { SpecialClass::S8 }

assert_equal(SpecialModule::S8, 8)
assert_raise(NameError, "uninitialized constant SpecialModule::S1") { SpecialModule::S1 }
assert_raise(NameError, "uninitialized constant SpecialModule::S7") { SpecialModule::S7 }

class C
    # get those defined outside
    assert_equal(::S1, 1)
    assert_equal(Object::S2, 2)
    assert_equal(S3, 3)
    
    assert_equal(SpecialClass::S7, 7)
    assert_equal(SpecialModule::S8, 8)
    
    assert_equal(C::S2, 2)  # warning: toplevel constant S2 referenced by C::S2
    assert_equal(C::SpecialClass::S7, 7)  # warning
    assert_equal(C::SpecialModule::S8, 8)  # warning
    
    # set 
    S9 = 9
    ::S10 = 10
    C::S11 = 11
    self::S12 = 12
    
    SpecialClass::S13 = 13
    SpecialModule::S14 = 14
    
    # get those newly defined
    assert_equal(S9, 9)
    assert_equal(C::S9, 9)
    assert_equal(self::S9, 9)
    assert_raise(NameError, "uninitialized constant S9")  { ::S9 }
    
    assert_equal(::S10, 10)
    assert_equal(Object::S10, 10)
    assert_equal(C::S10, 10)    # warning 
    assert_equal(self::S10, 10) # warning 
    
    assert_equal(self::S11, 11)
    assert_equal(C::S12, 12)
    
    assert_equal(SpecialClass::S7, 7)
    assert_equal(SpecialClass::S13, 13)
    assert_equal(SpecialModule::S8, 8)
    assert_equal(SpecialModule::S14, 14)

    assert_equal(SpecialClass::S10, 10)   # warning
    assert_raise(NameError, "uninitialized constant SpecialModule::S10") { SpecialModule::S10 }
    
    def C::m1; 1; end   # succeed, defined to the C "class"
    
    # duplicate variable names    
    ::S2 = 20   # warning 
    assert_equal(Object::S2, 20)
    S11 = 110   # warning
    assert_equal(self::S11, 110)
    
    def self::method1_before_redefining_C; C; end   
    def self::method2_before_redefining_C; self::S9; end 
    
    C = "ruby"
    assert_equal(C, "ruby")
    assert_equal(self::C, "ruby")
    
    assert_raise(TypeError, "ruby is not a class/module") { C::C }   
    assert_raise(TypeError, "ruby is not a class/module") { C::S12 }
    
    def C::m2; 2; end  # succeed, defined to the C "ruby"
    
    # calling methods inside C
    assert_raise(NoMethodError) { C::m1 }
    assert_equal(C::m2, 2)
    assert_equal(self::m1, 1)
    assert_raise(NoMethodError) { self::m2 }
    
    def self::method1_after_redefining_C; C; end 
    def self::method2_after_redefining_C; self::S9; end 
end

# get
assert_equal(S2, 20)
assert_equal(S10, 10)
assert_equal(C::S11, 110)

assert_equal(SpecialClass::S7, 7)
assert_equal(SpecialClass::S13, 13)
assert_equal(SpecialModule::S8, 8)
assert_equal(SpecialModule::S14, 14)

# calling methods outside C    
assert_equal(C::m1, 1)
assert_raise(NoMethodError) { C::C::m1 }
assert_raise(NoMethodError) { C::m2 }
assert_equal(C::C::m2, 2)

# 
assert_equal(C::method1_before_redefining_C, "ruby")
assert_equal(C::method2_before_redefining_C, 9)
assert_equal(C::method1_after_redefining_C, "ruby")
assert_equal(C::method2_after_redefining_C, 9)

module M
    # get
    assert_equal(S1, 1)
    assert_equal(C::S9, 9)
    assert_equal(SpecialModule::S8, 8)
    
    # set
    S15 = 15
    ::S16 = 16
    M::S17 = 17
    self::S18 = 18

    SpecialClass::S19 = 19
    SpecialModule::S20 = 20
    
    # get 
    assert_equal(M::S15, 15)
    assert_equal(self::S15, 15)
    
    assert_equal(S16, 16)
    assert_equal(C::S16, 16)  # warning
    assert_raise(NameError)  { self::S16 }
    assert_raise(NameError)  { M::S16 }

    assert_equal(S17, 17)
    assert_equal(S18, 18)
    
    assert_equal(SpecialClass::S7, 7)
    assert_equal(SpecialClass::S13, 13)
    assert_equal(SpecialClass::S19, 19)
    
    assert_equal(SpecialModule::S8, 8)
    assert_equal(SpecialModule::S14, 14)
    assert_equal(SpecialModule::S20, 20)
    
    module N 
        S21 = 21
        ::S22 = 22
        
        assert_equal(S22, 22)
        assert_equal(::S22, 22)
    end 
    
    assert_raise(NameError) { S21 } 
    assert_equal(N::S21, 21)
    assert_equal(M::N::S21, 21)
    
    assert_equal(::S22, 22)
    assert_equal(S22, 22)
    assert_raise(NameError) { N::S22 } 
end

assert_equal(M::S15, 15)
assert_equal(S16, 16)
assert_raise(NameError) { M::S16 }
assert_raise(NameError) { ::S17 }
assert_equal(M::N::S21, 21)
assert_equal(S22, 22)

