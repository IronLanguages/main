#####################################################################
# tc_msvcrt_time.rb
#
# Test case for the Windows::MSVCRT::Time module.
#####################################################################
require 'windows/msvcrt/time'
require 'test/unit'

class MTimeFoo
   include Windows::MSVCRT::Time
end

class TC_Windows_MSVCRT_Time < Test::Unit::TestCase
   def setup
      @foo = MTimeFoo.new
   end
   
   def test_method_constants
      assert_not_nil(MTimeFoo::Asctime)
   end
   
   def test_asctime
      assert_respond_to(@foo, :asctime)
   end
   
   def teardown
      @foo  = nil
   end
end
