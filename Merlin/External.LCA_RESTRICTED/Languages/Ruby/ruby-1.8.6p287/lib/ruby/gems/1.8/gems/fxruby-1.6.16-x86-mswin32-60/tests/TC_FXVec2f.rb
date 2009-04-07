require 'test/unit'
require 'fox16'

include Fox

class TC_FXVec2f < Test::Unit::TestCase

  def test_default_constructor
    FXVec4f.new
  end

  def test_copy_constructor
    vec = FXVec2f.new(2.0, 3.0)
    assert_equal(vec, FXVec2f.new(vec)) # also tests the '==' method!
  end

  def test_construct_from_components
    vec = FXVec2f.new(2.0, 3.0)
    assert_equal(2.0, vec[0])
    assert_equal(2.0, vec.x)
    assert_equal(3.0, vec[1])
    assert_equal(3.0, vec.y)
  end
  
  def test_getitem
    vec = FXVec2f.new(2.0, 3.0)
    assert_equal(2.0, vec[0])
    assert_equal(3.0, vec[1])
  end

  def test_setitem
    vec = FXVec2f.new(2.0, 3.0)
    vec[0] = 1.0
    vec[1] = 4.0
    assert_equal(1.0, vec[0])
    assert_equal(4.0, vec[1])
  end

  def test_bounds_checks
    vec = FXVec2f.new
    assert_raises(IndexError) { vec[-1] }
    assert_raises(IndexError) { vec[2] }
    assert_raises(IndexError) { vec[-1] = 0.0 }
    assert_raises(IndexError) { vec[2] = 0.0 }
  end

  def test_unary_minus
    vec = -FXVec2f.new(1.0, 2.0)
    assert_equal(vec[0], -1.0)
    assert_equal(vec[1], -2.0)
  end

  def test_add
    a = FXVec2f.new(1.0, 2.0)
    b = FXVec2f.new(2.0, 4.0)
    c = FXVec2f.new(3.0, 6.0)
    assert_equal(c, a + b)
  end

  def test_subtract
    a = FXVec2f.new(3.0, 6.0)
    b = FXVec2f.new(2.0, 4.0)
    c = FXVec2f.new(1.0, 2.0)
    assert_equal(c, a - b)
  end

  def test_multiply_by_scalar
    v1 = FXVec2f.new(3.0,  6.0)
    v2 = FXVec2f.new(6.0, 12.0)
    assert_equal(v2, v1*2)
  end

  def test_dot_product
    v1 = FXVec2f.new(3.0, 6.0)
    v2 = FXVec2f.new(2.0, 4.0)
    assert_equal(30.0, v1*v2)
    assert_equal(30.0, v2*v1)
    assert_equal(30.0, v1.dot(v2))
    assert_equal(30.0, v2.dot(v1))
  end

  def test_divide_by_scalar
    v1 = FXVec2f.new(6.0, 12.0)
    v2 = FXVec2f.new(3.0,  6.0)
    assert_equal(v2, v1/2)
      assert_raises(ZeroDivisionError) {
      v1/0
    }
  end

  def test_length
    v = FXVec2f.new(1.0, 1.0)
    assert_in_delta(Math.sqrt(2), v.length, 1.0e-7) 
  end

  def test_length2
    v = FXVec2f.new(1.0, 1.0)
    assert_equal(2.0, v.length2) 
  end

  def test_normalize
    vec = FXVec2f.new(1.0, 1.0).normalize
    assert_in_delta(1.0/Math.sqrt(2), vec.x, 1.0e-7)
    assert_in_delta(1.0/Math.sqrt(2), vec.y, 1.0e-7)
  end

  def test_lo
    v1 = FXVec2f.new(3.0, 2.0)
    v2 = FXVec2f.new(2.0, 3.0)
    assert_equal(v1.lo(v2), v2.lo(v1))
    lo = v1.lo(v2)
    assert_equal(2.0, lo.x)
    assert_equal(2.0, lo.y)
  end

  def test_hi
    v1 = FXVec2f.new(3.0, 2.0)
    v2 = FXVec2f.new(2.0, 3.0)
    assert_equal(v1.hi(v2), v2.hi(v1))
    hi = v1.hi(v2)
    assert_equal(3.0, hi.x)
    assert_equal(3.0, hi.y)
  end

  def test_to_a
    ary = FXVec2f.new(1.0, 1.0).to_a
    assert_equal(Array, ary.class)
    assert_equal(2, ary.length)
    assert_equal(1.0, ary[0])
    assert_equal(1.0, ary[1])
  end

  def test_equal
    assert(FXVec2f.new(1.0, 2.0) == FXVec2f.new(1.0, 2.0))
  end

end
