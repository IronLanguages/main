require 'test/unit'
require 'fox16'
require 'testcase'

include Fox

class TC_FXGLViewer < TestCase
  def setup
    super(self.class.name)
    vis = FXGLVisual.new(app, VISUAL_DOUBLEBUFFER)
    @viewer = FXGLViewer.new(mainWindow, vis)
  end
=begin
  def test_readPixels
    pixels = @viewer.readPixels(0, 0, @viewer.width, @viewer.height)
    assert(pixels)
    assert_equal(3*@viewer.width*@viewer.height, pixels.size)
  end
=end
end
