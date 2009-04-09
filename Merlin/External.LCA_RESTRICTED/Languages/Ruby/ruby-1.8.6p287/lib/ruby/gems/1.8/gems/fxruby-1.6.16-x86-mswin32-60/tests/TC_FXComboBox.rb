require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXComboBox < TestCase
  def setup
    super(self.class.name)
    @comboBox = FXComboBox.new(mainWindow, 1)
  end

  def test_moveItem
    @comboBox.appendItem("First")
    @comboBox.appendItem("Second")
    assert_nothing_raised {
      @comboBox.moveItem(0, 0)
      @comboBox.moveItem(0, 1)
      @comboBox.moveItem(1, 0)
      @comboBox.moveItem(1, 1)
    }
    assert_raises(IndexError) {
      @comboBox.moveItem(2, 0)
    }
    assert_raises(IndexError) {
      @comboBox.moveItem(-1, 0)
    }
    assert_raises(IndexError) {
      @comboBox.moveItem(0, 2)
    }
    assert_raises(IndexError) {
      @comboBox.moveItem(0, -1)
    }
  end

  def test_first
    assert_instance_of(FXTextField, @comboBox.first)
  end

  def test_children
    assert_instance_of(FXTextField, @comboBox.children[0])
    assert_instance_of(FXMenuButton, @comboBox.children[1])
  end
  
  def test_set_current_to_none
    assert_nothing_raised do
      @comboBox.currentItem = -1
    end
  end

  def test_fill_items_returns_num_items_added
    assert_equal(3, @comboBox.fillItems(%w{one two three}))
  end

  def test_fill_items
    @comboBox.fillItems(%w{one two three})
    items = @comboBox.map { |text, data| text }
    assert_equal("one", items[0])
    assert_equal("two", items[1])
    assert_equal("three", items[2])
  end
end

