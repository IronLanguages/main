require 'test/unit'

require 'fox16'

include Fox

class TC_FXRectangle < Test::Unit::TestCase
  def setup
    @rect1 = FXRectangle.new
    @rect2 = FXRectangle.new(5, 5, 300, 200)
    @rect3 = FXRectangle.new(FXPoint.new(5, 5), FXSize.new(300, 200))
    @rect4 = FXRectangle.new(FXPoint.new(5, 5), FXPoint.new(304, 204))
  end

  def test_equals
    r1 = FXRectangle.new
    r1.x = 10
    r1.y = 15
    r1.w = 20
    r1.h = 25
    @rect1.x = r1.x
    @rect1.y = r1.y
    @rect1.w = r1.w
    @rect1.h = r1.h
    assert_equal(@rect1, r1)
    assert_equal(@rect2, FXRectangle.new(5, 5, 300, 200))
    assert_equal(@rect3, FXRectangle.new(FXPoint.new(5, 5), FXSize.new(300, 200)))
    assert_equal(@rect4, FXRectangle.new(FXPoint.new(5, 5), FXPoint.new(304, 204)))
  end

  def test_contains?
    assert(@rect2.contains?(100, 100))
    assert(@rect2.contains?(FXPoint.new(100, 100)))
    assert(@rect2.contains?(FXRectangle.new(10, 10, 10, 10)))
  end

  def test_overlaps?
    assert(@rect2.overlaps?(@rect3))
    assert(@rect2.overlaps?(FXRectangle.new(200, 2, 400, 50)))
    assert(!@rect2.overlaps?(FXRectangle.new(2, 2, 2, 2)))
  end

  def test_move!
    x = @rect2.x
    y = @rect2.y
    result = @rect2.move!(10, 10)
    assert_same(@rect2, result)
    assert_equal(@rect2.x, x + 10)
    assert_equal(@rect2.y, y + 10)
  end

  def test_grow!
    result = @rect2.grow!(3)
    assert_same(@rect2, result)
    result = @rect3.grow!(3, 3)
    assert_same(@rect3, result)
    result = @rect4.grow!(3, 3, 3, 3)
    assert_same(@rect4, result)
    assert_equal(@rect2, @rect3)
    assert_equal(@rect2, @rect4)
    assert_equal(@rect3, @rect4)
  end

  def test_shrink!
    result = @rect2.shrink!(3)
    assert_same(@rect2, result)
    result = @rect3.shrink!(3, 3)
    assert_same(@rect3, result)
    result = @rect4.shrink!(3, 3, 3, 3)
    assert_same(@rect4, result)
    assert_equal(@rect2, @rect3)
    assert_equal(@rect2, @rect4)
    assert_equal(@rect3, @rect4)
  end

  def test_corners
    assert_equal(FXPoint.new(  5,   5), @rect2.tl)
    assert_equal(FXPoint.new(304,   5), @rect2.tr)
    assert_equal(FXPoint.new(  5, 204), @rect2.bl)
    assert_equal(FXPoint.new(304, 204), @rect2.br)
  end

  def test_union
  end

  def test_intersection
  end
end
