#####################################################################
# test_null.rb
# 
# Test case for the File.null method. You should run this test via
# the 'rake test_null' task.
#####################################################################
require 'test/unit'
require 'ptools'

class TC_Null < Test::Unit::TestCase
   def setup
      @nulls = ['/dev/null', 'NUL', 'NIL:', 'NL:']
   end

   def test_null_basic
      assert_respond_to(File, :null)
      assert_nothing_raised{ File.null }
   end

   def test_null_expected_results
      assert_kind_of(String, File.null)
      assert(@nulls.include?(File.null))
   end

   def test_null_expected_errors
      assert_raises(ArgumentError){ File.null(1) }
   end

   def teardown
      @nulls = nil
   end
end
