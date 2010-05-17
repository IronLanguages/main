#####################################################################
# test_constants.rb
#
# Tests the constants that have been defined for our package. This
# test case should be run via the 'rake test_constants' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'rbconfig'
require 'ptools'

class TC_Constants < Test::Unit::TestCase
   def test_version
      assert_equal('1.1.7', File::PTOOLS_VERSION)
   end

   def test_image_ext
      assert_equal(%w/.bmp .gif .jpeg .jpg .png/, File::IMAGE_EXT.sort)
   end

   def test_windows
      omit_unless(Config::CONFIG['host_os'].match('mswin'), "Skipping on Unix systems")
      assert_not_nil(File::IS_WINDOWS)
   end

   def test_win32exts
      omit_unless(Config::CONFIG['host_os'].match('mswin'), "Skipping on Unix systems")
      assert_not_nil(File::WIN32EXTS)
   end
end
