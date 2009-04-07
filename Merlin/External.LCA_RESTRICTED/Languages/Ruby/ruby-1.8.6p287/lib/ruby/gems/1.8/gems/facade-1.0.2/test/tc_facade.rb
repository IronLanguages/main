#########################################################################
# tc_facade.rb
#
# Test suite for the Facade module. This test suite should be run via
# the 'rake test' task.
#########################################################################
require "test/unit"
require "facade"

module Baz
   def testme(str)
      str
   end
end

class Foo < String
   extend Facade
   facade File, :basename, "dirname"
   facade Dir
   facade Baz
   def blockdev?
      "test"
   end
end

class TC_Facade < Test::Unit::TestCase
   def setup
      @f = Foo.new("/home/djberge")
   end
   
   def test_facade_version
      assert_equal('1.0.2', Facade::FACADE_VERSION)
   end

   def test_file_methods
      assert_respond_to(@f, :basename)
      assert_respond_to(@f, :dirname)
      assert_raises(NoMethodError){ @f.exists? }
      assert_raises(NoMethodError){ @f.chardev? }
   end

   def test_file_method_return_values
      assert_equal("djberge", @f.basename)
      assert_equal("/home", @f.dirname)
   end

   def test_dir_methods
      assert_respond_to(@f, :pwd)
      assert_respond_to(@f, :entries)
   end
   
   def test_no_clobber
      assert_respond_to(@f, :blockdev?)
      assert_equal("test", @f.blockdev?)
   end

   def test_module_methods
      assert_respond_to(@f, :testme)
      assert_equal("/home/djberge", @f.testme)
   end

   def teardown
      @f = nil
   end
end
