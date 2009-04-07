require 'test/unit'

require 'fox16'

include Fox

class TC_FXFontDesc < Test::Unit::TestCase
  def setup
    @fontdesc = FXFontDesc.new
  end

  def test_face
    @fontdesc.face = "Times New Roman"
    assert_equal("Times New Roman", @fontdesc.face)
  end

  def test_size
    @fontdesc.size = 120
    assert_equal(120, @fontdesc.size)
  end

  def test_weight
    weights = [FXFont::Thin,
               FXFont::ExtraLight,
               FXFont::Light,
               FXFont::Normal,
               FXFont::Medium,
               FXFont::DemiBold,
               FXFont::Bold,
               FXFont::ExtraBold,
               FXFont::Black]
    weights.each do |weight|
      @fontdesc.weight = weight
      assert_equal(weight, @fontdesc.weight)
    end
  end

  def test_slant
    slants = [FXFont::ReverseOblique,
              FXFont::ReverseItalic,
	      FXFont::Straight,
	      FXFont::Italic,
	      FXFont::Oblique]
    slants.each do |slant|
      @fontdesc.slant = slant
      assert_equal(slant, @fontdesc.slant)
    end
  end

  def test_encoding
  end

  def test_setwidth
  end

  def test_flags
  end
end
