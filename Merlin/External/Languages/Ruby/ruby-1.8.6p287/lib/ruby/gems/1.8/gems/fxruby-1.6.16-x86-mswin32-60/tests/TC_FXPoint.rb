require 'test/unit'

require 'fox16'

include Fox

class TC_FXPoint < Test::Unit::TestCase
  def setup
    @point1 = FXPoint.new
    @point2 = FXPoint.new(300, 200)
    @point3 = FXPoint.new(FXSize.new(300, 200))
  end

  def test_copy
    assert_equal(@point1, FXPoint.new(@point1))
    assert_equal(@point2, FXPoint.new(@point2))
  end

  # The assertEqual() method will test the implementation of
  # FXPoint's '==' method, which is the point of this test
  def test_equals
    assert_equal(@point2, @point3)
  
    samePoint1 = FXPoint.new
    samePoint1.x = @point1.x
    samePoint1.y = @point1.y
    assert_equal(@point1, samePoint1)
    assert_equal(samePoint1, @point1)

    samePoint2 = FXPoint.new(300, 200)
    assert_equal(@point2, samePoint2)
    assert_equal(samePoint2, @point2)
  end

  def test_uminus
    point1 = -(@point1)
    assert(point1.x == -(@point1.x) && point1.y == -(@point1.y))
    point2 = -(@point2)
    assert(point2.x == -(@point2.x) && point2.y == -(@point2.y))
  end

  def test_add
    assert(FXPoint.new(1, 2) + FXPoint.new(3, 4) == FXPoint.new(4, 6))
  end

  def test_sub
    assert(FXPoint.new(4, 6) - FXPoint.new(3, 4) == FXPoint.new(1, 2))
  end

  def test_mul
    assert(FXPoint.new(1, 2)*3 == FXPoint.new(3, 6))
  end

  def test_div
    assert(FXPoint.new(3, 6)/3 == FXPoint.new(1, 2))
  end
end
