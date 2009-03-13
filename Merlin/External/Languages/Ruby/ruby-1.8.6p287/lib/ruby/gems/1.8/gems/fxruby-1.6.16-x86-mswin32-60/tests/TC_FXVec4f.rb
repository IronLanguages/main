require 'test/unit'

require 'fox16'

include Fox

class TC_FXVec4f < Test::Unit::TestCase

  def test_new
    FXVec4f.new
  end
  
  def test_new2
    vec1 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    assert_equal(vec1, FXVec4f.new(vec1))
  end
  
  def test_new3
    a = FXVec3f.new(1, 2, 3)
    b = FXVec4f.new(a)
    assert_equal(1, b[0])
    assert_equal(2, b[1])
    assert_equal(3, b[2])
    assert_equal(1, b[3])
  end
  
  def test_new4
    a = FXVec4f.new(1, 2, 3)
    assert_equal(1, a[0])
    assert_equal(2, a[1])
    assert_equal(3, a[2])
    assert_equal(1, a[3])

    b = FXVec4f.new(1, 2, 3, 4)
    assert_equal(1, b[0])
    assert_equal(2, b[1])
    assert_equal(3, b[2])
    assert_equal(4, b[3])
  end
  
  def test_new5
    c = FXVec4f.new(FXRGB(128, 128, 128))
  end

  def test_getitem
    v = FXVec4f.new
    assert_kind_of(Float, v[0])
    assert_kind_of(Float, v[1])
    assert_kind_of(Float, v[2])
    assert_kind_of(Float, v[3])
    assert_raises(IndexError) { v[-1] }
    assert_raises(IndexError) { v[4] }
  end

  def test_setitem
    v = FXVec4f.new
    assert_kind_of(Float, v[0] = 0.0)
    assert_kind_of(Float, v[1] = 0.0)
    assert_kind_of(Float, v[2] = 0.0)
    assert_kind_of(Float, v[3] = 0.0)
    assert_raises(IndexError) { v[-1] = 0.0 }
    assert_raises(IndexError) { v[4] = 0.0 }
  end

  def test_neg
    vec = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    vec = -vec
    assert_equal(vec[0], -1.0)
    assert_equal(vec[1], -2.0)
    assert_equal(vec[2], -3.0)
    assert_equal(vec[3], -4.0)
  end

  def test_add
    v1 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    v2 = FXVec4f.new(2.0, 4.0, 6.0, 8.0)
    v3 = FXVec4f.new(3.0, 6.0, 9.0, 12.0)
    assert_equal(v3, v1 + v2)
  end

  def test_sub
    v1 = FXVec4f.new(3.0, 6.0, 9.0, 12.0)
    v2 = FXVec4f.new(2.0, 4.0, 6.0, 8.0)
    v3 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    assert_equal(v3, v1 - v2)
  end

  def test_mul
    v1 = FXVec4f.new(3.0,  6.0,  9.0, 12.0)
    v2 = FXVec4f.new(6.0, 12.0, 18.0, 24.0)
    assert_equal(v2, v1 * 2)
  end
  
  def test_mul2 # same as dot product
    v1 = FXVec4f.new(3.0, 6.0, 9.0, 12.0)
    v2 = FXVec4f.new(2.0, 4.0, 6.0, 8.0)
    assert_equal(180.0, v1*v2)
    assert_equal(180.0, v2*v1)
  end

  def test_div
    v1 = FXVec4f.new(6.0, 12.0, 18.0, 24.0)
    v2 = FXVec4f.new(3.0,  6.0,  9.0, 12.0)
    assert_equal(v2, v1/2)
      assert_raises(ZeroDivisionError) {
      v1/0
    }
  end

  def test_dot
    v1 = FXVec4f.new(3.0, 6.0, 9.0, 12.0)
    v2 = FXVec4f.new(2.0, 4.0, 6.0, 8.0)
    assert_equal(180.0, v1.dot(v2))
    assert_equal(180.0, v2.dot(v1))
  end

  def test_length
    v = FXVec4f.new(1.0, 1.0, 1.0, 1.0)
    assert_equal(2.0, v.length) 
  end

  def test_normalize
    vec = FXVec4f.new(1.0, 1.0, 1.0, 1.0)
    assert_equal(FXVec4f.new(0.5, 0.5, 0.5, 0.5), vec.normalize)
  end

  def test_lo
    v1 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    v2 = FXVec4f.new(2.0, 3.0, 4.0, 5.0)
    assert_equal(v1, v1.lo(v2))
    assert_equal(v1, v2.lo(v1))
  end

  def test_hi
    v1 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    v2 = FXVec4f.new(2.0, 3.0, 4.0, 5.0)
    assert_equal(v2, v1.hi(v2))
    assert_equal(v2, v2.hi(v1))
  end

  def test_to_a
    vec = FXVec4f.new(1.0, 1.0, 1.0)
    arr = vec.to_a
    assert_equal(Array, arr.class)
    assert_equal(4, arr.length)
    assert_equal(vec[0], arr[0])
    assert_equal(vec[1], arr[1])
    assert_equal(vec[2], arr[2])
    assert_equal(vec[3], arr[3])
  end

  def test_equal
    vec1 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    vec2 = FXVec4f.new(1.0, 2.0, 3.0, 4.0)
    assert(vec1 == vec2)
  end

end
