require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class TC_FXMessageBox < TestCase
  def setup
    super(self.class.name)
  end
  
  def test_construct_with_save_cancel_dontsave
    assert_nothing_raised(RangeError) do
      FXMessageBox.new(mainWindow, "Save?", "Save?", :opts => MBOX_SAVE_CANCEL_DONTSAVE)
    end
  end
  
  def test_mbox_clicked_dontsave_defined
    assert(Fox.const_defined?(:MBOX_CLICKED_DONTSAVE))
  end
  
  def test_mbox_clicked_dontsave_equal_to_mbox_clicked_no
    assert_equal(MBOX_CLICKED_NO, MBOX_CLICKED_DONTSAVE)
  end
end
