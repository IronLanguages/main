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

<<<<<<< Updated upstream:Merlin/Main/Languages/Ruby/Tests/Interop/test_basic.rb
test_inherit
test_monkeypatch
=======
test_unmangling
>>>>>>> Stashed changes:Merlin/Main/Languages/Ruby/Tests/Interop/test_basic.rb
test_invisible_types
