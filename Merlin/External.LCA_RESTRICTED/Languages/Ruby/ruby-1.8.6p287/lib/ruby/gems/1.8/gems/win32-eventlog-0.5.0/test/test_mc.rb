############################################################################
# test_mc.rb
#
# Test suite for the win32-mc library. The tests need to run in a specific
# order, hence the numerics added to the method names.
#
# This test case should be run via the 'rake test' Rakefile task.
############################################################################
require 'rubygems'
gem 'test-unit'
require 'test/unit'
require 'win32/mc'
require 'ptools'
include Win32

class TC_Win32_MC < Test::Unit::TestCase
   class << self
      def startup
         Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'
         @@mc_cmd = File.which('mc')
         @@rc_cmd = File.which('rc')
         @@link_cmd = File.which('link')
      end
      
      def shutdown
         @@mc_cmd = nil
         @@rc_cmd = nil
         @@link_cmd = nil
      end      
   end
   
   def setup 
      @mc = MC.new('foo.mc')
   end
   
   def test_01_version
      assert_equal('0.1.4', MC::VERSION)
   end

   def test_02_create_header
      omit_if(@@mc_cmd.nil?, "'mc' command not found - skipping")
      assert_respond_to(@mc, :create_header)
      assert_equal(true, @mc.create_header)
   end

   def test_03_create_res_file
      omit_if(@@rc_cmd.nil?, "'rc' command not found - skipping")
      assert_respond_to(@mc, :create_res_file)
      assert_equal(true, @mc.create_res_file)
   end
   
   def test_04_create_dll_file
      omit_if(@@link_cmd.nil?, "'link' command not found - skipping")
      assert_respond_to(@mc, :create_dll_file)
      assert_equal(true, @mc.create_dll_file)
   end
   
   def test_05_clean
      assert_respond_to(@mc, :clean)
      assert_nothing_raised{ @mc.clean }
   end

   def teardown
      @mc = nil
      File.delete('foo.dll') rescue nil
   end
end
