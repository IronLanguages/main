###########################################################################
# test_win32_open3.rb
#
# Test suite for the win32-open3 library.  Except for the
# 'test_open3_with_arguments' test and Open4 tests, this suite passes
# on Unix as well.
#
# You should run this test suite via the 'rake test' task.
###########################################################################
require 'win32/open3'
require 'test/unit'

class TC_Win32_Open3 < Test::Unit::TestCase
   def setup
      @good_cmd = 'ver'
      @bad_cmd  = 'verb'
      @stdin    = nil
      @stdout   = nil
      @stderr   = nil
   end

   def test_open3_version
      assert_equal('0.2.8', Open3::WIN32_OPEN3_VERSION)
      assert_equal('0.2.8', Open4::WIN32_OPEN3_VERSION)
   end
   
   def test_open3_basic
      assert_respond_to(Open3, :popen3)
      assert_nothing_raised{ Open3.popen3(@good_cmd) }
      assert_nothing_raised{ Open3.popen3(@bad_cmd) }
   end
   
   def test_open4_basic
      assert_respond_to(Open4, :popen4)
      assert_nothing_raised{ Open4.popen4(@good_cmd) }
      assert_nothing_raised{ Open4.popen4(@bad_cmd) }
   end
   
   # This test would fail on other platforms
   def test_open3_with_arguments
      assert_nothing_raised{ Open3.popen3(@good_cmd, 't') }
      assert_nothing_raised{ Open3.popen3(@bad_cmd, 't') }
      assert_nothing_raised{ Open3.popen3(@good_cmd, 'b') }
      assert_nothing_raised{ Open3.popen3(@bad_cmd, 'b') }
      assert_nothing_raised{ Open3.popen3(@good_cmd, 't', false) }
      assert_nothing_raised{ Open3.popen3(@good_cmd, 't', true) }
   end
   
   def test_open3_handles
      arr = Open3.popen3(@good_cmd)
      assert_kind_of(Array, arr)
      assert_kind_of(IO, arr[0])
      assert_kind_of(IO, arr[1])
      assert_kind_of(IO, arr[2])
   end
   
   def test_open3_block
      assert_nothing_raised{ Open3.popen3(@good_cmd){ |pin, pout, perr| } }
      Open3.popen3(@good_cmd){ |pin, pout, perr|
         assert_kind_of(IO, pin)
         assert_kind_of(IO, pout)
         assert_kind_of(IO, perr)
      }
   end
   
   def test_open4_block
      assert_nothing_raised{ Open4.popen4(@good_cmd){ |pin, pout, perr, pid| } }
      Open4.popen4(@good_cmd){ |pin, pout, perr, pid|
         assert_kind_of(IO, pin)
         assert_kind_of(IO, pout)
         assert_kind_of(IO, perr)
         assert_kind_of(Fixnum, pid)
      }
   end
   
   def test_open4_return_values
      arr = Open4.popen4(@good_cmd)
      assert_kind_of(Array,arr)
      assert_kind_of(IO, arr[0])
      assert_kind_of(IO, arr[1])
      assert_kind_of(IO, arr[2])
      assert_kind_of(Fixnum, arr[3])
   end
   
   def test_handle_good_content
      fin, fout, ferr = Open3.popen3(@good_cmd)
      assert_kind_of(String, fout.gets)
      assert_nil(ferr.gets)
   end

   def test_handle_bad_content
      fin, fout, ferr = Open3.popen3(@bad_cmd)
      assert_kind_of(String, ferr.gets)
      assert_nil(fout.gets)
   end
   
   def teardown
      @good_cmd = nil
      @bad_cmd  = nil
   end
end
