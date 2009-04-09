require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXList < TestCase
  def setup
    super(self.class.name)
    @list = FXList.new(mainWindow)
  end
  
  def test_numVisible
    @list.numVisible = 7
    assert_equal(7, @list.numVisible)
  end
  
  def test_appendItem_byItem
    items = []
    0.upto(4) do |i|
      items << FXListItem.new("item#{i}")
    end
    assert_equal(0, @list.numItems)
    @list.appendItem(items[0])
    assert_equal(1, @list.numItems)
    @list.appendItem(items[1], true)
    assert_equal(2, @list.numItems)
    @list.appendItem(items[2], false)
    assert_equal(3, @list.numItems)
    assert_raises(ArgumentError) do
      @list.appendItem(items[3], 42) # second argument must be true or false
    end
    assert_equal(3, @list.numItems)
  end

  def test_appendOp
    assert_equal(0, @list.numItems)
    @list << FXListItem.new("item1")
    assert_equal(1, @list.numItems)
    @list << FXListItem.new("item2")
    assert_equal(2, @list.numItems)
    @list << FXListItem.new("item3")
    assert_equal(3, @list.numItems)
  end
  
  def test_appendItem_byText
    assert_equal(0, @list.numItems)
    itemIndex = @list.appendItem("")
    assert_equal(1, @list.numItems)
    itemIndex = @list.appendItem("anItem")
    assert_equal(2, @list.numItems)
    itemIndex = @list.appendItem("anItem", nil)
    assert_equal(3, @list.numItems)
    itemIndex = @list.appendItem("anItem", nil, "someData")
    assert_equal(4, @list.numItems)
    itemIndex = @list.appendItem("anItem", nil, "someData", true)
    assert_equal(5, @list.numItems)
    itemIndex = @list.appendItem("anItem", nil, "someData", false)
    assert_equal(6, @list.numItems)
    assert_raises(ArgumentError) do
      @list.appendItem("anItem", nil, "someData", 42) # last argument must be true or false
    end
    assert_equal(6, @list.numItems)
  end
  
  def test_getItem
    assert_raises(IndexError) {
      @list.getItem(0)
    }
    theItem = FXListItem.new("anItem")
    @list << theItem
    retrievedItem = nil
    assert_nothing_raised {
      retrievedItem = @list.getItem(0)
    }
    assert_same(theItem, retrievedItem)
  end

  def test_moveItem
    @list.appendItem("First")
    @list.appendItem("Second")
    assert_raises(IndexError) {
      @list.moveItem(0, -1)
    }
    assert_raises(IndexError) {
      @list.moveItem(0, 2)
    }
    assert_raises(IndexError) {
      @list.moveItem(-1, 0)
    }
    assert_raises(IndexError) {
      @list.moveItem(2, 0)
    }
    assert_nothing_raised {
      @list.moveItem(0, 0)
      @list.moveItem(0, 1)
      @list.moveItem(1, 0)
      @list.moveItem(1, 1)
    }
    assert_equal(0, @list.moveItem(0, 1))
    assert_equal(1, @list.moveItem(1, 0))
  end
  
  def test_makeItemVisible
    items = []
    0.upto(2) { |i|
      items << @list.appendItem("item#{i}")
    }
    assert_raises(IndexError) {
      @list.makeItemVisible(-1)
    }
    assert_raises(IndexError) {
      @list.makeItemVisible(3)
    }
  end
end
