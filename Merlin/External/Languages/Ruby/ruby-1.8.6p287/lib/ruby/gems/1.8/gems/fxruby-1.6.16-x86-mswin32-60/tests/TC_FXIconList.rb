require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXIconList < TestCase

private

  def checkBounds(meth, *args)
    assert_raises(IndexError) {
      @iconList.send(meth, -1, *args)
    }
    assert_raises(IndexError) {
      @iconList.send(meth, 1, *args)
    }
    assert_nothing_raised(IndexError) {
      @iconList.send(meth, 0, *args)
    }
  end
  
public

  def setup
    super('TC_FXIconList')
    @iconList = FXIconList.new(mainWindow)
  end
  
  def test_appendItem_byItem
    items = []
    0.upto(4) do |i|
      items << FXIconItem.new("item#{i}")
    end
    assert_equal(0, @iconList.numItems)
    @iconList.appendItem(items[0])
    assert_equal(1, @iconList.numItems)
    @iconList.appendItem(items[1], true)
    assert_equal(2, @iconList.numItems)
    @iconList.appendItem(items[2], false)
    assert_equal(3, @iconList.numItems)
    assert_raises(ArgumentError) do
      @iconList.appendItem(items[3], 42) # second argument must be true or false
    end
    assert_equal(3, @iconList.numItems)
  end

  def test_appendOp
    assert_equal(0, @iconList.numItems)
    @iconList << FXIconItem.new("item1")
    assert_equal(1, @iconList.numItems)
    @iconList << FXIconItem.new("item2")
    assert_equal(2, @iconList.numItems)
    @iconList << FXIconItem.new("item3")
    assert_equal(3, @iconList.numItems)
  end
  
  def test_removeHeader
    @iconList.appendHeader("One")
    checkBounds(:removeHeader)
  end
  
  def test_setHeaderText
    @iconList.appendHeader("One")
    checkBounds(:setHeaderText, "Foo")
  end
  
  def test_getHeaderText
    @iconList.appendHeader("Boo")
    checkBounds(:getHeaderText)
  end

  def test_setHeaderIcon
    @iconList.appendHeader("Boo")
    checkBounds(:setHeaderIcon, nil)
  end
  
  def test_getHeaderIcon
    @iconList.appendHeader("Boo")
    checkBounds(:getHeaderIcon)
  end
  
  def test_setHeaderSize
    @iconList.appendHeader("Boo")
    checkBounds(:setHeaderSize, 0)
  end
  
  def test_getHeaderSize
    @iconList.appendHeader("Boo")
    checkBounds(:getHeaderSize)
  end
  
  def test_getItem
    @iconList.appendItem("Foo")
    assert_equal(1, @iconList.numItems)
    checkBounds(:getItem)
  end

  def test_moveItem
    @iconList.appendItem("First")
    @iconList.appendItem("Second")
    assert_raises(IndexError) {
      @iconList.moveItem(0, -1)
    }
    assert_raises(IndexError) {
      @iconList.moveItem(0, 2)
    }
    assert_raises(IndexError) {
      @iconList.moveItem(-1, 0)
    }
    assert_raises(IndexError) {
      @iconList.moveItem(2, 0)
    }
    assert_nothing_raised(IndexError) {
      @iconList.moveItem(0, 0)
      @iconList.moveItem(0, 1)
      @iconList.moveItem(1, 0)
      @iconList.moveItem(1, 1)
    }
    assert_equal(0, @iconList.moveItem(0, 1))
    assert_equal(1, @iconList.moveItem(1, 0))
  end

  def test_SEL_REPLACED
    @iconList.appendItem("One")
    @iconList.appendItem("Two")
    itemIndex = 0
    @iconList.connect(SEL_REPLACED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @iconList.setItem(1, "", nil, nil, nil, true)
    assert_equal(1, itemIndex)
  end
  
  def test_SEL_INSERTED
    @iconList.appendItem("One")
    @iconList.appendItem("Two")
    itemIndex = 0
    @iconList.connect(SEL_INSERTED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @iconList.insertItem(1, "One Point Five", nil, nil, nil, true)
    assert_equal(1, itemIndex)
  end
  
  def test_SEL_DELETED
    @iconList.appendItem("One")
    @iconList.appendItem("Two")
    itemIndex = 0
    @iconList.connect(SEL_DELETED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @iconList.removeItem(1, true)
    assert_equal(1, itemIndex)
  end

  def test_SEL_SELECTED
    @iconList.appendItem("One")
    @iconList.appendItem("Two")
    itemIndex = 0
    @iconList.connect(SEL_SELECTED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @iconList.selectItem(1, true)
    assert_equal(1, itemIndex)
  end

  def test_SEL_DESELECTED
    @iconList.appendItem("One")
    @iconList.appendItem("Two")
    itemIndex = 0
    @iconList.connect(SEL_DESELECTED) { |sender, sel, ptr|
      itemIndex = ptr
    }
    @iconList.selectItem(1, true)
    @iconList.deselectItem(1, true)
    assert_equal(1, itemIndex)
  end
  
  def test_makeItemVisible
    items = []
    0.upto(2) { |i|
      items << @iconList.appendItem("item#{i}")
    }
    assert_raises(IndexError) {
      @iconList.makeItemVisible(-1)
    }
    assert_raises(IndexError) {
      @iconList.makeItemVisible(3)
    }
  end
end
