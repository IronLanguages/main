require 'test/unit'

require 'fox16'
require 'fox16/glshapes'

include Fox

class TC_FXMaterial < Test::Unit::TestCase

  DELTA = 1.0e-6
  
  def setup
    @mat = FXMaterial.new
  end
  
  def test_ambient
    @mat.ambient = [0.5, 0.6, 0.7, 0.8]
    assert_in_delta(0.5, @mat.ambient[0], DELTA)
    assert_in_delta(0.6, @mat.ambient[1], DELTA)
    assert_in_delta(0.7, @mat.ambient[2], DELTA)
    assert_in_delta(0.8, @mat.ambient[3], DELTA)

    @mat.ambient = FXVec4f.new(0.5, 0.6, 0.7, 0.8)
    assert_in_delta(0.5, @mat.ambient[0], DELTA)
    assert_in_delta(0.6, @mat.ambient[1], DELTA)
    assert_in_delta(0.7, @mat.ambient[2], DELTA)
    assert_in_delta(0.8, @mat.ambient[3], DELTA)
  end
  
  def test_diffuse
    @mat.diffuse = [0.5, 0.6, 0.7, 0.8]
    assert_in_delta(0.5, @mat.diffuse[0], DELTA)
    assert_in_delta(0.6, @mat.diffuse[1], DELTA)
    assert_in_delta(0.7, @mat.diffuse[2], DELTA)
    assert_in_delta(0.8, @mat.diffuse[3], DELTA)

    @mat.diffuse = FXVec4f.new(0.5, 0.6, 0.7, 0.8)
    assert_in_delta(0.5, @mat.diffuse[0], DELTA)
    assert_in_delta(0.6, @mat.diffuse[1], DELTA)
    assert_in_delta(0.7, @mat.diffuse[2], DELTA)
    assert_in_delta(0.8, @mat.diffuse[3], DELTA)
  end
  
  def test_emission
    @mat.emission = [0.5, 0.6, 0.7, 0.8]
    assert_in_delta(0.5, @mat.emission[0], DELTA)
    assert_in_delta(0.6, @mat.emission[1], DELTA)
    assert_in_delta(0.7, @mat.emission[2], DELTA)
    assert_in_delta(0.8, @mat.emission[3], DELTA)

    @mat.emission = FXHVec.new(0.5, 0.6, 0.7, 0.8)
    assert_in_delta(0.5, @mat.emission[0], DELTA)
    assert_in_delta(0.6, @mat.emission[1], DELTA)
    assert_in_delta(0.7, @mat.emission[2], DELTA)
    assert_in_delta(0.8, @mat.emission[3], DELTA)
  end
  
  def test_emission
    @mat.emission = [0.5, 0.6, 0.7, 0.8]
    assert_in_delta(0.5, @mat.emission[0], DELTA)
    assert_in_delta(0.6, @mat.emission[1], DELTA)
    assert_in_delta(0.7, @mat.emission[2], DELTA)
    assert_in_delta(0.8, @mat.emission[3], DELTA)

    @mat.emission = FXVec4f.new(0.5, 0.6, 0.7, 0.8)
    assert_in_delta(0.5, @mat.emission[0], DELTA)
    assert_in_delta(0.6, @mat.emission[1], DELTA)
    assert_in_delta(0.7, @mat.emission[2], DELTA)
    assert_in_delta(0.8, @mat.emission[3], DELTA)
  end
  
  def test_shininess
    @mat.shininess = 0.5
    assert_in_delta(0.5, @mat.shininess, DELTA)
  end

  def test_bug
    cube = FXGLCube.new(0, 0, 0, 0, 0)
    mat = FXMaterial.new
    mat.diffuse  = FXVec4f.new(0, 0, 0, 0)
    mat.specular = FXVec4f.new(0, 0, 0, 0)
    mat.ambient  = FXVec4f.new(0, 0, 0, 0)
    cube.setMaterial(0, mat)
    mat2 = cube.getMaterial(0)
    assert_instance_of(FXVec4f, mat2.ambient)
    assert_instance_of(FXVec4f, mat2.specular)
    assert_instance_of(FXVec4f, mat2.diffuse)
  end
end
