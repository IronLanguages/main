#########################################################################
# test_facade.rb
#
# Test suite for the Facade module. This test suite should be run via
# the 'rake test' task.
#########################################################################
require 'test/unit'
require 'facade'

module Baz
   def testme(str)
      str
   end
end

class FooString < String
   extend Facade
   facade File, :basename, 'dirname'
   facade Dir
   facade Baz

   def blockdev?
      'test'
   end
end

class TC_Facade < Test::Unit::TestCase
   def setup
      @str = FooString.new('/home/djberge')
   end
   
   def test_facade_version
      assert_equal('1.0.4', Facade::FACADE_VERSION)
   end

   def test_file_methods
      assert_respond_to(@str, :basename)
      assert_respond_to(@str, :dirname)
      assert_raises(NoMethodError){ @str.executable? }
      assert_raises(NoMethodError){ @str.chardev? }
   end

   def test_file_method_return_values
      assert_equal('djberge', @str.basename)
      assert_equal('/home', @str.dirname)
   end

   def test_dir_methods
      assert_respond_to(@str, :pwd)
      assert_respond_to(@str, :entries)
   end
   
   def test_no_clobber
      assert_respond_to(@str, :blockdev?)
      assert_equal('test', @str.blockdev?)
   end

   def test_module_methods
      assert_respond_to(@str, :testme)
      assert_equal('/home/djberge', @str.testme)
   end

   def teardown
      @str = nil
   end
end
