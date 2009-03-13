require 'fox16'
require 'test/unit'
require 'testcase'

include Fox

class TC_FXBMPImage < TestCase
  def setup
    super(self.class.name)
  end

  def test_fileExt
    assert_equal("bmp", FXBMPImage.fileExt)
  end
end
