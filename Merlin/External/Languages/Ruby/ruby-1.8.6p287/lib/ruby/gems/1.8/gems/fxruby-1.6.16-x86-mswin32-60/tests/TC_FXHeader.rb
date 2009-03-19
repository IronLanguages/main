require 'fox16'
require 'test/unit'
require 'testcase'

include Fox

class TC_FXHeader < TestCase

  def setup
    super(self.class.name)
    @header = FXHeader.new(mainWindow, nil, 0, HEADER_NORMAL)
  end

  def test_setArrowDir
    @header.appendItem("")
    @header.setArrowDir(0, true)
    @header.setArrowDir(0, false)
    @header.setArrowDir(0, Fox::TRUE)
    @header.setArrowDir(0, Fox::FALSE)
    @header.setArrowDir(0, Fox::MAYBE)
  end

  def test_getArrowDir
    @header.appendItem("")
    assert_instance_of(Fixnum, @header.getArrowDir(0))
  end

  def test_arrowUp?
    @header.appendItem("")
    assert_same(false, @header.arrowUp?(0))
    @header.setArrowDir(0, Fox::TRUE)
    assert_same(true, @header.arrowUp?(0))
  end

  def test_arrowDown?
    @header.appendItem("")
    assert_same(false, @header.arrowDown?(0))
    @header.setArrowDir(0, Fox::FALSE)
    assert_same(true, @header.arrowDown?(0))
  end

  def test_arrowMaybe?
    @header.appendItem("")
    assert_same(true, @header.arrowMaybe?(0))
    @header.setArrowDir(0, Fox::MAYBE)
    assert_same(true, @header.arrowMaybe?(0))
  end
  
  def test_SEL_REPLACED
    @header.appendItem("One")
    @header.appendItem("Two")
    itemIndex = 0
    @header.connect(SEL_REPLACED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @header.setItem(1, "", nil, 0, nil, true)
    assert_equal(1, itemIndex)
  end
  
  def test_SEL_INSERTED
    @header.appendItem("One")
    @header.appendItem("Two")
    itemIndex = 0
    @header.connect(SEL_INSERTED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @header.insertItem(1, "One Point Five", nil, 0, nil, true)
    assert_equal(1, itemIndex)
  end
  
  def test_SEL_DELETED
    @header.appendItem("One")
    @header.appendItem("Two")
    itemIndex = 0
    @header.connect(SEL_DELETED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @header.removeItem(1, true)
    assert_equal(1, itemIndex)
  end
end

