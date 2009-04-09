require 'fox16'
require 'test/unit'
require 'testcase'

include Fox

class TC_FXBMPIcon < TestCase
  def setup
    super(self.class.name)
  end

  def test_fileExt
    assert_equal("bmp", FXBMPIcon.fileExt)
  end
end
