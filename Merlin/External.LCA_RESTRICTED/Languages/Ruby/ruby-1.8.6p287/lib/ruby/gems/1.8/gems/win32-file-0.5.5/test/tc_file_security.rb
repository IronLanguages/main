#####################################################################
# tc_file_security.rb
#
# Test case for security related methods of win32-file. You should
# use the 'rake test' or 'rake test_security' task to run this.
#####################################################################
require 'test/unit'
require 'win32/file'
require 'socket'

class TC_Win32_File_Security < Test::Unit::TestCase
   def setup
      @host  = Socket.gethostname
      @dir   = File.dirname(File.expand_path(__FILE__))
      @file  = File.join(@dir, 'sometestfile.txt')      
      @perms = nil
   end

   # This will fail if there is no "Users" builtin.  Not to worry.
   def test_get_permissions
      assert_respond_to(File, :get_permissions)
      assert_nothing_raised{ File.get_permissions(@file) }
      assert_nothing_raised{ File.get_permissions(@file, @host) }
      assert_kind_of(Hash, File.get_permissions(@file))
      assert(File.get_permissions(@file).has_key?('BUILTIN\\Users'))
      assert(File.get_permissions(@file).has_key?('BUILTIN\\Administrators'))
   end

   def test_set_permissions
      assert_respond_to(File, :set_permissions)
      assert_nothing_raised{ @perms = File.get_permissions(@file) }
      assert_nothing_raised{ File.set_permissions(@file, @perms) }
   end

   def test_securities
      assert_respond_to(File, :securities)
      assert_nothing_raised{ @perms = File.get_permissions(@file) }

      @perms.each{ |acct, mask|
         assert_nothing_raised{ File.securities(mask) }
         assert_kind_of(Array, File.securities(mask))
      }
   end

   def teardown
      @file  = nil
      @host  = nil
      @perms = nil
      @dir   = nil
   end
end
