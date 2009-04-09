#####################################################################
# tc_msvcrt_string.rb
#
# Test case for the Windows::MSVCRT::String module.
#####################################################################
require 'windows/msvcrt/string'
require 'test/unit'

class TC_Windows_MSVCRT_String < Test::Unit::TestCase
   include Windows::MSVCRT::String

   def setup
      @buf = 0.chr * 260
   end

   def test_method_constants
      assert_not_nil(Strcmp)
      assert_not_nil(Strcpy)
      assert_not_nil(Strcspn)
      assert_not_nil(Strlen)
      assert_not_nil(Strncpy)
      assert_not_nil(Strrchr)
      assert_not_nil(Strrev)
      assert_not_nil(Strtok)
   end

   def test_strchr
      assert_respond_to(self, :strchr)
      assert_equal('llo', strchr('hello', 'l'[0]))
      assert_equal(nil, strchr('hello', 'x'[0]))
   end

   def test_strchr_with_zero
      assert_nil(strchr(0, 'l'[0]))
      assert_nil(strchr('hello', 0))
   end

   def test_strchr_expected_errors
      assert_raise(ArgumentError){ strchr }
      assert_raise(ArgumentError){ strchr('hello') }
   end

   def test_strcmp
      assert_respond_to(self, :strcmp)
      assert_equal(-1, strcmp('alpha', 'beta'))
      assert_equal(1, strcmp('beta', 'alpha'))
      assert_equal(0, strcmp('alpha', 'alpha'))
   end

   def test_strcmp_expected_errors
      assert_raise(ArgumentError){ strcmp }
      assert_raise(ArgumentError){ strcmp('alpha') }
   end

   def test_strcpy
      assert_respond_to(self, :strcpy)
      assert_kind_of(Fixnum, strcpy(@buf, ['hello'].pack('p*').unpack('L')[0]))
      assert_equal('hello', @buf.strip)
   end

   def test_strcspn
      assert_respond_to(self, :strcspn)
      assert_equal(3, strcspn('abcxyz123', '&^(x'))
      assert_equal(9, strcspn('abcxyz123', '&^(('))
   end

   def test_strcspn_expected_errors
      assert_raise(ArgumentError){ strcspn }
      assert_raise(ArgumentError){ strcspn('alpha') }
   end

   def test_strlen
      assert_respond_to(self, :strlen)
      assert_equal(5, strlen('hello'))
      assert_equal(0, strlen(''))
   end

   def test_strlen_expected_errors
      assert_raise(ArgumentError){ strlen }
      assert_raise(ArgumentError){ strlen('a', 'b') }
   end

   def test_strncpy
      assert_respond_to(self, :strncpy)
      assert_equal('alp', strncpy(@buf, 'alpha', 3))
      assert_equal('alp', @buf.strip)
   end

   def teardown
      @buf = nil
   end
end
