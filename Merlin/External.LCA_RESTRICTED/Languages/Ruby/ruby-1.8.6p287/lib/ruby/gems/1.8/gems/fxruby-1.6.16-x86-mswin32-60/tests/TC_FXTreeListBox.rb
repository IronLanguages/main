require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXTreeListBox < TestCase
  
  def setup
    super(self.class.name)
    @treeListBox = FXTreeListBox.new(mainWindow)
  end

  def test_sortRootItems
    @treeListBox.appendItem(nil, "B")
    @treeListBox.appendItem(nil, "A")
    @treeListBox.appendItem(nil, "C")
    @treeListBox.sortRootItems
    assert_equal("A", @treeListBox.firstItem.text)
    assert_equal("B", @treeListBox.firstItem.next.text)
    assert_equal("C", @treeListBox.lastItem.text)
  end

  def test_each_for_empty_list
    count = 0
    @treeListBox.each { |item| count += 1 }
    assert_equal(0, count, "count for empty list should be zero")
  end
  
  def test_each
    @treeListBox.appendItem(nil, "1")
    @treeListBox.appendItem(nil, "2")
    @treeListBox.appendItem(nil, "3")
    @treeListBox.appendItem(nil, "4")
    count = 0
    @treeListBox.each { |item| count += 1 }
    assert_equal(4, count, "count didn't match expected number of items")
  end
  
end

