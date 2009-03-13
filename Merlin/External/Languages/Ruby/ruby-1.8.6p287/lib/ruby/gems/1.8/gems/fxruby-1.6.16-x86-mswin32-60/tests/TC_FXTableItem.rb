require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class OverrideError < Exception
end

class CustomTableItem < FXTableItem
  def getText
    raise OverrideError
  end
  def getIcon
    raise OverrideError
  end
end

class TC_FXTableItem < TestCase
  def setup
    super(self.class.name)
    @table = FXTable.new(mainWindow)
    @table.setTableSize(1, 1)
    @tableItem = FXTableItem.new("item")
  end

  def test_setStipple
    @tableItem.stipple = STIPPLE_CROSSDIAG
  end

  def test_getStipple
    assert_kind_of(Integer, @tableItem.stipple)
  end

  def test_overridden_getText
    @table.setItem(0, 0, CustomTableItem.new(""))
    assert_raises(OverrideError) {
      @table.getItemText(0, 0)
    }
  end

  def test_overridden_getIcon
    @table.setItem(0, 0, CustomTableItem.new(""))
    assert_raises(OverrideError) {
      @table.getItemIcon(0, 0)
    }
  end
end
