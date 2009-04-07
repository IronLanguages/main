require 'test/unit'

require 'fox16'

include Fox

class TC_FXFont < Test::Unit::TestCase
  def setup
    if FXApp.instance.nil?
      @app = FXApp.new('TC_FXFont', 'FoxTest') 
    else
      @app = FXApp.instance
    end
  end

  def testConstructFromFontDescription
    fontdesc = @app.normalFont.fontDesc
    font = FXFont.new(@app, fontdesc)
  end

  def testConstructFromParameters
    # Check default argument values
    fontdesc = @app.normalFont.fontDesc
    font1 = FXFont.new(@app, fontdesc.face, fontdesc.size)
    font2 = FXFont.new(@app, fontdesc.face, fontdesc.size, FXFont::Normal, FXFont::Straight, FONTENCODING_DEFAULT, FXFont::NonExpanded, 0)
    assert_equal(font1.name, font2.name)
    assert_equal(font1.size, font2.size)
    assert_equal(font1.weight, font2.weight)
    assert_equal(font1.slant, font2.slant)
    assert_equal(font1.encoding, font2.encoding)
    assert_equal(font1.setWidth, font2.setWidth)
    assert_equal(font1.hints, font2.hints)
    assert_equal(font1.isFontMono, font2.isFontMono)
    assert_equal(font1.minChar, font2.minChar)
    assert_equal(font1.maxChar, font2.maxChar)
    assert_equal(font1.fontWidth, font2.fontWidth)
    assert_equal(font1.fontHeight, font2.fontHeight)
    assert_equal(font1.fontAscent, font2.fontAscent)
    assert_equal(font1.fontDescent, font2.fontDescent)
    assert_equal(font1.fontLeading, font2.fontLeading)
    assert_equal(font1.fontSpacing, font2.fontSpacing)
  end

  def testConstructFromFontString
    font = FXFont.new(@app, "")
  end
  
  def testGetTextWidthAndHeight
    font = FXFont.new(@app, "Times", 10)
    assert(font.getTextWidth("Test") > 0)
    assert(font.getTextHeight("Test") > 0)
  end

  def test_listFonts
    fonts = FXFont.listFonts("")
    assert_instance_of(Array, fonts)
    assert(fonts.length > 0)
  end
  
  def test_hasChar?
    @app.normalFont.create
    assert(@app.normalFont.hasChar('a'))
    assert(@app.normalFont.hasChar(?a))
    assert(@app.normalFont.hasChar?('a'))
    assert(@app.normalFont.hasChar?(?a))
    assert_raises(ArgumentError) { @app.normalFont.hasChar? "" }
    assert_raises(ArgumentError) { @app.normalFont.hasChar? "ab" }
  end
end
