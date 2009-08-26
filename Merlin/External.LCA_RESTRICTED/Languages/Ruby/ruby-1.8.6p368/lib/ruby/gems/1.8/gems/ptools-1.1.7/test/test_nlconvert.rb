#####################################################################
# test_nlconvert.rb
#
# Test case for the File.nl_convert method. You should run this
# test via the 'rake test_nlconvert' task.
#####################################################################
require 'rubygems'
gem 'test-unit'

require 'test/unit'
require 'ptools'

class TC_FileNLConvert < Test::Unit::TestCase
   def self.startup
      Dir.chdir('test') unless File.basename(Dir.pwd) == 'test'
   end
   
   def setup   
      @test_file1 = 'test_file1.txt'
      @test_file2 = 'test_file2.txt'
      @dos_file   = 'dos_test_file.txt'
      @mac_file   = 'mac_test_file.txt'
      @unix_file  = 'nix_test_file.txt'
   end

   def test_nl_convert_basic
      assert_respond_to(File, :nl_convert)
      assert_nothing_raised{ File.nl_convert(@test_file2) }
      assert_nothing_raised{ File.nl_convert(@test_file2, @test_file2) }
      assert_nothing_raised{ File.nl_convert(@test_file2, @test_file2, "unix") }
   end

   def test_nl_convert_to_dos
      msg = "dos file should be larger, but isn't"

      assert_nothing_raised{ File.nl_convert(@test_file1, @dos_file, "dos") }
      assert_equal(true, File.size(@dos_file) > File.size(@test_file1), msg)
      assert_equal(["\cM","\cJ"],
         IO.readlines(@dos_file).first.split("")[-2..-1]
      )
   end

   def test_nl_convert_to_mac
      if Config::CONFIG['host_os'].match("mswin")
         msg = "** test may fail on MS Windows **" 
      else
         msg = "** mac file should be the same size (or larger), but isn't **"
      end

      assert_nothing_raised{ File.nl_convert(@test_file1, @mac_file, "mac") }
      assert_equal(true, File.size(@mac_file) == File.size(@test_file1), msg)
      assert_equal("\cM", IO.readlines(@mac_file).first.split("").last)
   end
   
   def test_nl_convert_to_unix
      msg = "unix file should be the same size (or smaller), but isn't"

      assert_nothing_raised{ File.nl_convert(@test_file1, @unix_file, "unix") }
      assert_equal("\n", IO.readlines(@unix_file).first.split("").last)

      if File::ALT_SEPARATOR
         assert_equal(true, File.size(@unix_file) >= File.size(@test_file1),msg)
      else
         assert_equal(true, File.size(@unix_file) <= File.size(@test_file1),msg)
      end
   end
   
   def test_nl_convert_expected_errors
      assert_raises(ArgumentError){ File.nl_convert }
      assert_raises(ArgumentError){
         File.nl_convert(@test_file1, "bogus.txt", "blah")
      }
   end

   def teardown
      [@dos_file, @mac_file, @unix_file].each{ |file|
         File.delete(file) if File.exists?(file)
      }
      @dos_file   = nil
      @mac_file   = nil
      @unix_file  = nil
      @test_file1 = nil
      @test_file2 = nil
   end
end
