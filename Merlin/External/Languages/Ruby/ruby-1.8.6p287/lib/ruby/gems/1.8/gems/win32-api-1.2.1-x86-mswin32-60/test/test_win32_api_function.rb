########################################################################
# test_win32_api_function.rb
#
# Test case for the Win32::API::Function class. You should run these
# tests via the 'rake test' task.
########################################################################
require 'test/unit'
require 'win32/api'

class TC_Win32_API_Function < Test::Unit::TestCase
   def setup
      @func = Win32::API::Function.new(123456789, 'LP', 'L')
   end

   def test_constructor
      assert_nothing_raised{ Win32::API::Function.new(1) }
      assert_nothing_raised{ Win32::API::Function.new(1, 'LL') }
      assert_nothing_raised{ Win32::API::Function.new(1, 'LL', 'I') }
   end
   
   def test_subclass
      assert_kind_of(Win32::API, @func)
      assert_respond_to(@func, :call)
   end

   def test_address
      assert_respond_to(@func, :address)
      assert_equal(123456789, @func.address)
   end

   def test_prototype
      assert_respond_to(@func, :prototype)
      assert_equal(['L', 'P'], @func.prototype)
   end

   def test_return_type
      assert_respond_to(@func, :return_type)
      assert_equal('L', @func.return_type)
   end

   def test_expected_errors
      assert_raise(ArgumentError){ Win32::API::Function.new }
   end

   def teardown
      @func = nil
   end
end
