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

# reading CONST inside methods

MyVar = 1

module MyModule
    C100 = 100
    class MyNestedClass
        C110 = 110
    end 
    module MyNestedModule
        C120 = 120
    end 
end 

class MyClass
    C200 = 200
    class MyNestedClass
        C210 = 210
    end 
    module MyNestedModule
        C220 = 220
    end   
end

class TestClass
    C300 = 300
    
    class MyNestedClass
        C310 = 310
    end 
    
    def my_instance_method 
        assert_raise(NameError) { MyModule::MyVar }  # can not find the top level constant referenced by top level MODULE 
        assert_raise(NameError) { C400 }
        
        assert_raise(TypeError) { self::C300 }
        assert_raise(TypeError) { self::MyNestedClass }
        assert_raise(TypeError) { self::MyNestedModule }
        assert_raise(NameError) { TestClass::MyNestedModule::C400 }
        
        return [
            MyVar, ::MyVar, 
            MyModule::C100, MyModule::MyNestedClass::C110, MyModule::MyNestedModule::C120, 
            MyClass::C200, MyClass::MyNestedClass::C210, MyClass::MyNestedModule::C220,
            
            TestClass::C300, C300, 
            TestClass::C305, C305,
            
            TestClass::MyNestedClass::C310, MyNestedClass::C310,
            TestClass::MyNestedModule::C320, MyNestedModule::C320,
            
            MyClass::MyVar,  TestClass::MyVar, # warning            
        ]        
    end 
    
    def self::my_static_method
        assert_raise(NameError) { MyModule::MyVar }  
        assert_raise(NameError) { C400 }
        assert_raise(NameError) { TestClass::MyNestedClass::C400 }
        
        return [
            MyVar, ::MyVar, 
            MyModule::C100, MyModule::MyNestedClass::C110, MyModule::MyNestedModule::C120, 
            MyClass::C200, MyClass::MyNestedClass::C210, MyClass::MyNestedModule::C220,
            
            TestClass::C300, C300, 
            TestClass::C305, C305,

            TestClass::MyNestedClass::C310, MyNestedClass::C310,
            TestClass::MyNestedModule::C320, MyNestedModule::C320,
            
            MyClass::MyVar,  TestClass::MyVar, # warning      
                    
            self::C300, self::C305, 
            self::MyNestedClass::C310, self::MyNestedModule::C320
        ]
    end 
    
    C305 = 305   # defined after my_instance_method
    
    module MyNestedModule
        C320 = 320
    end 
end 

assert_equal(TestClass.new.my_instance_method, [1, 1, 100, 110, 120, 200, 210, 220, 300, 300, 305, 305, 310, 310, 320, 320, 1, 1])
assert_equal(TestClass::my_static_method, [1, 1, 100, 110, 120, 200, 210, 220, 300, 300, 305, 305, 310, 310, 320, 320, 1, 1, 300, 305, 310, 320])

# dup name
class C100
    S10 = 10
    
    def C100::m1; 
        [ S10, self::S10, C100::S10, ::C100::S10 ]
    end 
end 

assert_equal(C100::m1, [10, 10, 10, 10])

# 
class C200
    S10 = 10
    
    def C200::m1; 
        assert_equal(C200.class, Class) # read C200 is ok
        assert_raise(NameError, "uninitialized constant C200::C200::S10") { C200::S10 } 
        
        [ S10, self::S10, ::C200::S10 ]
    end 
    
    class C200
        def self::m2; 
            assert_raise(NameError) { self::S10 }
            assert_raise(NameError) { C200::S10 }
            
            [
                S10, ::C200::S10,
                S20, self::S20, C200::S20, ::C200::C200::S20
            ]
        end 
        S20 = 20
    end 
end 

assert_equal(C200::m1, [10, 10, 10])
#assert_equal(C200::C200::m2, [10, 10, 20, 20, 20, 20])

# module
module M300
    def M300::m1; 1; end 

    class M300
        def m2; 2; end 
        def M300::m3; 3; end 
        
        def m4;
#            assert_raise(NameError) { m1 }
#            assert_raise(NoMethodError) { M300::m1 }
            
            assert_raise(NoMethodError) { M300::m2 }
            assert_raise(NoMethodError) { M300::M300::m2 }
            
#            assert_raise(NameError) { m3 }
            
            [
                ::M300::m1,
                m2, self::m2, 
                M300::m3, 
            ] 
        end 
    end 
end 
assert_equal(M300::m1, 1) 
assert_equal(M300::M300.new.m2, 2) 
#assert_equal(M300::M300::m3, 3) 

assert_equal(M300::M300.new.m4, [1, 2, 2, 3])