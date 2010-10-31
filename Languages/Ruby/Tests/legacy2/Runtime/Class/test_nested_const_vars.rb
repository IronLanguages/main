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

##
## not covering the constant lookup inside method
##

## inside a module, defining classes which have the same name as previously defined global class, self

class C100; 
    def C100::m1; 1; end 
end 

module M100
    def C100::m2; 2; end
    
    assert_equal(C100::m1, 1)
    
    class C100              # new nested class
        def C100::m3; 3; end
    end
    def C100::m4; 4; end    # adding method to the nested class
    class M100::C100;       # re-opening the nested class
        def C100::m5; 5; end;
        def self::m5_1; 51; end;
    end     
    class self::C100;       # re-opening the nested class
        def C100::m6; 6 end; 
        def self::m6_1; 61; end
    end 
    
    assert_raise(NoMethodError) { C100::m1 }
    
    class C100::C100        # nested C100 inside the nested class C100
        def C100::m7; 7; end     # C100 is M100::C100
        def self::m8; 8; end
    end 
    
    class ::C100;           # re-opening existing top-level class
        def self::m9; 9; end
        def C100::m9_1; 91; end
    end 
    
    class ::C110;           # creating another new top-level class
        def self::m10; 10; end 
    end 
    
    class M100              #  new nested class
        def M100::m11; 11; end
    end
        
    class M100::M100        # defining nested class M100 inside M100::M100
        def M100::m12; 12; end
        def self::m13; 13; end
    end 
    
    assert_equal(M100::m12, 12)
    assert_equal(M100::M100::m13, 13)
end

assert_equal(C100::m1, 1)
assert_equal(C100::m2, 2)

assert_equal(M100::C100::m3, 3)
assert_equal(M100::C100::m4, 4)
assert_equal(M100::C100::m5, 5)
assert_equal(M100::C100::m5_1, 51)
assert_equal(M100::C100::m6, 6)
assert_equal(M100::C100::m6_1, 61)

assert_equal(M100::C100::m7, 7)
assert_equal(M100::C100::C100::m8, 8)

assert_equal(C100::m9, 9)
assert_equal(M100::C100::m9_1, 91)

assert_equal(C110::m10, 10)

assert_equal(M100::M100::m12, 12)
assert_equal(M100::M100::M100::m13, 13)

## inside a module, defining a nested module which has the same name as self
module M200 
    module M200 
        S1 = 1        
    end 
    
    assert_raise(NameError, "uninitialized constant M200::M200::M200")  { M200::M200::S1 }
        
    module M200::M200       # 
        S2 = 2
    end
    
    module ::M200      
        S3 = 3
    end
    
    module self::M200
        S4 = 4
    end 
    
    assert_equal(M200::S1, 1)
    assert_equal(M200::M200::S2, 2)
    assert_equal(S3, 3)
    assert_equal(M200::S4, 4)
    
    assert_raise(NameError, "uninitialized constant M200::M200::M200::S1")  { M200::M200::S1 }
end 

assert_equal(M200::M200::S1, 1)
assert_equal(M200::M200::M200::S2, 2)
assert_equal(M200::S3, 3)
assert_equal(M200::M200::S4, 4)

## inside a class, defining classes which have the same name as previously defined global module, or self

module M300
    S1 = 1
end 

