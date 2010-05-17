######################################################################
# test_whereis.rb
#
# Test case for the File.whereis method. This test should be run
# via the 'rake test_whereis' task.
######################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'
require 'rbconfig'
include Config

class TC_FileWhereis < Test::Unit::TestCase
   def setup
      @expected_locs = [CONFIG['bindir']]
      
      if Config::CONFIG['host_os'].match('mswin')
         @expected_locs = ["c:\\ruby\\bin\\ruby.exe"]
      else
         @expected_locs << '/usr/local/bin/ruby'
         @expected_locs << '/opt/sfw/bin/ruby'
         @expected_locs << '/opt/bin/ruby'
         @expected_locs << '/usr/bin/ruby'
      end
   end

   def test_whereis_basic
      assert_respond_to(File, :whereis)
      assert_nothing_raised{ File.whereis("ruby") }
      assert_nothing_raised{ File.whereis("ruby","/usr/bin:/usr/local/bin") }
      assert_nothing_raised{ File.whereis("ruby"){} }
   end

   def test_whereis_expected_return_values
      msg = "You may need to adjust the setup method if this test failed"
      locs = File.whereis('ruby').map{ |e| e.downcase }
      assert_kind_of(Array, locs)
      assert(@expected_locs.include?(locs.first), msg)
      assert_equal(nil, File.whereis("blahblah"))
   end

   def test_whereis_expected_errors
      assert_raises(ArgumentError){ File.whereis }
      assert_raises(ArgumentError){ File.whereis("ruby", "foo", "bar") }
   end

   def teardown
      @expected_locs = nil
   end
end
