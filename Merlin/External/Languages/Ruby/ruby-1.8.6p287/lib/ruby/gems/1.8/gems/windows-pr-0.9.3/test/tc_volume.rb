#####################################################################
# tc_volume.rb
#
# Test case for the Windows::Volume module.
#####################################################################
require 'windows/volume'
require 'test/unit'

class VolumeFoo
   include Windows::Volume
end

class TC_Windows_Volume < Test::Unit::TestCase
   def setup
      @foo = VolumeFoo.new
   end
   
   def test_method_constants
      assert_not_nil(VolumeFoo::GetVolumeInformation)
   end
   
   def teardown
      @foo = nil
   end
end
