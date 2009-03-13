require 'test/unit'
require 'fox16'
require 'fox16/colors'
require 'testcase'

include Fox

SAMPLE = "The quick brown fox jumped over the lazy dog"

class TC_FXText < TestCase

private

  def setup_text
    @text = FXText.new(mainWindow)
    @text.text = SAMPLE
  end

  def setup_styled_text
    @styledText = FXText.new(mainWindow)

    hs1 = FXHiliteStyle.new
    hs1.normalForeColor = FXColor::Red
    hs1.normalBackColor = FXColor::Blue
    hs1.selectForeColor = @styledText.selTextColor
    hs1.selectBackColor = @styledText.selBackColor
    hs1.hiliteForeColor = @styledText.hiliteTextColor
    hs1.hiliteBackColor = @styledText.hiliteBackColor
    hs1.activeBackColor = @styledText.activeBackColor
    hs1.style = 0

    hs2 = FXHiliteStyle.new
    hs2.normalForeColor = FXColor::Blue
    hs2.normalBackColor = FXColor::Yellow
    hs2.selectForeColor = @styledText.selTextColor
    hs2.selectBackColor = @styledText.selBackColor
    hs2.hiliteForeColor = @styledText.hiliteTextColor
    hs2.hiliteBackColor = @styledText.hiliteBackColor
    hs2.activeBackColor = @styledText.activeBackColor
    hs2.style = FXText::STYLE_UNDERLINE

    @styledText.styled = true
    @styledText.hiliteStyles = [hs1, hs2]
    @styledText.text = SAMPLE
    @styledText.changeStyle(SAMPLE.index("quick"), "quick".length, 1)
    @styledText.changeStyle(SAMPLE.index("lazy"), "lazy".length, 2)
  end

public

  def setup
    super(self.class.name)
    setup_text
    setup_styled_text
  end

  def test_extractStyle
    assert_nil(@text.extractStyle(0, 5))
    assert_equal("", @styledText.extractStyle(0, 0))
    assert_equal("\0"*3, @styledText.extractStyle(0, 3))
    assert_equal("\1"*"quick".length, @styledText.extractStyle(SAMPLE.index("quick"), "quick".length))
    assert_equal("\2"*"lazy".length, @styledText.extractStyle(SAMPLE.index("lazy"), "lazy".length))
  end

  def test_extractText
    assert_equal("", @text.extractText(0, 0))
    assert_equal(SAMPLE, @text.extractText(0, SAMPLE.length))
    assert_equal("brown", @text.extractText(10, 5))
  end

  def test_getText
    assert_equal(SAMPLE, @text.text)
  end
  
  def test_find_text
    @text.text = "99 bottles of beer"
    startIndex, endIndex = @text.findText("bottles")
    assert_equal([3], startIndex)
    assert_equal([10], endIndex)
  end

  def test_find_text_with_startpos
    @text.text = "I came, I saw, I conquered"
    startIndex, endIndex = @text.findText("I ", 5)
    assert_equal([8], startIndex)
    assert_equal([10], endIndex)
  end

  def test_find_text_with_groups
    @text.text = "I came, I saw, I conquered"
    startIndex, endIndex = @text.findText("I ([a-z]+)(, )?", 0, SEARCH_REGEX)
    assert_equal([0, 2, 6], startIndex)
    assert_equal([8, 6, 8], endIndex)
  end

end
