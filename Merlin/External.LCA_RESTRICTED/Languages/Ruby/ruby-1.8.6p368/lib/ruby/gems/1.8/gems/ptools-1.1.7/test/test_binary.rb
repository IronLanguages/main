#####################################################################
# tc_binary.rb
# 
# Test case for the File.binary? method. You should run this test
# via the 'rake test_binary' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_Binary < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'      
   end
   
   def setup
      @text_file = 'test_file1.txt'
   end

   def test_binary_basic
      assert_respond_to(File, :binary?)
      assert_nothing_raised{ File.binary?(@text_file) }
   end

   def test_binary_expected_results
      assert_equal(false, File.binary?(@text_file))
   end

   def test_binary_expected_errors
      assert_raises(Errno::ENOENT, ArgumentError){ File.binary?('bogus') }
   end

   def teardown
      @text_file = nil
   end
end
