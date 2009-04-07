require 'test/unit'
require 'fox16'
require 'testcase'

include Fox

class TC_FXIconDict < TestCase
  def setup
    super(self.class.name)
  end
  
  def test_defaultIconPath_s
    assert_equal("~/.foxicons:/usr/local/share/icons:/usr/share/icons", FXIconDict.defaultIconPath)
  end

  def test_empty
    iconDict = FXIconDict.new(app)
    assert(iconDict.empty?)
  end
  
  def test_defaultIconPath
    iconDict = FXIconDict.new(app)
    assert_equal(FXIconDict.defaultIconPath, iconDict.iconPath)
  end
  
  def test_iconPath
    iconDict = FXIconDict.new(app, "foo")
    assert_equal("foo", iconDict.iconPath)
    iconDict.iconPath = "bar"
    assert_equal("bar", iconDict.iconPath)
  end
  
  def test_insert
  end
=begin
  def test_remove_existing_icon
    iconDict = FXIconDict.new(app)
    iconDict.insert("gnu-animal.xpm")
    assert_equal(1, iconDict.size)
    assert_nil(iconDict.remove("gnu-animal.xpm"))
    assert_equal(0, iconDict.size)
  end
 
  def test_remove_nonexistent_icon
    iconDict = FXIconDict.new(app)
    assert_nil(iconDict.remove("xxxxx.png"))
  end
=end
  def test_find
  end
end

