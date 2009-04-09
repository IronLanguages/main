#####################################################################
# tc_msvcrt_directory.rb
#
# Test case for the Windows::MSVCRT::File module.
#####################################################################
require 'windows/msvcrt/directory'
require 'fileutils'
require 'test/unit'

class TC_Windows_MSVCRT_Directory < Test::Unit::TestCase
   include Windows::MSVCRT::Directory

   def setup
      @pwd = Dir.pwd
      @dir = 'delete_me'
      @buf = 0.chr * 260
   end

   def test_method_constants
      assert_not_nil(Chdir)
      assert_not_nil(Wchdir)
      assert_not_nil(Chdrive)
      assert_not_nil(Getcwd)
      assert_not_nil(Wgetcwd)
      assert_not_nil(Getdcwd)
      assert_not_nil(Getdiskfree)
      assert_not_nil(Getdrive)
      assert_not_nil(Getdrives)
      assert_not_nil(Mkdir)
      assert_not_nil(Wmkdir)
      assert_not_nil(Rmdir)
      assert_not_nil(Wrmdir)
      assert_not_nil(Searchenv)
      assert_not_nil(Wsearchenv)
   end
   
   def test_chdir
      assert_respond_to(self, :chdir)
      assert_nothing_raised{ chdir('..') }
      assert_equal(File.dirname(@pwd), Dir.pwd)
   end

   def test_wchdir
      assert_respond_to(self, :wchdir)
      assert_nothing_raised{ wchdir("C:\\") }
   end

   def test_chdrive
      assert_respond_to(self, :chdrive)
   end

   def test_getcwd
      assert_respond_to(self, :getcwd)
   end

   def test_wgetcwd
      assert_respond_to(self, :wgetcwd)
   end

   def test_getdcwd
      assert_respond_to(self, :getdcwd)
   end

   def test_wgetdcwd
      assert_respond_to(self, :wgetdcwd)
   end

   def test_mkdir
      assert_respond_to(self, :mkdir)
      assert_nothing_raised{ mkdir(@dir) }
      assert_equal(true, File.exists?(@dir))
   end

   def test_rmdir
      Dir.mkdir(@dir) unless File.exists?(@dir)
      assert_respond_to(self, :rmdir)
      assert_nothing_raised{ rmdir(@dir) }
      assert_equal(false, File.exists?(@dir))
   end

   def test_searchenv
      possible = [
         "c:\\winnt\\system32\\notepad.exe",
         "c:\\windows\\system32\\notepad.exe"
      ]
      assert_respond_to(self, :searchenv)
      assert_nothing_raised{ searchenv("notepad.exe", "PATH", @buf) }
      assert_equal(true, possible.include?(@buf.strip.downcase))
   end

   def teardown
      FileUtils.rm_f(@dir) if File.exists?(@dir)
      @pwd = nil
      @dir = nil
      @buf = 0
   end
end
