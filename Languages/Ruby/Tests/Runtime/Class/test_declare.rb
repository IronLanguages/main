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

# WHAT CAN BE THE CLASS/MODULE NAME?

# re-opening the type/module but the name is used as something else
C_FixNum = 10
assert_raise(TypeError, "C_FixNum is not a class") { class C_FixNum; end }

class C_Class; end
assert_raise(TypeError, "C_Class is not a module") { module C_Class; end }

M_Str = "string"
assert_raise(TypeError, "M_Str is not a module")  { module M_Str; end }

module M_Module; end 
assert_raise(TypeError, "M_Module is not a class") { class M_Module; end }

# try to derive from itself
class B100 < Object; end 
assert_raise(TypeError, "superclass mismatch for class B100") { class B100 < B100; end }
assert_raise(TypeError, "superclass mismatch for class B110") {
    class B110
        class ::B110 < self; end 
    end 
}
# unrelated
class B120
    class B120 < self; end 
    def m; 3; end 
end
assert_equal(B120::B120.new.m, 3)

# re-opening with different 'name'
class B200
    ::B210 = self
    def m1; 1; end 
end 
class B210
    def m2; 2; end 
end 
x = B200.new 
assert_equal([x.m1, x.m2, x.class.name], [1, 2, 'B200'])
y = B210.new
assert_equal([y.m1, y.m2, y.class.name], [1, 2, 'B200'])
assert_equal(B200, B210)

# WHAT CAN BE THE BASE TYPE?

assert_raise(TypeError, "superclass must be a Class (String given)") { class C < 'abc'; end }
assert_raise(TypeError, "superclass must be a Class (Fixnum given)") { class C < 1; end }
#assert_raise(NameError) {  class C < object; end } 

class B300; end 
x = B300
class B310 < ('whatever'; "else"; x); end 

# WHERE YOU CAN DEFINE THE CLASS

# based on current implementation
# Define*Class combination
class C100
    class C100
        S110 = 110
    end 
    class C100::C100
        S120 = 120
    end 
    class << self
        S130 = 130
    end 
    S140 = 140
    C100::S150 = 150
end 
assert_equal(C100::S140, 140)
assert_equal(C100::C100::S110, 110)
assert_equal(C100::C100::C100::S120, 120)
assert_equal(C100::C100::S150, 150)
assert_raise(NameError) { C100::S130 }

class C200
    class C200::C200
        S210 = 210
    end 
    class C200
        S220 = 220
    end 
   
    C200::S230 = 230
    C200::C200::S240 = 240  # warning
    
    class ::C100
        S160 = 160
    end 
end 

assert_equal(C200::C200::S210, 210)
assert_equal(C200::C200::S220, 220)
assert_equal(C200::C200::S230, 230)
assert_equal(C200::S240, 240)
assert_equal(C100::S160, 160)

class C200::C200
    S250 = 250
end 
assert_equal(C200::C200::S250, 250)
