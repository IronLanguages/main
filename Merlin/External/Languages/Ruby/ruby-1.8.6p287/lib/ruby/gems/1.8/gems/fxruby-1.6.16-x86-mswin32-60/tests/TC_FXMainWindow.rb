require 'test/unit'
require 'fox16'

include Fox

class TC_FXMainWindow < Test::Unit::TestCase
  def test_nil_app_raises_argument_error
    assert_raise ArgumentError do
      FXMainWindow.new(nil, "title")
    end
  end
  
  def test_non_created_app_raises_runtime_error
    app = FXApp.new
    assert_raise RuntimeError do
      FXMainWindow.new(app, "title").create
    end
  end
end