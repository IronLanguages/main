#####################################################################
# test_image.rb
# 
# Test case for the File.image? method. You should run this test
# via the 'rake test_image' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_Ptools_Image < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'      
   end
   
   def setup   
      @text_file = 'test_file1.txt'
   end

   def test_image_basic
      assert_respond_to(File, :image?)
      assert_nothing_raised{ File.image?(@text_file) }
   end

   def test_image_expected_results
      assert_equal(false, File.image?(@text_file))
   end

   def test_image_expected_errors
      assert_raises(Errno::ENOENT, ArgumentError){ File.image?('bogus') }
   end

   def teardown
      @text_file = nil
   end
end
