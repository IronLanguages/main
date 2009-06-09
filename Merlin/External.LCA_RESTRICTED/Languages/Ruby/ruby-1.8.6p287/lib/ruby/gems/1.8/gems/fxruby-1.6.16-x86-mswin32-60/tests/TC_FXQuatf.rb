require 'test/unit'
require 'fox16'

include Fox

class TC_FXQuatf < Test::Unit::TestCase
  def setup
    @quat = FXQuatf.new
  end
  def test_default_constructor
    q = FXQuatf.new
  end
  def test_construct_from_axis_and_angle
    axis = FXVec3f.new(1.0, 1.0, 1.0)
    q = FXQuatf.new(axis)
    q = FXQuatf.new(axis, 0.0)
  end
  def test_construct_from_components
    x, y, z, w = 1.0, 1.0, 1.0, 1.0
    q = FXQuatf.new(x, y, z, w)
  end
  def test_construct_from_roll_pitch_yaw
    roll, pitch, yaw = 45.0, 45.0, 45.0
    q = FXQuatf.new(roll, pitch, yaw)
  end
  def test_adjust!
    adjusted = @quat.adjust!
    assert_same(@quat, adjusted)
  end
  def test_setRollPitchYaw
    roll, pitch, yaw = 0.0, 0.0, 0.0
    @quat.setRollPitchYaw(roll, pitch, yaw)
  end
  def test_getRollPitchYaw
    rpy = @quat.getRollPitchYaw()
    assert_instance_of(Array, rpy)
    assert_equal(3, rpy.length)
  end
  def test_exp
    expQuat = @quat.exp
    assert_instance_of(FXQuatf, expQuat)
  end
  def test_log
    logQuat = @quat.log
    assert_instance_of(FXQuatf, logQuat)
  end
  def test_invert
    invertQuat = @quat.invert
    assert_instance_of(FXQuatf, invertQuat)
  end
  def test_conj
    conjQuat = @quat.conj
    assert_instance_of(FXQuatf, conjQuat)
  end
  def test_mul
    anotherQuat = FXQuatf.new
    product = @quat*anotherQuat
    assert_instance_of(FXQuatf, product)
    assert_equal(product, anotherQuat*@quat)
  end
  def test_arc
    a = FXVec3f.new(0.0, 0.0, 0.0)
    b = FXVec3f.new(0.0, 0.0, 0.0)
    q = FXQuatf.arc(a, b)
    assert_instance_of(FXQuatf, q)
  end
  def test_lerp
    u = FXQuatf.new
    v = FXQuatf.new
    q = FXQuatf.lerp(u, v, 5.0)
    assert_instance_of(FXQuatf, q)
  end
end
