require 'fox16'
require 'test/unit'
require 'testcase'

include Fox

class TC_FXXPMIcon < TestCase
  def setup
    super(self.class.name)
  end

  def test_fileExt
    assert_equal("xpm", FXXPMIcon.fileExt)
  end
end
