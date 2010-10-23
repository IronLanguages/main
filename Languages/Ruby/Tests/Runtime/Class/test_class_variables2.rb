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

# status: complete (after fixing $x issue)
#  - necessary update needed based on the implementation change.

# test_class_variable.rb already covers most of aspects related to class variables.
# this test is more target on the current implementation.

require '../../util/assert.rb'

# Set/GetObjectClassVariable
class C100; 
    def m3; @@a100; end 
end
x = C100.new 
class << x
    def m1; @@a100 = 1; end  # expected warning
    def m2; [@@a100, @@a110]; end      # warning   
    @@a110 = 2               # warning
end 

assert_equal(@@a110, 2)
assert_raise(NameError, "uninitialized class variable @@a100 in Object") { x.m2 }
assert_equal(x.m1, 1)
assert_equal(x.m2, [1, 2])
assert_equal(x.m3, 1)
assert_equal(@@a100, 1)

y = C100.new 
assert_equal(y.m3, 1)

# GetClassVariableOwner

@@a200 = 3
class C200;
    assert_equal(@@a200, 3)     # look for the superclass Object
    assert_raise(NameError, "uninitialized class variable @@a210 in C200") { @@a210 }
    @@a200 = 4
    @@a210 = 5
    assert_equal(@@a200, 4)
    assert_equal(@@a210, 5)
end 
assert_equal(@@a200, 4)
assert_raise(NameError, "uninitialized class variable @@a210 in Object") { @@a210 } 

# inside singleton, emits SetClassVariable
# the class variables are in the module M300
module M300 
    class C300; end 
    $x = C300.new  
    
    class << $x
        @@a300 = 6
        def m1; @@a300; end
    end 
    assert_equal(@@a300, 6)
    @@a300 = 7
    assert_equal($x.m1, 7)
    
    def self.m2
        @@a300
    end 
    def self.m3= v
        @@a300 = v
    end 
end 
def M300.m4; @@a300; end 
def M300.m5= v; @@a300 = v; end  # add to the "Object"

M300.m3= 8
assert_equal(M300.m2, 8)
assert_raise(NameError, "uninitialized class variable @@a300 in Object") { M300.m4 } 
M300.m5 = 9 
assert_equal(@@a300, 9)
assert_equal(M300.m4, 9)
assert_equal(M300.m2, 8)   # remains 8

# walking the scope: GetClassVariableOwner
@@a400 = -1
module M400
    @@a401 = 1
    @@a402 = 2
    
    class C410
        @@a401 = 11
        @@a403 = 33
        
        assert_equal(@@a400, -1)
        assert_equal(@@a401, 11) 
        assert_raise(NameError, "uninitialized class variable @@a402 in M400::C410") { @@a402 }
        assert_equal(@@a403, 33)
    end 
    
    $x = C410.new  # update!
    class << $x
        @@a402 = 222
        @@a404 = 444
        assert_raise(NameError, "uninitialized class variable @@a400 in M400") { @@a400 }
        assert_equal(@@a401, 1) 
        assert_equal(@@a402, 222)
        assert_raise(NameError, "uninitialized class variable @@a403 in M400") { @@a403 }
        assert_equal(@@a404, 444)
    end 

    module M420
        @@a401 = 1111
        @@a405 = 5555
        assert_raise(NameError, "uninitialized class variable @@a400 in M400::M420") { @@a400 }
        assert_equal(@@a401, 1111)
        assert_raise(NameError, "uninitialized class variable @@a402 in M400::M420") { @@a402 }
        assert_equal(@@a405, 5555)
    end 
    
    assert_raise(NameError, "uninitialized class variable @@a400 in M400") { @@a400 }
    assert_equal(@@a401, 1)
    assert_equal(@@a402, 222)
    assert_raise(NameError, "uninitialized class variable @@a403 in M400") { @@a403 }
    assert_equal(@@a404, 444)
end

# order impact: where the static variable is stored?
class C500
    def m0; @@a501; end
end 
class C510 < C500
    def m1= v; @@a501 = v; end 
    def m2; @@a501; end 
end
x = C510.new 
x.m1 = -1
assert_equal(x.m2, -1)
assert_raise(NameError, "uninitialized class variable @@a501 in C500") { x.m0 }

# order
class C600
    def m1= v; @@a601 = v; end 
    def m2; @@a601; end
end 

class C610 < C600
    def m3= v; @@a601 = v; end 
    def m4; @@a601; end
end 

x = C610.new 
assert_raise(NameError, "uninitialized class variable @@a601 in C610") { x.m4  }
x.m1 = -2
assert_equal(x.m4, -2)
assert_equal(x.m2, -2)
x.m3 = -22
assert_equal(x.m4, -22)
assert_equal(x.m2, -22)

# TryResolveClassVariable
module M700
    @@a710 = 10
    @@a720 = 20
end 
module M701
    @@a720 = -20
    @@a730 = -30
    @@a740 = -40
end 

module M702
    @@a741 = -41
end 

class C703
    include M702
    
    @@a740 = 400
    @@a750 = 500
    @@a760 = 600
end 

class C705 < C703
    include M700, M701
    
    @@a760 = -6000
    
    assert_equal(@@a710, 10)
    assert_equal(@@a720, 20)   # in order search, the first hit wins
    assert_equal(@@a730, -30)
    assert_equal(@@a740, -40)  # C705's mixins first
    assert_equal(@@a750, 500)
    assert_equal(@@a760, -6000)  # starts from self
    assert_equal(@@a741, -41)
end 

module M706
    include M700, M701
    
    @@a750 = 500

    assert_equal(@@a710, 10)
    assert_equal(@@a720, 20)   # in order search, the first hit wins
    assert_equal(@@a730, -30)
    assert_equal(@@a740, -40)  
    assert_equal(@@a750, 500)
end 

# SCOPE
def read_sv
    return @@sv1
end 
class C800
    @@sv1 = 1
    def read; read_sv; end 
end 
assert_raise(NameError) { C800.new.read } 
@@sv1 = 2
assert_equal(C800.new.read, 2)