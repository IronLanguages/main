require 'test/unit'

require 'fox16'

include Fox

class TC_FXSize < Test::Unit::TestCase
  def setup
    @size1 = FXSize.new
    @size2 = FXSize.new(300, 200)
  end

  def test_copy
    assert_equal(@size1, FXSize.new(@size1))
    assert_equal(@size2, FXSize.new(@size2))
  end

  def test_equals
    @size1.w = 250
    @size1.h = 475
    sameSize1 = FXSize.new
    sameSize1.w = 250
    sameSize1.h = 475
    assert_equal(@size1, sameSize1)
    assert_equal(sameSize1, @size1)

    sameSize2 = FXSize.new(300, 200)
    assert_equal(@size2, sameSize2)
    assert_equal(sameSize2, @size2)
  end

  def test_uminus
    size1 = -(@size1)
    assert(size1.w == -(@size1.w) && size1.h == -(@size1.h))
    size2 = -(@size2)
    assert(size2.w == -(@size2.w) && size2.h == -(@size2.h))
  end

  def test_add
    assert_equal(FXSize.new(1, 2) + FXSize.new(3, 4), FXSize.new(4, 6))
  end

  def test_sub
    assert_equal(FXSize.new(4, 6) - FXSize.new(3, 4), FXSize.new(1, 2))
  end

  def test_mul
    assert_equal(FXSize.new(1, 2)*3, FXSize.new(3, 6))
  end

  def test_div
    assert_equal(FXSize.new(3, 6)/3, FXSize.new(1, 2))
  end
end
