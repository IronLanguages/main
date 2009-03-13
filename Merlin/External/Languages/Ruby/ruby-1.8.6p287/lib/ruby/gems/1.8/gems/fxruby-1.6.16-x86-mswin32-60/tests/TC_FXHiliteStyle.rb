require 'fox16'
require 'test/unit'

include Fox

class TC_FXHiliteStyle < Test::Unit::TestCase

  def setup
    @style = FXHiliteStyle.new
  end

  def test_new_object_is_initialized
    assert_equal(0, @style.normalForeColor)
    assert_equal(0, @style.normalBackColor)
    assert_equal(0, @style.selectForeColor)
    assert_equal(0, @style.selectBackColor)
    assert_equal(0, @style.hiliteForeColor)
    assert_equal(0, @style.hiliteBackColor)
    assert_equal(0, @style.activeBackColor)
    assert_equal(0, @style.style)
  end
end

