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

# how to define const, and how to read it later
TOP_LEVEL_CONST = 2

assert_equal(TOP_LEVEL_CONST, 2)
assert_equal(Object::TOP_LEVEL_CONST, 2)
assert_raise(NoMethodError) { Object.TOP_LEVEL_CONST }
assert_raise(TypeError) { self::TOP_LEVEL_CONST } # main is not a class/module (TypeError)

def check
    # access inside function
    assert_equal(TOP_LEVEL_CONST, 2)
    # try to declare one
    # CONST_INSIDE_FUNCTION = 3  # syntax error expected (to be added)
end 
check

class My_const
    # declare one inside class
    CONST = 1
    
    # access them inside class
    def check
        assert_equal(TOP_LEVEL_CONST, 2)
        assert_equal(Object::TOP_LEVEL_CONST, 2)
        
        assert_equal(CONST, 1)
        assert_equal(My_const::CONST, 1)
    end
    
    def check2
        CONST2
    end 
    
    # re-assign -> warning
    CONST = 1
end 

assert_equal(My_const::CONST, 1)
assert_raise(NameError) { Object::CONST } # uninitialized constant CONST (NameError)

x = My_const.new
assert_raise(NoMethodError) { x.CONST }  # undefined method `CONST' for #<My:0x764e9a0> (NoMethodError)
assert_raise(TypeError) { x::CONST }  # #<My:0x75cf2b8> is not a class/module (TypeError)
x.check

assert_raise(NameError) { x.check2 }
# define outside class
My_const::CONST2 = 345
assert_equal(x.check2, 345)

# ORDER/SCOPE

def foo
  Foo
end
Foo = 123

class Bar
  def bar
    foo
  end
  def baz
    Foo
  end
  Foo = 456
end

assert_equal(Bar.new.bar, 123)
assert_equal(Bar.new.baz, 456)
assert_equal(Bar::Foo, 456)
assert_raise(TypeError) { Bar.new::Foo }  # #<Bar:0x341b970> is not a class/module

Foo = 789   # warning
Bar::Foo = 987  # warning
assert_equal(Bar.new.bar, 789)
assert_equal(Bar.new.baz, 987)

# SCOPE

class Object
  def test_const_lookup
    Array.new(3,5)
  end
end

x = module TestModule
  # This function should look for Object::Array, not TestModule::Array
  test_const_lookup
end
assert_equal(x, [5, 5, 5])
