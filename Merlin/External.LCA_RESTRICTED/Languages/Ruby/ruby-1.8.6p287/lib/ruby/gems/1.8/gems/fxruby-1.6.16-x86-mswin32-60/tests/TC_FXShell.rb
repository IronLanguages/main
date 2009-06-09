require 'test/unit'

require 'fox16'

include Fox

class TC_FXShell < Test::Unit::TestCase
  def setup
    if FXApp.instance.nil?
      @app = FXApp.new('TC_FXShell', 'FXRuby')
      @app.init([])
    else
      @app = FXApp.instance
    end
    @mainWin = FXMainWindow.new(@app, 'TC_FXShell')
  end
  
  def test_new
    # Free-floating
    shell1 = FXShell.new(@app, 0, 0, 0, 0, 0)
      
    # Owned
    shell2 = FXShell.new(@mainWin, 0, 0, 0, 0, 0)
    assert_same(@mainWin, shell2.owner)
  end
end
