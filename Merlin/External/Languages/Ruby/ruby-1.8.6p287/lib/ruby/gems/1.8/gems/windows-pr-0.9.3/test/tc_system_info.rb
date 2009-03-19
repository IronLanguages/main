#####################################################################
# tc_system_info.rb
#
# Test case for the Windows::SystemInfo module.
#####################################################################
require 'windows/system_info'
require 'test/unit'

class SystemInfoFoo
   include Windows::SystemInfo
end

class TC_Windows_SystemInfo < Test::Unit::TestCase
   def setup
      @foo = SystemInfoFoo.new
   end

   def test_numeric_constants
      assert_equal(386, SystemInfoFoo::PROCESSOR_INTEL_386)
      assert_equal(486, SystemInfoFoo::PROCESSOR_INTEL_486)
      assert_equal(586, SystemInfoFoo::PROCESSOR_INTEL_PENTIUM)
      assert_equal(2200, SystemInfoFoo::PROCESSOR_INTEL_IA64)
      assert_equal(8664, SystemInfoFoo::PROCESSOR_AMD_X8664)
   end
   
   def test_method_constants
      assert_not_nil(SystemInfoFoo::ExpandEnvironmentStrings)
      assert_not_nil(SystemInfoFoo::GetComputerName)
      assert_not_nil(SystemInfoFoo::GetComputerNameEx)
      assert_not_nil(SystemInfoFoo::GetSystemInfo)
   end
   
   def teardown
      @foo = nil
   end
end
