#####################################################################
# tc_clipboard.rb
#
# Test case for the Windows::Clipboard module.
#####################################################################
require "windows/clipboard"
require "test/unit"

class ClipboardFoo
   include Windows::Clipboard
end

class TC_Windows_Clipboard < Test::Unit::TestCase
   def setup
      @foo = ClipboardFoo.new
   end
   
   def test_numeric_constants
      assert_equal(1, ClipboardFoo::CF_TEXT)
      assert_equal(2, ClipboardFoo::CF_BITMAP)
      assert_equal(3, ClipboardFoo::CF_METAFILEPICT)
      assert_equal(4, ClipboardFoo::CF_SYLK)
      assert_equal(5, ClipboardFoo::CF_DIF)
      assert_equal(6, ClipboardFoo::CF_TIFF)
      assert_equal(7, ClipboardFoo::CF_OEMTEXT)
      assert_equal(8, ClipboardFoo::CF_DIB)
      assert_equal(9, ClipboardFoo::CF_PALETTE)
      assert_equal(10, ClipboardFoo::CF_PENDATA)
      assert_equal(11, ClipboardFoo::CF_RIFF)
      assert_equal(12, ClipboardFoo::CF_WAVE)
      assert_equal(13, ClipboardFoo::CF_UNICODETEXT)
      assert_equal(14, ClipboardFoo::CF_ENHMETAFILE)
   end
   
   def test_method_constants
      assert_not_nil(ClipboardFoo::OpenClipboard)
      assert_not_nil(ClipboardFoo::CloseClipboard)
      assert_not_nil(ClipboardFoo::GetClipboardData)
      assert_not_nil(ClipboardFoo::EmptyClipboard)
      assert_not_nil(ClipboardFoo::SetClipboardData)
      assert_not_nil(ClipboardFoo::CountClipboardFormats)
      assert_not_nil(ClipboardFoo::IsClipboardFormatAvailable)
      assert_not_nil(ClipboardFoo::GetClipboardFormatName)
      assert_not_nil(ClipboardFoo::EnumClipboardFormats)
      assert_not_nil(ClipboardFoo::RegisterClipboardFormat)
   end
   
   def teardown
      @foo = nil
   end
end
