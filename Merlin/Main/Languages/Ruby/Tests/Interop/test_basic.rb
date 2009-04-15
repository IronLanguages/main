# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require "../util/assert.rb"

require "mscorlib"

  # TODO: more interesting tests when more features of .NET interop are working
  class Bob_test_inherit < System::Collections::ArrayList
    def foo
      count
    end
  end

def test_inherit

  a = Bob_test_inherit.new
  a.add 1
  a.Add 2
  a.add 3
  assert_equal(a.foo, 3)
  assert_equal(a.Count, 3)

end

class System::Collections::ArrayList
        def total
            sum = 0
            each { |i| sum += i }
            sum
        end
end

def test_monkeypatch
  a = System::Collections::ArrayList.new
  
  b = System::Collections::ArrayList.new
  a.add 3
  a << 2 << 1
  assert_equal(a.total, 6)
  b.replace [4,5,6]
  assert_equal(b.total, 15)
end
  
def test_unmangling
  max = 2147483647
  assert_equal(System::Int32.MaxValue, max)
  assert_equal(System::Int32.max_value, max)
  
  # Can't unmangle names with leading, trailing, or consecutive underscores
  assert_raise(NoMethodError) { System::Int32.max_value_ }
  assert_raise(NoMethodError) { System::Int32._max_value }
  assert_raise(NoMethodError) { System::Int32.max__value }
  assert_raise(NoMethodError) { System::Int32.MaxValue_ }
  assert_raise(NoMethodError) { System::Int32._MaxValue }
  
  # Also can't unmangle names with uppercase letters
  assert_raise(NoMethodError) { System::Int32.maxValue }
  assert_raise(NoMethodError) { System::Int32.max_Value }
  assert_raise(NoMethodError) { System::Int32.Maxvalue }
  assert_raise(NoMethodError) { System::Int32.Max_value }  
end

def test_invisible_types
  # we should be able to call methods on a type
  # that's not visible--as long as the method itself is
  # on a visible type somewhere in the heirarchy
  
  # Test returning a non-visible type (RuntimeType in this case)
  type = System::Type.get_type('System.Int32'.to_clr_string)
  
  # Calling properties/methods on a non-visible type
  assert_equal(type.full_name, 'System.Int32'.to_clr_string)
  assert_equal(type.is_assignable_from(type), true)
end

def test_generics_give_type_error
  assert_raise(TypeError) do
    Class.new(System::Collections::Generic::List)
  end
  assert_raise(TypeError) do
    Class.new do
      include System::Collections::Generic::List
    end
  end
end

def test_include_interface_after_type_creation
  c = Class.new do
    include System::Collections::Generic::List[System::Int32]
  end

  # No object has been instantiated so we should be able to reopen the class and add a new interface
  c.class_eval do
    include System::IDisposable
  end

  # After instantiating the object, this will give us an error
  tmp = c.new
  assert_raise(TypeError) do
    c.class_eval do
      include System::IDisposable
    end
  end
end

test_ilist
test_inherit
test_monkeypatch
test_unmangling
test_invisible_types
