############################################################################
# test_win32_api.rb
# 
# Test case for the Win32::API class. You should run this as Rake task,
# i.e. 'rake test', instead of running it directly.
############################################################################
require 'win32/api'
require 'test/unit'
include Win32

class TC_Win32_API < Test::Unit::TestCase
   def setup
      @buf = 0.chr * 260
      @gcd = API.new('GetCurrentDirectory', 'LP')
      @gle = API.new('GetLastError', 'V', 'L')
      @str = API.new('strstr', 'PP', 'P', 'msvcrt')
   end

   def test_version
      assert_equal('1.2.1', API::VERSION)
   end

   def test_constructor_basic
      assert_nothing_raised{ API.new('GetCurrentDirectory') }
      assert_nothing_raised{ API.new('GetCurrentDirectory', 'LP') }
      assert_nothing_raised{ API.new('GetCurrentDirectory', 'LP', 'L') }
      assert_nothing_raised{ API.new('GetCurrentDirectory', 'LP', 'L', 'kernel32') }
   end
 
   def test_call
      assert_respond_to(@gcd, :call)
      assert_nothing_raised{ @gcd.call(@buf.length, @buf) }
      assert_equal(Dir.pwd.tr('/', "\\"), @buf.strip)
   end
   
   def test_call_with_void
      assert_nothing_raised{ @gle.call }
      assert_nothing_raised{ @gle.call(nil) }
   end
   
   def test_dll_name
      assert_respond_to(@gcd, :dll_name)
      assert_equal('kernel32', @gcd.dll_name)
   end
   
   def test_function_name
      assert_respond_to(@gcd, :function_name)
      assert_equal('GetCurrentDirectory', @gcd.function_name)
      assert_equal('strstr', @str.function_name)
   end
   
   def test_effective_function_name
      assert_respond_to(@gcd, :effective_function_name)
      assert_equal('GetCurrentDirectoryA', @gcd.effective_function_name)
      assert_equal('strstr', @str.effective_function_name)

      @gcd = API.new('GetCurrentDirectoryA', 'LP')
      assert_equal('GetCurrentDirectoryA', @gcd.effective_function_name)

      @gcd = API.new('GetCurrentDirectoryW', 'LP')
      assert_equal('GetCurrentDirectoryW', @gcd.effective_function_name)
   end
   
   def test_prototype
      assert_respond_to(@gcd, :prototype)
      assert_equal(['L', 'P'], @gcd.prototype)
   end
   
   def test_return_type
      assert_respond_to(@gcd, :return_type)
      assert_equal('L', @gcd.return_type)
   end
   
   def test_constructor_high_iteration
      1000.times{
         assert_nothing_raised{ API.new('GetUserName', 'P', 'P', 'advapi32') }
      }
   end
   
   def test_constructor_expected_failures
      assert_raise(ArgumentError){ API.new }
      assert_raise(API::Error){ API.new('GetUserName', 'PL', 'I', 'foo') }
      assert_raise(API::Error){ API.new('GetUserName', 'X', 'I', 'kernel32') }
      assert_raise(API::Error){ API.new('GetUserName', 'PL', 'X', 'kernel32') }
   end

   # Compare MSVCRT error messages vs regular error messages. This validates
   # that we're skipping the 'A' and 'W' lookups for MSVCRT functions.
   #
   # The JRuby test message is somewhat different because we're not using
   # GetLastError().
   #
   def test_constructor_expected_failure_messages
      begin
         API.new('GetBlah')
      rescue API::Error => err
         if RUBY_PLATFORM.match('java')
            expected = "Unable to load function 'GetBlah', 'GetBlahA', or 'GetBlahW'"
         else
            expected = "GetProcAddress() failed for 'GetBlah', 'GetBlahA' and "
            expected += "'GetBlahW': The specified procedure could not be found."
         end
         assert_equal(expected, err.to_s)
      end

      begin
         API.new('strxxx', 'P', 'P', 'msvcrt')
      rescue API::Error => err
         if RUBY_PLATFORM.match('java')
            expected = "Unable to load function 'strxxx'"
         else
            expected = "GetProcAddress() failed for 'strxxx': The specified "
            expected += "procedure could not be found."
         end
         assert_equal(expected, err.to_s)
      end
   end
   
   def test_call_expected_failures
      assert_raise(TypeError){ @gcd.call('test', @buf) }
   end
   
   def teardown
      @buf = nil
      @gcd = nil
      @gle = nil
      @str = nil
   end
end
