require 'test/unit'
require 'fox16'

include Fox

class TC_FXGLShape < Test::Unit::TestCase

  DELTA = 1.0e-5

  def setup
    @shape = FXGLShape.new(0.0, 0.0, 0.0, 0)
  end
  def test_getPosition
    assert_kind_of(FXVec3f, @shape.position)
  end
  def test_setPosition
    @shape.position = FXVec3f.new(0.1, 0.2, 0.3)
    assert_in_delta(0.1, @shape.position[0], DELTA)
    assert_in_delta(0.2, @shape.position[1], DELTA)
    assert_in_delta(0.3, @shape.position[2], DELTA)

    @shape.position = [0.4, 0.5, 0.6]
    assert_in_delta(0.4, @shape.position[0], DELTA)
    assert_in_delta(0.5, @shape.position[1], DELTA)
    assert_in_delta(0.6, @shape.position[2], DELTA)
  end
end

