#####################################################################
# tc_process.rb
#
# Test case for the Windows::Process module.
#####################################################################
require 'windows/process'
require 'test/unit'

class ProcessFoo
   include Windows::Process
end

class TC_Windows_Process < Test::Unit::TestCase
   def setup
      @foo  = ProcessFoo.new
   end

   def test_numeric_constants
      assert_equal(0x1F0FFF, ProcessFoo::PROCESS_ALL_ACCESS)
   end

   def test_method_constants
      assert_not_nil(ProcessFoo::CreateProcess)
   end

   def teardown
      @foo  = nil
   end
end
