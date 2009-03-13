require 'test/unit'

require 'fox16'

include Fox

class TC_FXLight < Test::Unit::TestCase
  def setup
    @light = FXLight.new
  end
  def testAttributes
    assert(@light.ambient)
    assert_kind_of(FXVec4f, @light.ambient)
    assert(@light.diffuse)
    assert_kind_of(FXVec4f, @light.diffuse)
    assert(@light.specular)
    assert_kind_of(FXVec4f, @light.specular)
    assert(@light.position)
    assert_kind_of(FXVec4f, @light.position)
    assert(@light.direction)
    assert_kind_of(FXVec3f, @light.direction)
    assert(@light.exponent)
    assert_kind_of(Float, @light.exponent)
    assert(@light.cutoff)
    assert_kind_of(Float, @light.cutoff)
    assert(@light.c_attn)
    assert_kind_of(Float, @light.c_attn)
    assert(@light.l_attn)
    assert_kind_of(Float, @light.l_attn)
    assert(@light.q_attn)
    assert_kind_of(Float, @light.q_attn)
  end
end
