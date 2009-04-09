require 'test/unit'
require 'fox16'

include Fox

class TC_FXVec3f < Test::Unit::TestCase

  def test_default_constructor
    FXVec3f.new
  end

  def test_copy_constructor
    vec = FXVec3f.new(1, 2, 3)
    assert_equal(vec, FXVec3f.new(vec)) # also tests the '==' method!
  end

  def test_construct_from_components
    vec = FXVec3f.new(1, 2, 3)
    assert_equal(1, vec[0])
    assert_equal(1, vec.x)
    assert_equal(2, vec[1])
    assert_equal(2, vec.y)
    assert_equal(3, vec[2])
    assert_equal(3, vec.z)
  end
  
  def test_construct_from_color
    vec = FXVec3f.new(FXRGB(1, 1, 1))
    assert_in_delta(0.003921568627,  vec.x, 1.0e-7)
    assert_in_delta(0.003921568627, vec.y, 1.0e-7)
    assert_in_delta(0.003921568627, vec.z, 1.0e-7)
  end

  def test_getitem
    v = FXVec3f.new(1, 2, 3)
    assert_equal(1, v[0])
    assert_equal(2, v[1])
    assert_equal(3, v[2])
  end

  def test_setitem
    v = FXVec3f.new
    assert_equal(1, v[0] = 1)
    assert_equal(2, v[1] = 2)
    assert_equal(3, v[2] = 3)
  end
  
  def test_bounds_checks
    vec = FXVec3f.new
    assert_raises(IndexError) { vec[-1] }
    assert_raises(IndexError) { vec[3]  }
    assert_raises(IndexError) { vec[-1] = 0.0 }
    assert_raises(IndexError) { vec[3]  = 0.0 }
  end
  
  def test_unary_minus
    assert_equal(FXVec3f.new(-1, -2, -3), -FXVec3f.new(1, 2, 3))
  end

  def test_add
    a = FXVec3f.new(1, 2, 3)
    b = FXVec3f.new(2, 4, 6)
    c = FXVec3f.new(3, 6, 9)
    assert_equal(c, a + b)
  end

  def test_subtract
    a = FXVec3f.new(3, 6, 9)
    b = FXVec3f.new(2, 4, 6)
    c = FXVec3f.new(1, 2, 3)
    assert_equal(c, a - b)
  end

  def test_multiply_by_scalar
    v1 = FXVec3f.new(3, 6, 9)
    v2 = FXVec3f.new(6, 12, 18)
    assert_equal(v2, v1*2)
  end

  def test_divide_by_scalar
    v1 = FXVec3f.new(6, 12, 18)
    v2 = FXVec3f.new(3, 6, 9)
    assert_equal(v2, v1/2)
      assert_raises(ZeroDivisionError) {
      v1/0
    }
  end

  def test_dot_product
    v1 = FXVec3f.new(1, 2, 3)
    v2 = FXVec3f.new(1, 2, 3)
    assert_equal(14, v1*v2)
    assert_equal(14, v2*v1)
    assert_equal(14, v1.dot(v2))
    assert_equal(14, v2.dot(v1))
  end

  def test_cross_product
  end

  def test_length
    v = FXVec3f.new(1, 1, 1)
    assert_in_delta(Math.sqrt(3), v.length, 1.0e-7) 
  end

  def test_length2
    v = FXVec3f.new(1, 1, 1)
    assert_equal(3, v.length2) 
  end

  def test_normalize
    vec = FXVec3f.new(1, 1, 1).normalize
    assert_in_delta(1/Math.sqrt(3), vec.x, 1.0e-7)
    assert_in_delta(1/Math.sqrt(3), vec.y, 1.0e-7)
    assert_in_delta(1/Math.sqrt(3), vec.z, 1.0e-7)
  end

  def test_lo
    v1 = FXVec3f.new(3, 2, 1)
    v2 = FXVec3f.new(1, 2, 3)
    assert_equal(v1.lo(v2), v2.lo(v1))
    lo = v1.lo(v2)
    assert_equal(1, lo.x)
    assert_equal(2, lo.y)
    assert_equal(1, lo.z)
  end

  def test_hi
    v1 = FXVec3f.new(1, 2, 3)
    v2 = FXVec3f.new(3, 2, 1)
    assert_equal(v1.hi(v2), v2.hi(v1))
    hi = v1.hi(v2)
    assert_equal(3, hi.x)
    assert_equal(2, hi.y)
    assert_equal(3, hi.z)
  end

  def test_to_a
    ary = FXVec3f.new(1, 1, 1).to_a
    assert_equal(Array, ary.class)
    assert_equal(3, ary.length)
    assert_equal(1, ary[0])
    assert_equal(1, ary[1])
    assert_equal(1, ary[2])
  end

  def test_equal
    assert(FXVec3f.new(1, 2, 3) == FXVec3f.new(1, 2, 3))
  end
end
