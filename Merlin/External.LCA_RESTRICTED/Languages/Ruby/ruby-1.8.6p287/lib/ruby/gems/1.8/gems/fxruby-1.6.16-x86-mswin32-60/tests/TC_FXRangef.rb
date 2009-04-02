require 'test/unit'

require 'fox16'

include Fox

class TC_FXRangef < Test::Unit::TestCase

  WIDTH, HEIGHT, DEPTH = 2, 4, 6

  def setup
    @range = FXRangef.new(0, WIDTH, 0, HEIGHT, 0, DEPTH)
  end
  
  def test_width
    assert_equal(@range.width, WIDTH)
  end
  def test_height
    assert_equal(@range.height, HEIGHT)
  end
  def test_depth
    assert_equal(@range.depth, DEPTH)
  end
  def test_longest
    assert_equal([@range.width, @range.height, @range.depth].max, @range.longest)
  end
  def test_shortest
    assert_equal([@range.width, @range.height, @range.depth].min, @range.shortest)
  end
  def test_empty?
  end
  def test_overlaps?
  end
  def test_contains?
  end
  def test_include
  end
  def test_clipTo
  end
  def test_corners
  end
  def test_intersects?
  end
  def test_center
  end
  def test_diagonal
  end
end
