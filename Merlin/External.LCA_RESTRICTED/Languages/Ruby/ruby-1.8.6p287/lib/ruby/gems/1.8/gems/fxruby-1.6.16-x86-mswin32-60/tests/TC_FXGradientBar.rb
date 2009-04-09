require 'test/unit'
require 'fox16'
require 'testcase'

include Fox

class TC_FXGradientBar < TestCase
  def setup
    super(self.class.name)
    @gradientBar = FXGradientBar.new(mainWindow)
  end
  def test_getSegment
  end
  def test_getGrip
  end
  def test_getNumSegments
  end
  def test_setGradients
  end
  def test_getGradients
  end
  def test_setCurrentSegment
  end
  def test_getCurrentSegment
  end
  def test_setAnchorSegment
  end
  def test_getAnchorSegment
  end
  def test_selectSegments
  end
  def test_deselectSegments
  end
  def test_isSegmentSelected
  end
  def test_setSegmentLowerColor
  end
  def test_setSegmentUpperColor
  end
  def test_getSegmentLowerColor
  end
  def test_getSegmentUpperColor
  end
  def test_moveSegmentLower
  end
  def test_moveSegmentMiddle
  end
  def test_moveSegmentUpper
  end
  def test_moveSegments
  end
  def test_getSegmentLower
  end
  def test_getSegmentMiddle
  end
  def test_getSegmentUpper
  end
  def test_gradient
    emptyRamp = @gradientBar.gradient(0)
    assert_kind_of(Array, emptyRamp)
    assert(emptyRamp.empty?)
  end
  def test_getSegmentBlend
  end
  def test_splitSegments
  end
  def test_mergeSegments
  end
  def test_uniformSegments
  end
  def test_blendSegments
  end
  def test_barStyle
    @gradientBar.barStyle = 0
    assert_equal(0, @gradientBar.barStyle)
  end
  def test_selectColor
    @gradientBar.selectColor = FXRGB(255, 0, 255)
    assert_equal(FXRGB(255, 0, 255), @gradientBar.selectColor)
  end
  def test_helpText
    @gradientBar.helpText = "helpText"
    assert_equal("helpText", @gradientBar.helpText)
  end
  def test_tipText
    @gradientBar.tipText = "tipText"
    assert_equal("tipText", @gradientBar.tipText)
  end
end
