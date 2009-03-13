#####################################################################
# tc_msvcrt_buffer.rb
#
# Test case for the Windows::MSVCRT::Buffer module.
#####################################################################
require "windows/msvcrt/buffer"
require "test/unit"

class BufferFoo
   include Windows::MSVCRT::Buffer
end

class TC_Windows_MSVCRT_Buffer < Test::Unit::TestCase
   def setup
      @foo = BufferFoo.new
   end
   
   def test_method_constants
      assert_not_nil(BufferFoo::Memcpy)
      assert_not_nil(BufferFoo::MemcpyPLL)
      assert_not_nil(BufferFoo::MemcpyLPL)
      assert_not_nil(BufferFoo::MemcpyLLL)
      assert_not_nil(BufferFoo::MemcpyPPL)
      assert_not_nil(BufferFoo::Memccpy)
      assert_not_nil(BufferFoo::Memchr)
      assert_not_nil(BufferFoo::Memcmp)
      assert_not_nil(BufferFoo::Memicmp)
      assert_not_nil(BufferFoo::Memmove)
      assert_not_nil(BufferFoo::Memset)
      assert_not_nil(BufferFoo::Swab)
   end

   def test_memcpy
      assert_respond_to(@foo, :memcpy)
   end
   
   def test_memccpy
      assert_respond_to(@foo, :memccpy)
   end
   
   def test_memchr
      assert_respond_to(@foo, :memchr)
   end
   
   def test_memcmp
      assert_respond_to(@foo, :memcmp)
   end
   
   def test_memicmp
      assert_respond_to(@foo, :memicmp)
   end
   
   def test_memmove
      assert_respond_to(@foo, :memmove)
   end
   
   def test_memset
      assert_respond_to(@foo, :memset)
   end
   
   def test_swab
      assert_respond_to(@foo, :swab)
   end
   
   def teardown
      @foo  = nil
   end
end
