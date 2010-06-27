#####################################################################
# test_wc.rb
#
# Test case for the File.wc method. This test should be run via
# the 'rake test_wc' task.
#####################################################################
require 'test/unit'
require 'ptools'

class TC_FileWC < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') if File.exists?('test')      
   end
   
   def setup     
      @test_file = 'test_file1.txt'
   end

   def test_wc_basic
      assert_respond_to(File, :wc)
      assert_nothing_raised{ File.wc(@test_file) }
      assert_nothing_raised{ File.wc(@test_file, 'bytes') }
      assert_nothing_raised{ File.wc(@test_file, 'chars') }
      assert_nothing_raised{ File.wc(@test_file, 'words') }
      assert_nothing_raised{ File.wc(@test_file, 'LINES') }
   end

   def test_wc_results
      assert_kind_of(Array, File.wc(@test_file))
      assert_equal([166,166,25,25], File.wc(@test_file))
      assert_equal(166, File.wc(@test_file,'bytes'), "Wrong number of bytes")
      assert_equal(166, File.wc(@test_file,'chars'), "Wrong number of chars")
      assert_equal(25,  File.wc(@test_file,'words'), "Wrong number of words")
      assert_equal(25,  File.wc(@test_file,'lines'), "Wrong number of lines")
   end

   def test_wc_expected_errors
      assert_raises(ArgumentError){ File.wc }
      assert_raises(ArgumentError){ File.wc(@test_file, "bogus") }
   end

   def teardown
      @test_file = nil
   end
end
