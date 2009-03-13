require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXMenuRadio < TestCase
  def setup
    super(self.class.name)
    @menuRadio = FXMenuRadio.new(mainWindow, "menuRadio")
  end
  
  def test_setCheck_TRUE
    @menuRadio.check = Fox::TRUE
    assert_equal(true, @menuRadio.check)
    assert_equal(Fox::TRUE, @menuRadio.checkState)
    assert(@menuRadio.checked?)
    assert(!@menuRadio.unchecked?)
    assert(!@menuRadio.maybe?)
  end
  
  def test_setCheck_FALSE
    @menuRadio.check = Fox::FALSE
    assert_equal(false, @menuRadio.check)
    assert_equal(Fox::FALSE, @menuRadio.checkState)
    assert(!@menuRadio.checked?)
    assert(@menuRadio.unchecked?)
    assert(!@menuRadio.maybe?)
  end
  
  def test_setCheck_MAYBE
    @menuRadio.check = Fox::MAYBE
    assert_equal(true, @menuRadio.check) # this is not a typo!
    assert_equal(Fox::MAYBE, @menuRadio.checkState)
    assert(!@menuRadio.checked?)
    assert(!@menuRadio.unchecked?)
    assert(@menuRadio.maybe?)
  end
  
  def test_setCheck_true
    @menuRadio.check = true
    assert_equal(true, @menuRadio.check)
    assert_equal(Fox::TRUE, @menuRadio.checkState)
    assert(@menuRadio.checked?)
    assert(!@menuRadio.unchecked?)
    assert(!@menuRadio.maybe?)
  end
  
  def test_setCheck_false
    @menuRadio.check = false
    assert_equal(false, @menuRadio.check)
    assert_equal(Fox::FALSE, @menuRadio.checkState)
    assert(!@menuRadio.checked?)
    assert(@menuRadio.unchecked?)
    assert(!@menuRadio.maybe?)
  end
end
