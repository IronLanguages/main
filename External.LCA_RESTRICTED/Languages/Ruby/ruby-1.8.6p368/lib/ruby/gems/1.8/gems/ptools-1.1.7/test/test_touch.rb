#####################################################################
# tc_touch.rb
#
# Test case for the File.touch method. This test should be run
# via the 'rake test_touch task'.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_FileTouch < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'      
   end
   
   def setup   
      @test_file = 'delete.this'
      @xfile = 'test_file1.txt'
   end

   def test_touch_basic
      assert_respond_to(File, :touch)
      assert_nothing_raised{ File.touch(@test_file) }
   end

   def test_touch_expected_results
      assert_equal(File, File.touch(@test_file))
      assert_equal(true, File.exists?(@test_file))
      assert_equal(0, File.size(@test_file))
   end

   def test_touch_existing_file
      stat = File.stat(@xfile)
      sleep 1
      assert_nothing_raised{ File.touch(@xfile) }
      assert_equal(true, File.size(@xfile) == stat.size)
      assert_equal(false, File.mtime(@xfile) == stat.mtime)
   end

   def test_touch_expected_errors
      assert_raises(ArgumentError){ File.touch }
   end

   def teardown
      File.delete(@test_file) if File.exists?(@test_file)
      @test_file = nil
   end
end
