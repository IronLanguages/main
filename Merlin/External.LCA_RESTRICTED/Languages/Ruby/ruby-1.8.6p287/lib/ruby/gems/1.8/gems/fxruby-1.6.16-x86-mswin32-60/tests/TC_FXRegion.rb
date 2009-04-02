require 'test/unit'

require 'fox16'

include Fox

class TC_FXRegion < Test::Unit::TestCase

  def setup
    @region = FXRegion.new(5, 5, 10, 10)
  end

  def test_construct_from_points
    points = [
               FXPoint.new(0, 0),
               FXPoint.new(0, 100),
               FXPoint.new(100, 100),
               FXPoint.new(0, 0)
             ]
    r1 = FXRegion.new(points, true)
    r2 = FXRegion.new(points, false)
    r3 = FXRegion.new(points)
  end

  def test_copy_constructor
    assert_equal(@region, FXRegion.new(@region))
  end

  def test_empty
    assert(!@region.empty?)
    empty_region = FXRegion.new(5, 5, 0, 0)
    assert(empty_region.empty?)
  end

  def test_containsPoint
    # Definitely out of bounds
    assert(!@region.contains?(2, 3))

    # Definitely in bounds    
    assert(@region.contains?(6, 6))

    # Check corners too
    assert(@region.contains?(5, 5))
    assert(@region.contains?(5, 14))
    assert(@region.contains?(14, 14))
    assert(@region.contains?(14, 5))
  end

  def test_containsRectangle
    assert(@region.contains?(2, 3, 15, 15)) # why doesn't this fail?
    assert(@region.contains?(5, 5, 10, 10))
    assert(@region.contains?(6, 6, 5, 5))
  end
end
