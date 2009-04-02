require 'test/unit'
require 'fox16'

include Fox

class TC_FXArc < Test::Unit::TestCase
  def test_new
    anArc = FXArc.new
    assert_equal(0, anArc.x)
    assert_equal(0, anArc.y)
    assert_equal(0, anArc.w)
    assert_equal(0, anArc.h)
    assert_equal(0, anArc.a)
    assert_equal(0, anArc.b)
  end
  def test_new_with_values
    anArc = FXArc.new(1, 2, 3, 4, 5, 6)
    assert_equal(1, anArc.x)
    assert_equal(2, anArc.y)
    assert_equal(3, anArc.w)
    assert_equal(4, anArc.h)
    assert_equal(5, anArc.a)
    assert_equal(6, anArc.b)
  end
end

