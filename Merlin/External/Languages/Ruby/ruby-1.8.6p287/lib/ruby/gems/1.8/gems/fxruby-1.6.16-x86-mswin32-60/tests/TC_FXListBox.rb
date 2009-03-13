require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXListBox < TestCase
  def setup
    super(self.class.name)
    @listBox = FXListBox.new(mainWindow)
  end
  
  def test_appendItem
    assert_equal(0, @listBox.numItems)
    @listBox.appendItem("An item")
    assert_equal(1, @listBox.numItems)
  end

  def test_appendOp
    assert_equal(0, @listBox.numItems)
    @listBox << "An item"
    assert_equal(1, @listBox.numItems)
  end

  def test_moveItem
    @listBox.appendItem("First")
    @listBox.appendItem("Second")
    assert_raises(IndexError) {
      @listBox.moveItem(0, -1)
    }
    assert_raises(IndexError) {
      @listBox.moveItem(0, 2)
    }
    assert_raises(IndexError) {
      @listBox.moveItem(-1, 0)
    }
    assert_raises(IndexError) {
      @listBox.moveItem(2, 0)
    }
    assert_nothing_raised {
      @listBox.moveItem(0, 0)
      @listBox.moveItem(0, 1)
      @listBox.moveItem(1, 0)
      @listBox.moveItem(1, 1)
    }
    assert_equal(0, @listBox.moveItem(0, 1))
    assert_equal(1, @listBox.moveItem(1, 0))
  end
end
