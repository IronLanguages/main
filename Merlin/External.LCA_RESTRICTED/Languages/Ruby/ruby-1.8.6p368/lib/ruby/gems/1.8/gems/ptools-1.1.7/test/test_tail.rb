#####################################################################
# test_tail.rb
#
# Test case for the File.tail method. This test should be run via
# the 'rake test_tail' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_FileTail < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'
   end
   
   def setup     
      @test_file = 'test_file1.txt'

      @expected_tail1 = ["line16\n","line17\n","line18\n","line19\n"]
      @expected_tail1.push("line20\n","line21\n","line22\n", "line23\n")
      @expected_tail1.push("line24\n","line25\n")

      @expected_tail2 = ["line21\n","line22\n","line23\n","line24\n","line25\n"]
   end

   def test_tail_basic
      assert_respond_to(File, :tail)
      assert_nothing_raised{ File.tail(@test_file) }
      assert_nothing_raised{ File.tail(@test_file, 5) }
      assert_nothing_raised{ File.tail(@test_file){} }
   end

   def test_tail_expected_return_values
      assert_kind_of(Array, File.tail(@test_file))
      assert_equal(@expected_tail1, File.tail(@test_file))
      assert_equal(@expected_tail2, File.tail(@test_file, 5))
   end

   def test_tail_expected_errors
      assert_raises(ArgumentError){ File.tail }
      assert_raises(ArgumentError){ File.tail(@test_file, 5, 5) }
   end

   def teardown
      @test_file = nil
      @expected_tail1 = nil
      @expected_tail2 = nil
   end
end
