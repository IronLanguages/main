require 'fox16'
require 'test/unit'
require 'testcase'

include Fox

class TC_FXXBMIcon < TestCase
  def setup
    super(self.class.name)
  end

  def test_fileExt
    assert_equal("xbm", FXXBMIcon.fileExt)
  end
end