class C300
    def C300; 301; end 
    def C300::m1; 1; end
    
    def M300; 302; end 
    def M300::m2; 2; end
    
    class C300
        def C300::m3; 3; end                    # C300::C300
        def self::m3_1; 31; end                     
        def m3_2; 32; end 
    end 

    class C300::C300
        def C300::m4; 4; end                    # C300::C300
        def self::m4_1; 41; end                 # C300::C300::C300
    end
    
    class C300
        class C300
            def self::m4_2; 42; end
            class C300
                def self::m4_3; 43; end
            end 
            def m4_4; 44; end
        end 
        assert_equal(C300::m4_2, 42)
        assert_equal(self::C300::m4_2, 42)
        assert_equal(C300::C300::m4_3, 43)
        assert_equal(::C300::C300::C300::C300::m4_3, 43)
        assert_raise(NoMethodError) { C300::C300::C300::C300::m4_3 }    # warning: toplevel constant C300 referenced by C300::C300::C300::C300::C300
        
        assert_raise(NoMethodError) { C300::m1 }
        assert_raise(NoMethodError) { C300::C300::m1 }
        assert_equal(C300::C300::C300::m1, 1)                           # warning
        assert_raise(NoMethodError) { C300::C300::C300::C300::m1 }      # warning
        
        assert_equal(C300.new.m4_4, 44)
    end 
    
    class ::C300
        def self::m5; 5; end
        def C300::m6; 6; end 
    end  
    class self::C300
        def self::m7; 7; end 
        def C300::m8; 8; end
    end 
    
    assert_raise(TypeError) { class ::M300; end  }
    
    class M300
        def M300::m9; 9; end 
    end 
    
    def M300::m10; 10; end
end 

assert_equal(C300::m1, 1)
assert_equal(M300::m2, 2)
assert_equal(M300::S1, 1)
assert_equal(C300.new.C300, 301)
assert_equal(C300.new.M300, 302)

assert_equal(C300::C300::m3, 3)
assert_equal(C300::C300::m3_1, 31)
assert_raise(NoMethodError) { C300.new.m3_2 }
assert_equal(C300::C300.new.m3_2, 32)

assert_equal(C300::C300::m4, 4)                 # different from module
assert_equal(C300::C300::C300::m4_1, 41)
assert_equal(C300::C300::C300.new.m4_4, 44)

assert_equal(C300::m5, 5)
assert_equal(C300::C300::m6, 6)
assert_equal(C300::C300::m7, 7)
assert_equal(C300::C300::C300::m8, 8)

assert_equal(C300::M300::m9, 9)
assert_equal(C300::M300::m10, 10)

## inside a class, defining modules which have the same name as previously defined global module

module M400
    S1 = 1
end

class C400
    module M400
        def M400::m1; 1; end
    end
    module ::M400
        def self::m1_1; 11; end
    end 
    
    def C400::m2; 2; end 

    assert_equal(C400::m2, 2)
    assert_equal(m2, 2)
    
    module C400::C400
        def self::m3; 3; end 
        def C400::m4; 4; end 
    end 
    
    module self::C400
        def self::m5; 5; end 
        def C400::m6; 6; end 
    end 

    assert_raise(NoMethodError) {  C400::m2 }
    assert_equal(m2, 2)
    assert_equal(self::m2, 2)    
    
    module C400
        def self::m7; 7; end
        def C400::m8; 8; end
    end
    
    assert_raise(NameError) { C400::C400 }
    assert_equal(C400::m5, 5)
    assert_equal(self::C400::m7, 7)

    module C400
        module C400
            def self::m9; 9; end 
        end 
        assert_equal(C400::m9, 9)
        assert_equal(self::C400::m9, 9)
        assert_raise(NameError) { C400::C400 }
    end
end 

assert_equal(C400::M400::m1, 1)
assert_equal(M400::m1_1, 11)
assert_equal(C400::m2, 2)
assert_equal(C400::C400::m3, 3)
assert_equal(C400::C400::m4, 4)
assert_equal(C400::C400::m5, 5)
assert_equal(C400::C400::m6, 6)
assert_equal(C400::C400::m7, 7)
assert_equal(C400::C400::m8, 8)
assert_equal(C400::C400::C400::m9, 9)


# 
def m1; 1; end 

module M500
    
    module M510
        def M500.m2; 2; end
        def M510.m3; 3; end
    end 
    
    assert_equal(m1, 1)
    assert_raise(NoMethodError) { M500::m1 }
    
    assert_equal(m2, 2)
    assert_equal(self::m2, 2)
    assert_equal(M500::m2, 2)
    
    assert_equal(M510::m3, 3)
    assert_equal(self::M510::m3, 3)
    assert_equal(M500::M510::m3, 3)
end

assert_equal(M500::m2, 2)
assert_equal(M500::M510::m3, 3)