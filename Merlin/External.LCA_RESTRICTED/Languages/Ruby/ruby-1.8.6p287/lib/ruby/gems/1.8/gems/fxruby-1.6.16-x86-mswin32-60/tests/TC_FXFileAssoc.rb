require 'test/unit'

require 'fox16'

include Fox

class TC_FXFileAssoc < Test::Unit::TestCase
  def setup
    @app = FXApp.instance || FXApp.new
    @icon = FXIcon.new(@app)
    @fileassoc = FXFileAssoc.new
  end
  
  def test_command
    @fileassoc.command = "netscape"
    assert_equal("netscape", @fileassoc.command)
  end
  
  def test_extension
    @fileassoc.extension = ".html"
    assert_equal(".html", @fileassoc.extension)
  end
  
  def test_mimetype
    @fileassoc.mimetype = "mimetype"
    assert_equal("mimetype", @fileassoc.mimetype)
  end
  
  def test_bigicon
    @fileassoc.bigicon = @icon
    assert_kind_of(FXIcon, @fileassoc.bigicon)
    assert_same(@icon, @fileassoc.bigicon)
  end

  def test_bigiconopen
    @fileassoc.bigiconopen = @icon
    assert_kind_of(FXIcon, @fileassoc.bigiconopen)
    assert_same(@icon, @fileassoc.bigiconopen)
  end

  def test_miniicon
    @fileassoc.miniicon = @icon
    assert_kind_of(FXIcon, @fileassoc.miniicon)
    assert_same(@icon, @fileassoc.miniicon)
  end

  def test_miniiconopen
    @fileassoc.miniiconopen = @icon
    assert_kind_of(FXIcon, @fileassoc.miniiconopen)
    assert_same(@icon, @fileassoc.miniiconopen)
  end

  def test_dragtype
    @fileassoc.dragtype = 0
    assert_equal(0, @fileassoc.dragtype)
  end
  
  def test_flags
    @fileassoc.flags = 0
    assert_equal(0, @fileassoc.flags)
  end
end
