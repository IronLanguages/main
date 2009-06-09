require 'test/unit'
require 'fox16'

include Fox

class TC_FXMat4f < Test::Unit::TestCase
  def setup
    @hmat = FXMat4f.new
  end
  
  def test_initialize
    h = FXMat4f.new
    assert_instance_of(FXMat4f, h)
  end
  
  def test_from_w
    w = 0.0
    h = FXMat4f.new(w)
    assert_instance_of(FXMat4f, h)
  end
  
  def test_from_elements
    a00, a01, a02, a03 = 0.0, 0.0, 0.0, 0.0
    a10, a11, a12, a13 = 0.0, 0.0, 0.0, 0.0
    a20, a21, a22, a23 = 0.0, 0.0, 0.0, 0.0
    a30, a31, a32, a33 = 0.0, 0.0, 0.0, 0.0
    h = FXMat4f.new(a00, a01, a02, a03,
                   a10, a11, a12, a13,
		   a20, a21, a22, a23,
		   a30, a31, a32, a33)
    assert_instance_of(FXMat4f, h)
  end
  
  def test_from_row_vectors
    a = FXVec4f.new(0.0, 0.0, 0.0, 0.0)
    b = FXVec4f.new(0.0, 0.0, 0.0, 0.0)
    c = FXVec4f.new(0.0, 0.0, 0.0, 0.0)
    d = FXVec4f.new(0.0, 0.0, 0.0, 0.0)
    h = FXMat4f.new(a, b, c, d)
    assert_instance_of(FXMat4f, h)
  end
  
  def test_copy
    anotherHMat = FXMat4f.new(@hmat)
    assert_instance_of(FXMat4f, anotherHMat)
    assert_not_same(@hmat, anotherHMat)
#   assert_equal(@hmat, anotherHMat)
  end
  
  def test_add
    anotherMat = FXMat4f.new
    sum1 = anotherMat + @hmat
    sum2 = @hmat + anotherMat
    assert_instance_of(FXMat4f, sum1)
    assert_instance_of(FXMat4f, sum2)
#   assert_equal(sum1, sum2)
  end
  
  def test_neg
    neg = -@hmat
    assert_instance_of(FXMat4f, neg)
  end
  
  def test_sub
    anotherMat = FXMat4f.new
    diff1 = anotherMat - @hmat
    diff2 = @hmat - anotherMat
    assert_instance_of(FXMat4f, diff1)
    assert_instance_of(FXMat4f, diff2)
#   assert_equal(diff1, -diff2)
  end
  
  def test_mul_matrices
    a = FXMat4f.new
    b = FXMat4f.new
    product = a*b
    assert_instance_of(FXMat4f, product)
#   assert_equal(product, b*a)
  end
  
  def test_mul_by_scalar
    p = FXMat4f.new.eye
    q = FXMat4f.new(4.0, 0.0, 0.0, 0.0,
                   0.0, 4.0, 0.0, 0.0,
		   0.0, 0.0, 4.0, 0.0,
		   0.0, 0.0, 0.0, 4.0)
    r = p*4.0
    assert_instance_of(FXMat4f, r)
#   assert_equal(q, r)
  end
  
  def test_div
    quotient = @hmat/2.0
    assert_instance_of(FXMat4f, quotient)
  end
  
  def test_det
    det = @hmat.det
    assert_instance_of(Float, det)
  end
  
  def test_transpose
    transposed = @hmat.transpose
    assert_instance_of(FXMat4f, transposed)
  end
  
  def test_invert
    identity = FXMat4f.new.eye
    inverted = identity.invert
    assert_instance_of(FXMat4f, inverted)
  end
  
  def test_eye
    eye = @hmat.eye
    assert_same(@hmat, eye)
  end
  
  def test_ortho
    left, right, bottom, top, hither, yon = 0.0, 1.0, 0.0, 1.0, 0.0, 1.0
    ortho = @hmat.ortho(left, right, bottom, top, hither, yon)
    assert_same(@hmat, ortho)
  end
  
  def test_frustum
    left, right, bottom, top, hither, yon = 0.0, 1.0, 0.0, 1.0, 0.1, 1.0
    frustum = @hmat.frustum(left, right, bottom, top, hither, yon)
    assert_same(@hmat, frustum)
  end
  
  def test_left
    left = @hmat.left
    assert_same(@hmat, left)
  end
  
  def test_rot_q
    q = FXQuatf.new
    rot = @hmat.rot(q)
    assert_same(@hmat, rot)
  end
  
  def test_rot_c_s_axis
    axis = FXVec3f.new
    c, s = 0.0, 0.0
    rot = @hmat.rot(axis, c, s)
    assert_same(@hmat, rot)
  end
  
  def test_rot_phi_axis
    axis = FXVec3f.new
    phi = 45.0
    rot = @hmat.rot(axis, phi)
    assert_same(@hmat, rot)
  end
  
  def test_xrot_c_s
    c, s = 0.0, 0.0
    xrot = @hmat.xrot(c, s)
    assert_same(@hmat, xrot)
  end
  
  def test_xrot_phi
    phi = 22.5
    xrot = @hmat.xrot(phi)
    assert_same(@hmat, xrot)
  end
  
  def test_yrot_c_s
    c, s = 0.0, 0.0
    yrot = @hmat.yrot(c, s)
    assert_same(@hmat, yrot)
  end
  
  def test_yrot_phi
    phi = 22.5
    yrot = @hmat.yrot(phi)
    assert_same(@hmat, yrot)
  end
  
  def test_zrot_c_s
    c, s = 0.0, 0.0
    zrot = @hmat.zrot(c, s)
    assert_same(@hmat, zrot)
  end
  
  def test_zrot_phi
    phi = 22.5
    zrot = @hmat.zrot(phi)
    assert_same(@hmat, zrot)
  end
  
  def test_look
    eye = FXVec3f.new
    cntr = FXVec3f.new
    vup = FXVec3f.new
    look = @hmat.look(eye, cntr, vup)
    assert_same(@hmat, look)
  end
  
  def test_trans_txyz
    tx, ty, tz = 0.0, 0.0, 0.0
    translated = @hmat.trans(tx, ty, tz)
    assert_same(@hmat, translated)
  end
  
  def test_trans_vec
    v = FXVec3f.new(0.0, 0.0, 0.0)
    translated = @hmat.trans(v)
    assert_same(@hmat, translated)
  end
  
  def test_scale_sxyz
    sx, sy, sz = 1.0, 1.0, 1.0
    scaled = @hmat.scale(sx, sy, sz)
    assert_same(@hmat, scaled)
  end
  
  def test_scale_s
    s = 1.0
    scaled = @hmat.scale(s)
    assert_same(@hmat, scaled)
  end
  
  def test_scale_vec
    v = FXVec3f.new(1.0, 1.0, 1.0)
    scaled = @hmat.scale(v)
    assert_same(@hmat, scaled)
  end
end
