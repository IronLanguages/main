#####################################################################
# test_which.rb
#
# Test case for the File.which method. You should run this test
# via the 'rake test_which' rake task.
#
# NOTE: I make the assumption that Ruby (or JRuby) is in your
# PATH for these tests.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'rbconfig'
require 'ptools'
include Config

class TC_FileWhich < Test::Unit::TestCase
   def setup
      @ruby = RUBY_PLATFORM.match('java') ? 'jruby' : 'ruby'
      @exe = File.join(CONFIG["bindir"], CONFIG["ruby_install_name"]) 

      if Config::CONFIG['host_os'].match('mswin')
         @exe.tr!('/','\\')
         @exe << ".exe"
      end
   end

   def test_which_basic
      assert_respond_to(File, :which)
      assert_nothing_raised{ File.which(@ruby) }
      assert_nothing_raised{ File.which(@ruby, "/usr/bin:/usr/local/bin") }
   end

   def test_which_expected_return_values
      assert_kind_of(String, File.which(@ruby))
      assert_equal(@exe, File.which(@ruby))
      assert_equal(nil, File.which(@ruby, "/bogus/path"))
      assert_equal(nil, File.which("blahblah"))
   end

   def test_which_expected_errors
      assert_raises(ArgumentError){ File.which }
      assert_raises(ArgumentError){ File.which(@ruby, "foo", "bar") }
   end

   def teardown
      @exe  = nil
      @ruby = nil
   end
end
