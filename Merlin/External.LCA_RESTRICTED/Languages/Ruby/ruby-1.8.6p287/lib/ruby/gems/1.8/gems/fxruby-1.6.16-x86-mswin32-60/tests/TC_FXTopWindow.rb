require 'test/unit'

require 'fox16'

include Fox

class TC_FXTopWindow < Test::Unit::TestCase
  def setup
    if FXApp.instance.nil?
      @app = FXApp.new('TC_FXTopWindow', 'FXRuby')
      @app.init([])
    else
      @app = FXApp.instance
    end
    @mainWin = FXMainWindow.new(@app, 'TC_FXButton')
  end
  
  def test_new
    # Free-floating
    top1 = FXTopWindow.new(@app, "top1", nil, nil, DECOR_ALL, 0, 0, 0, 0,
      DEFAULT_PAD, DEFAULT_PAD, DEFAULT_PAD, DEFAULT_PAD,
      DEFAULT_SPACING, DEFAULT_SPACING)
    assert_equal("top1", top1.title)
      
    # Owned
    top2 = FXTopWindow.new(@mainWin, "top2", nil, nil, DECOR_ALL, 0, 0, 0, 0,
      DEFAULT_PAD, DEFAULT_PAD, DEFAULT_PAD, DEFAULT_PAD,
      DEFAULT_SPACING, DEFAULT_SPACING)
    assert_same(@mainWin, top2.owner)
    assert_equal("top2", top2.title)
  end
end
