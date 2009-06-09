require 'test/unit'
require 'fox16'

include Fox

class TC_FXGLGroup < Test::Unit::TestCase
  def setup
    @group = FXGLGroup.new
  end

  def test_append
    assert_equal(0, @group.size)
    @group.append(FXGLObject.new)
    assert_equal(1, @group.size)
  end

  def test_appendOp
    assert_equal(0, @group.size)
    @group << FXGLObject.new
    assert_equal(1, @group.size)
  end
  
  def test_each_child_yields_to_block
    @group << FXGLObject.new
    @group << FXGLObject.new
    count = 0
    assert_nothing_raised {
      @group.each_child { |c| count += 1 }
    }
    assert_equal(2, count)
  end
end
