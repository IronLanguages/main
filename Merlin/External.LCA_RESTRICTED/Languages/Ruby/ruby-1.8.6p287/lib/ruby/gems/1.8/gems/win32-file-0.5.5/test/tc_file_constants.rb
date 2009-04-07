#############################################################################
# tc_file_constants.rb
#
# Test case for the path related methods of win32-file. Run this test via
# the 'rake test' or 'rake test_constants' task.
#############################################################################
require 'test/unit'
require 'win32/file'

class TC_Win32_File_Constants < Test::Unit::TestCase
   def test_file_attribute_constants
      assert_not_nil(File::ARCHIVE)
      assert_not_nil(File::HIDDEN)
      assert_not_nil(File::NORMAL)
      assert_not_nil(File::INDEXED)
      assert_not_nil(File::OFFLINE)
      assert_not_nil(File::READONLY)
      assert_not_nil(File::SYSTEM)
      assert_not_nil(File::TEMPORARY)
      assert_not_nil(File::CONTENT_INDEXED) # alias for INDEXED
   end
   
   def test_security_rights_constants
      assert_not_nil(File::FULL)
      assert_not_nil(File::DELETE)
      assert_not_nil(File::READ_CONTROL)
      assert_not_nil(File::WRITE_DAC)
      assert_not_nil(File::WRITE_OWNER)
      assert_not_nil(File::SYNCHRONIZE)
      assert_not_nil(File::STANDARD_RIGHTS_REQUIRED)
      assert_not_nil(File::STANDARD_RIGHTS_READ)
      assert_not_nil(File::STANDARD_RIGHTS_WRITE)
      assert_not_nil(File::STANDARD_RIGHTS_EXECUTE)
      assert_not_nil(File::STANDARD_RIGHTS_ALL)
      assert_not_nil(File::SPECIFIC_RIGHTS_ALL)
      assert_not_nil(File::ACCESS_SYSTEM_SECURITY)
      assert_not_nil(File::MAXIMUM_ALLOWED)
      assert_not_nil(File::GENERIC_READ)
      assert_not_nil(File::GENERIC_WRITE)
      assert_not_nil(File::GENERIC_EXECUTE)
      assert_not_nil(File::GENERIC_ALL)
      assert_not_nil(File::READ)
      assert_not_nil(File::CHANGE)
      assert_not_nil(File::ADD)
   end
end