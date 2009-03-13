require 'test/unit'
require 'fox16'

include Fox

class TC_FXSegment < Test::Unit::TestCase
  def test_new
    aSeg = FXSegment.new
    assert_equal(0, aSeg.x1)
    assert_equal(0, aSeg.y1)
    assert_equal(0, aSeg.x2)
    assert_equal(0, aSeg.y2)
  end
  def test_new_with_values
    aSeg = FXSegment.new(1, 2, 3, 4)
    assert_equal(1, aSeg.x1)
    assert_equal(2, aSeg.y1)
    assert_equal(3, aSeg.x2)
    assert_equal(4, aSeg.y2)
  end
end

