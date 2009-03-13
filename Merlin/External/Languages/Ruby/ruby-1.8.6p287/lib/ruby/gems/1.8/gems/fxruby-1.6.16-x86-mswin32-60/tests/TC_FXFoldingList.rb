require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXFoldingList < TestCase
  
  def setup
    super(self.class.name)
    @foldingList = FXFoldingList.new(mainWindow)
  end

  def test_each_for_empty_list
    count = 0
    @foldingList.each { |item| count += 1 }
    assert_equal(0, count, "count for empty list should be zero")
  end
  
  def test_each
    @foldingList.appendItem(nil, "1")
    @foldingList.appendItem(nil, "2")
    @foldingList.appendItem(nil, "3")
    @foldingList.appendItem(nil, "4")
    @foldingList.appendItem(nil, "5")
    count = 0
    @foldingList.each { |item| count += 1 }
    assert_equal(5, count, "count didn't match expected number of items")
  end
  
end

