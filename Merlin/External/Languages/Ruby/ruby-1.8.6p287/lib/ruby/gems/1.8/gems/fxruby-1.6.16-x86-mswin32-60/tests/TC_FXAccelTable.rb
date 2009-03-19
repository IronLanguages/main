require 'test/unit'

require 'fox16'

include Fox

class TC_FXAccelTable < Test::Unit::TestCase
  def setup
    @accel = FXAccelTable.new
  end

  def test_addAccel
    hotkey = fxparseHotKey('&A')
    target = FXObject.new
    seldn, selup = 0, 0
    @accel.addAccel(hotkey)
    @accel.addAccel(hotkey, target)
    @accel.addAccel(hotkey, target, seldn)
    @accel.addAccel(hotkey, target, seldn, selup)
  end
  
  def test_hasAccel
    hotkey = fxparseHotKey('&b')
    assert(!@accel.hasAccel?(hotkey))
    @accel.addAccel(hotkey)
    assert(@accel.hasAccel?(hotkey))
  end
  
  def test_targetOfAccel
    hotkey = fxparseHotKey("&x")
    target = FXObject.new
    @accel.addAccel(hotkey, target)
    assert_same(target, @accel.targetOfAccel(hotkey))
  end
  
  def test_removeAccel
    hotkey = fxparseHotKey('&b')
    @accel.addAccel(hotkey)
    assert(@accel.hasAccel?(hotkey))
    @accel.removeAccel(hotkey)
    assert(!@accel.hasAccel?(hotkey))
  end
end
