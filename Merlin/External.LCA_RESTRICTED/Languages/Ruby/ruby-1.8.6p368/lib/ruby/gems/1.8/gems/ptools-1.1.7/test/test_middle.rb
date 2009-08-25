#####################################################################
# test_middle.rb
#
# Test case for the File.middle method. You should run this test
# via the 'rake test_middle' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_FileMiddle < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'
   end
   
   def setup   
      @test_file = 'test_file1.txt'

      @expected_middle1 = ["line10\n", "line11\n", "line12\n", "line13\n", "line14\n"]
      @expected_middle1.push("line15\n","line16\n", "line17\n", "line18\n")
      @expected_middle1.push("line19\n","line20\n")

      @expected_middle2 = ["line14\n","line15\n","line16\n","line17\n"]
      @expected_middle2.push("line18\n","line19\n","line20\n")

      @expected_middle3 = ["line5\n","line6\n","line7\n"]
      @expected_middle3.push("line8\n","line9\n","line10\n")
   end

   def test_method_basic
      assert_respond_to(File, :middle)
      assert_nothing_raised{ File.middle(@test_file) }
      assert_nothing_raised{ File.middle(@test_file, 14) }
      assert_nothing_raised{ File.middle(@test_file, 5, 10) }
      assert_nothing_raised{ File.middle(@test_file){} }
   end

   def test_middle_expected_results
      assert_kind_of(Array, File.middle(@test_file))
      assert_equal(@expected_middle1, File.middle(@test_file))
      assert_equal(@expected_middle2, File.middle(@test_file, 14))
      assert_equal(@expected_middle3, File.middle(@test_file, 5, 10))
   end

   def test_middle_expected_errors
      assert_raises(ArgumentError){ File.middle }
      assert_raises(ArgumentError){ File.middle(@test_file, 5, 10, 15) }
      assert_raises(NoMethodError){ File.middle(@test_file, "foo") }
      assert_raises(Errno::ENOENT){ File.middle("bogus") }
   end

   def teardown
      @test_file = nil
      @expected_middle1 = nil
      @expected_middle2 = nil
      @expected_middle3 = nil
   end
end
