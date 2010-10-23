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

# access const from singleton class

class C100
    S101 = 1
    class NC
        S102 = 2
    end 
    module NM 
        S103 = 3
    end 
end

x = C100.new 

singleton = class << x
    S111 = 11
    class NC
        S112 = 12
    end 
    module NM
        S113 = 13
    end
    
    def self::check
        assert_raise(NameError) { C100::S111 }
        assert_raise(NameError) { NC::S102 }
        assert_raise(NameError) { C100::NC::S112 }
        assert_raise(NameError) { NM::S103 }
        assert_raise(NameError) { C100::NM::S113}
        
        [ 
            S101, self::S101, C100::S101, 
            S111, self::S111, 
            C100::NC::S102,
            NC::S112, self::NC::S112, 
            
            C100::NM::S103,
            NM::S113, self::NM::S113,
        ]        
    end
    
    self
end 

assert_equal(singleton::S101, 1)
assert_raise(NameError)  { singleton::NC::S102 }
assert_raise(NameError)  { singleton::NM::S103 }

assert_equal(singleton::S111, 11)
assert_equal(singleton::NC::S112, 12)
assert_equal(singleton::NM::S113, 13)

assert_equal(singleton::check, [1, 1, 1, 11, 11, 2, 12, 12, 3, 13, 13])

## 
class C200
    S101 = 1 
    def C200::m1; 1; end
    def m2; 2; end
end 

singleton = class << C200
    S111 = 11
    
    def C200::m3; 3; end 
    def m4; 4; end
    def self::m5; 5; end 

    self
end 

assert_raise(NameError) { singleton::S101 }
assert_equal(singleton::S111, 11)
assert_raise(NoMethodError) { singleton::m3 }
assert_raise(TypeError) { singleton.new } #.m4
assert_equal(singleton::m5, 5)
