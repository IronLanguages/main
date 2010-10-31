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

###############################################
puts 'test what can be included?'
###############################################

class C101; end 
module M101
    def m101; 1; end 
end 
module M102
    def m102; 2; end 
end 
module M103
    def m103; 3; end 
end 
module M104
    def m104; 4; end 
end 

module M105
#    assert_raise(ArgumentError, "cyclic include detected") { include M105 }
end

class C100
#    assert_raise(TypeError, "wrong argument type nil (expected Module)") { include nil }
#    assert_raise(TypeError, "wrong argument type Fixnum10 (expected Module)") { include 1 }
#    assert_raise(TypeError, "wrong argument type String (expected Module)") { include "str" }
    assert_raise(TypeError, "wrong argument type Class (expected Module)") { include C101 }
    
    ## atomic: M101 is not mixed in
    assert_raise(TypeError, "wrong argument type Class (expected Module)") { include M101, C101 }  
    assert_false { C100.include? M101 }
   
    # include a list
    include M102
    include M102, M103  # list
    include M103, M103  # twice
end

x = C100.new 
assert_raise(NoMethodError) { x.m101 }
assert_equal(x.m102, 2)
assert_equal(x.m103, 3)

###############################################
puts "test where can 'include' be used?"
###############################################

#assert_raise(NameError) { m104 }  # !!!

include M104

assert_equal(m104, 4)
assert_equal(x.m104, 4)
assert_equal(C100.m104, 4)

###############################################
puts "test return value"
###############################################

module M301; end 
class C300
    $x = include M301           # !!!
    assert_equal($x, C300)
end 

###############################################
puts "test transitive mix-in"
###############################################

module M201
    def m201; 1; end 
end 
module M202
    include M201
    def m202; 2; end 
end 
module M203
    def m203; 3; end 
end 
module M204
    include M203
    def m204; 4; end 
end 
module M205
    include M202, M204
    def m205; 5; end 
end     
    
class C200
    include M205
end 
x = C200.new 
assert_equal(x.m201, 1)
assert_equal(x.m202, 2)
assert_equal(x.m203, 3)
assert_equal(x.m204, 4)
assert_equal(x.m205, 5)
assert_raise(NoMethodError) { x.m206 }

assert_true { C200.include? M201 }
assert_false { M204.include? M204 }
assert_true { M202.include? M201 }

###############################################
puts "test changes from the dependent modules"
###############################################

module M201
    def m211; 11; end 
end 
module M207
    def m207; 7; end  
end 
module M204
    include M207     # no impact on C200, has impact on M204
    def m214; 14; end 
end 
assert_equal(x.m211, 11)
assert_equal(x.m214, 14)
assert_raise(NoMethodError) { x.m207 }
assert_true  { M204.include? M207 }
assert_false { C200.include? M207 }
    
###############################################
puts "test context"
###############################################

module M401
    def m401; 1; end   
end
module M402
    def m402; 2; end 
end 
module M403
    def m403; 3; end 
end 

module M404
    module M405
        include M403
        def m405; 5; end 
    end
    def m404; 4; end 
end 
class C410
    class C420
        include M402
    end
    class C430
        include M404        
    end
    class C440 
        include M404::M405
    end  
    include M401    
end

{   C410.new => [1, 0, 0, 0, 0], 
    C410::C420.new => [0, 2, 0, 0, 0], 
    C410::C430.new => [0, 0, 0, 4, 0], 
    C410::C440.new => [0, 0, 3, 0, 5], 
}.each do |k, v|
    v.each do |result|
        if result != 0
            assert_equal(k.send("m40#{result}"), result)
        else
            assert_raise(NoMethodError) { k.send("m40#{result}") }
        end 
    end 
end 

#assert_equal(C410.ancestors, [C410, M401, Object, M104, Kernel])
#assert_equal(C410::C420.ancestors, [C410::C420, M402, Object, M104, Kernel])
#assert_equal(C410::C440.ancestors, [C410::C440, M404::M405, M403, Object, M104, Kernel])

###############################################
puts "test include Kernel"
###############################################

class C500
    include Kernel
end 
#assert_equal(C500.new == 3, false)

module M510
    include Kernel
end
class C520
    include M510
end 
#assert_equal(C520.new == 3, false)

class C530
end 

include Kernel
assert_equal(C530.new == 3, false)
