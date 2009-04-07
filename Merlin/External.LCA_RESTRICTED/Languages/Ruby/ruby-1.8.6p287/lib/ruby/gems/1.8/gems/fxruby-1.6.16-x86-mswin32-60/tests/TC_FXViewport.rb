require 'test/unit'

require 'fox16'

include Fox

class TC_FXViewport < Test::Unit::TestCase
  def setup
    @viewport = FXViewport.new
  end

  def testAttributes
    assert(@viewport.w)
    assert_kind_of(Integer, @viewport.w)
    assert(@viewport.h)
    assert_kind_of(Integer, @viewport.h)
    assert(@viewport.left)
    assert_kind_of(Float, @viewport.left)
    assert(@viewport.right)
    assert_kind_of(Float, @viewport.right)
    assert(@viewport.bottom)
    assert_kind_of(Float, @viewport.bottom)
    assert(@viewport.top)
    assert_kind_of(Float, @viewport.top)
    assert(@viewport.hither)
    assert_kind_of(Float, @viewport.hither)
    assert(@viewport.yon)
    assert_kind_of(Float, @viewport.yon)
  end
end
