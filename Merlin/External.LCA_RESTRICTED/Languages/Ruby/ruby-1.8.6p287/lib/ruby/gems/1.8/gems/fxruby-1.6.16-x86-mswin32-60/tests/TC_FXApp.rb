require 'test/unit'
require 'fox16'

include Fox

class TC_FXApp < Test::Unit::TestCase
  def test_exception_for_second_app
    app = FXApp.new
    mainWindow = FXMainWindow.new(app, "")
    app.create
    assert_raise RuntimeError do
      app2 = FXApp.new
    end
  end
end

