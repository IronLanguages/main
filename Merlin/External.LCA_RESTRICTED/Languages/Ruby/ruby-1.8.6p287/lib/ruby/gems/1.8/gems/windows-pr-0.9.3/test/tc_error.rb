#####################################################################
# tc_error.rb
#
# Test case for the Windows::Error module.
#####################################################################
require "windows/error"
require "test/unit"

class ErrorFoo
   include Windows::Error
end

class TC_Windows_Error < Test::Unit::TestCase
  
   def setup
      @foo = ErrorFoo.new
   end
   
   def test_numeric_constants
      assert_equal(0x00000100, ErrorFoo::FORMAT_MESSAGE_ALLOCATE_BUFFER)
      assert_equal(0x00000200, ErrorFoo::FORMAT_MESSAGE_IGNORE_INSERTS)
      assert_equal(0x00000400, ErrorFoo::FORMAT_MESSAGE_FROM_STRING)
      assert_equal(0x00000800, ErrorFoo::FORMAT_MESSAGE_FROM_HMODULE)
      assert_equal(0x00001000, ErrorFoo::FORMAT_MESSAGE_FROM_SYSTEM)
      assert_equal(0x00002000, ErrorFoo::FORMAT_MESSAGE_ARGUMENT_ARRAY) 
      assert_equal(0x000000FF, ErrorFoo::FORMAT_MESSAGE_MAX_WIDTH_MASK) 
      assert_equal(0x0001, ErrorFoo::SEM_FAILCRITICALERRORS)
      assert_equal(0x0004, ErrorFoo::SEM_NOALIGNMENTFAULTEXCEPT)
      assert_equal(0x0002, ErrorFoo::SEM_NOGPFAULTERRORBOX)
      assert_equal(0x8000, ErrorFoo::SEM_NOOPENFILEERRORBOX)
   end
   
   def test_method_constants
      assert_not_nil(ErrorFoo::GetLastError)
      assert_not_nil(ErrorFoo::SetLastError)
      assert_not_nil(ErrorFoo::SetLastErrorEx) # Ignore for VC++ 6 or earlier.
      assert_not_nil(ErrorFoo::SetErrorMode)
      assert_not_nil(ErrorFoo::FormatMessage)
   end
   
   def test_get_last_error
      assert_respond_to(@foo, :get_last_error)
      assert_nothing_raised{ @foo.get_last_error }
      assert_kind_of(String, @foo.get_last_error)
   end
   
   def teardown
      @foo = nil
   end
end