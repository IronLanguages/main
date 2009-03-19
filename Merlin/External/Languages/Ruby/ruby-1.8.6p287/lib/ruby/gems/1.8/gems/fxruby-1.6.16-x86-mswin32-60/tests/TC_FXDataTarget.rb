require 'test/unit'
require 'fox16'

include Fox

class TC_FXDataTarget < Test::Unit::TestCase
  def setup
    @nilTarget    = FXDataTarget.new
    @intTarget    = FXDataTarget.new(42)
    @floatTarget  = FXDataTarget.new(3.14159)
    @stringTarget = FXDataTarget.new("foo")
    @trueTarget   = FXDataTarget.new(true)
    @falseTarget  = FXDataTarget.new(false)
  end
  def test_to_s
    assert_equal("", @nilTarget.to_s)
    assert_equal("42", @intTarget.to_s)
    assert_equal("3.14159", @floatTarget.to_s)
    assert_equal("foo", @stringTarget.to_s)
    assert_equal("true", @trueTarget.to_s)
    assert_equal("false", @falseTarget.to_s)
  end
end

