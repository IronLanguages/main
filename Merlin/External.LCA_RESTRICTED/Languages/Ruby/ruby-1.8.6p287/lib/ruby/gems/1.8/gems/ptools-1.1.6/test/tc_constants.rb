#####################################################################
# tc_constants.rb
#
# Tests the constants that have been defined for our package. This
# test case should be run via the 'rake test_constants' task.
#####################################################################
require 'test/unit'
require 'ptools'

class TC_Constants < Test::Unit::TestCase
   def test_version
      assert_equal('1.1.6', File::PTOOLS_VERSION)
   end

   def test_image_ext
      assert_equal(%w/.bmp .gif .jpeg .jpg .png/, File::IMAGE_EXT.sort)
   end

   if RUBY_PLATFORM.match('mswin')
      def test_windows
         assert_not_nil(File::IS_WINDOWS)
      end

      def test_win32exts
         assert_not_nil(File::WIN32EXTS)
      end
   end
end
