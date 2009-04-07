require 'test/unit'

require 'fox16'

class TC_Misc < Test::Unit::TestCase

  DELTA = 1.0e-5

  def test_MKUINT
    assert_equal(0, Fox.MKUINT(Fox::MINKEY, Fox::MINTYPE))
    assert_equal(65535, Fox.MKUINT(Fox::MAXKEY, Fox::MINTYPE))
    assert_equal(4294901760, Fox.MKUINT(Fox::MINKEY, Fox::MAXTYPE))
    assert_equal(4294967295, Fox.MKUINT(Fox::MAXKEY, Fox::MAXTYPE))
  end

  def test_FXSEL
    assert_equal(0, Fox.FXSEL(Fox::MINTYPE, Fox::MINKEY))
    assert_equal(65535, Fox.FXSEL(Fox::MINTYPE, Fox::MAXKEY))
    assert_equal(4294901760, Fox.FXSEL(Fox::MAXTYPE, Fox::MINKEY))
    assert_equal(4294967295, Fox.FXSEL(Fox::MAXTYPE, Fox::MAXKEY))
  end

  def test_FXSELTYPE
    assert_equal(Fox::MINTYPE, Fox.FXSELTYPE(0))
    assert_equal(Fox::MINTYPE, Fox.FXSELTYPE(65535))
    assert_equal(Fox::MAXTYPE, Fox.FXSELTYPE(4294901760))
    assert_equal(Fox::MAXTYPE, Fox.FXSELTYPE(4294967295))
  end

  def test_FXSELID
    assert_equal(Fox::MINKEY, Fox.FXSELID(0))
    assert_equal(Fox::MAXKEY, Fox.FXSELID(65535))
    assert_equal(Fox::MINKEY, Fox.FXSELID(4294901760))
    assert_equal(Fox::MAXKEY, Fox.FXSELID(4294967295))
  end

  def test_FXRGB
    # result depends on endian-ness of platform!
  end

  def test_FXRGBA
    # result depends on endian-ness of platform!
  end

  def test_FXREDVAL
    assert_equal(1, Fox.FXREDVAL(Fox.FXRGB(1, 0, 0)))
  end

  def test_FXGREENVAL
    assert_equal(1, Fox.FXGREENVAL(Fox.FXRGB(0, 1, 0)))
  end

  def test_FXBLUEVAL
    assert_equal(1, Fox.FXBLUEVAL(Fox.FXRGB(0, 0, 1)))
  end

  def test_FXALPHAVAL
    assert_equal(1, Fox.FXALPHAVAL(Fox.FXRGBA(0, 0, 0, 1)))
  end

  def test_FXRGBACOMPVAL
    clr = Fox.FXRGBA(0, 1, 2, 3)
    0.upto(3) { |i|
      assert_equal(i, Fox.FXRGBACOMPVAL(clr, i))
    }
  end

  def test_fxparseAccel
  end

  def test_fxparseHotKey
  end

  def test_fxfindhotkeyoffset
  end

  def test_makeHiliteColor
  end

  def test_makeShadowColor
  end

  def test_fxcolorfromname
  end

  def test_fxnamefromcolor
  end

  def test_fxhsv_to_rgb
    h, s, v = 180.0, 0.0, 1.0
    r, g, b = Fox.fxhsv_to_rgb(h, s, v)
    assert_in_delta(v, r, DELTA)
    assert_in_delta(v, g, DELTA)
    assert_in_delta(v, b, DELTA)

    h, s, v = 180.0, 0.5, 1.0
    r, g, b = Fox.fxhsv_to_rgb(h, s, v)
    assert_in_delta(0.5, r, DELTA)
    assert_in_delta(1.0, g, DELTA)
    assert_in_delta(1.0, b, DELTA)

    h, s, v = 0.0, 0.5, 1.0
    r, g, b = Fox.fxhsv_to_rgb(h, s, v)
    assert_in_delta(1.0, r, DELTA)
    assert_in_delta(0.5, g, DELTA)
    assert_in_delta(0.5, b, DELTA)

    h, s, v = 360.0, 0.5, 1.0
    r, g, b = Fox.fxhsv_to_rgb(h, s, v)
    assert_in_delta(1.0, r, DELTA)
    assert_in_delta(0.5, g, DELTA)
    assert_in_delta(0.5, b, DELTA)
  end

  def test_fxrgb_to_hsv
    r, g, b = 0.0, 0.0, 0.0
    h, s, v = Fox.fxrgb_to_hsv(r, g, b)
    assert_in_delta(0.0, h, DELTA)
    assert_in_delta(0.0, s, DELTA)
    assert_in_delta(0.0, v, DELTA)

    r, g, b = 0.5, 0.5, 0.5
    h, s, v = Fox.fxrgb_to_hsv(r, g, b)
    assert_in_delta(0.0, h, DELTA)
    assert_in_delta(0.0, s, DELTA)
    assert_in_delta(0.5, v, DELTA)

    r, g, b = 1.0, 0.0, 0.0
    h, s, v = Fox.fxrgb_to_hsv(r, g, b)
    assert_in_delta(0.0, h, DELTA)
    assert_in_delta(1.0, s, DELTA)
    assert_in_delta(1.0, v, DELTA)

    r, g, b = 0.0, 1.0, 0.0
    h, s, v = Fox.fxrgb_to_hsv(r, g, b)
    assert_in_delta(120.0, h, DELTA)
    assert_in_delta(1.0, s, DELTA)
    assert_in_delta(1.0, v, DELTA)

    r, g, b = 0.0, 0.0, 1.0
    h, s, v = Fox.fxrgb_to_hsv(r, g, b)
    assert_in_delta(240.0, h, DELTA)
    assert_in_delta(1.0, s, DELTA)
    assert_in_delta(1.0, v, DELTA)
  end
  
  def test_fxversion
    assert_instance_of(String, Fox.fxversion)
  end
  
  def test_fxrubyversion
    assert_instance_of(String, Fox.fxrubyversion)
  end

  def test_fxTraceLevel
  end
end
