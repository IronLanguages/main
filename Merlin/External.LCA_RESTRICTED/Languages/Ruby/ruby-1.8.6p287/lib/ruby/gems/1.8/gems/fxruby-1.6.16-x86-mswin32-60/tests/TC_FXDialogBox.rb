require 'test/unit'
require 'fox16'

include Fox

class TC_FXDialogBox < Test::Unit::TestCase
  def test_nil_app_raises_argument_error
    assert_raise ArgumentError do
      FXDialogBox.new(nil, "title")
    end
  end
end