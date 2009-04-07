require 'test/unit'

require 'fox16'

include Fox

class TC_FXDC < Test::Unit::TestCase
  def setup
    if FXApp.instance.nil?
      @app = FXApp.new('TC_FXDC', 'FXRuby')
      @app.init([])
    else
      @app = FXApp.instance
    end
    @dc = FXDC.new(@app)
  end
  
  def testGetApp
    app = @dc.app
    assert(app)
    assert_kind_of(FXApp, app)
    assert_same(@app, app)
  end
  
  def testReadPixel
    x, y = 0, 0
    pixel = @dc.readPixel(x, y)
    assert(pixel)
  end
  
  def testDrawPoint
    @dc.drawPoint(0, 0)
  end
  
  def testDrawPoints
    p1 = FXPoint.new
    p2 = FXPoint.new
    @dc.drawPoints([p1, p2])
  end
  
  def testDrawPointsRel
    p1 = FXPoint.new
    p2 = FXPoint.new
    @dc.drawPointsRel([p1, p2])
  end
  
  def testDrawLine
    x1, y1, x2, y2 = 0, 0, 5, 5
    @dc.drawLine(x1, y1, x2, y2)
  end
  
  def testDrawLines
    points = [ FXPoint.new, FXPoint.new ]
    @dc.drawLines(points)
  end
  
  def testDrawLinesRel
    points = [ FXPoint.new, FXPoint.new ]
    @dc.drawLinesRel(points)
  end
  
  def testDrawLineSegments
    segments = [ FXSegment.new, FXSegment.new ]
    @dc.drawLineSegments(segments)
  end
  
  def testDrawArc
    x, y, w, h, ang1, ang2 = 0, 0, 10, 10, 45, 135
    @dc.drawArc(x, y, w, h, ang1, ang2)
  end
  
  def testDrawArcs
    arcs = [ FXArc.new, FXArc.new ]
    @dc.drawArcs(arcs)
  end
  
  def testFillRectangle
    x, y, w, h = 0, 0, 20, 20
    @dc.fillRectangle(x, y, w, h)
  end
  
  def testFillRectangles(rectangles)
    rectangles = [ FXRectangle.new, FXRectangle.new ]
    @dc.fillRectangles(rectangles)
  end
  
  def testFillArc
    x, y, w, h, ang1, ang2 = 0, 0, 10, 10, 45, 135
    @dc.fillArc(x, y, w, h, ang1, ang2)
  end
  
  def testFillArcs
    arcs = [ FXArc.new, FXArc.new ]
    @dc.fillArcs(arcs)
  end
  
  def testFillPolygon
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillPolygon(points)
  end
  
  def testFillConcavePolygon
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillConcavePolygon(points)
  end
  
  def testFillComplexPolygon
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillComplexPolygon(points)
  end
  
  def testFillPolygonRel
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillPolygonRel(points)
  end

  def testFillConcavePolygonRel
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillConcavePolygonRel(points)
  end

  def testFillComplexPolygonRel
    points = [ FXPoint.new, FXPoint.new ]
    @dc.fillComplexPolygonRel(points)
  end
  
  def testDrawHashBox
    x, y, w, h, b = 0, 0, 20, 20, 2
    @dc.drawHashBox(x, y, w, h)
    @dc.drawHashBox(x, y, w, h, b)
  end
  
  def testDrawFocusRectangle
    x, y, w, h = 0, 0, 5, 5
    @dc.drawFocusRectangle(x, y, w, h)
  end
  
  def testDrawArea
    source = FXImage.new(@app)
    sx, sy, sw, sh = 0, 0, 10, 10
    dx, dy = 0, 0
    @dc.drawArea(source, sx, sy, sw, sh, dx, dy)
  end
  
  def testDrawImage
    image, dx, dy = FXImage.new(@app), 0, 0
    @dc.drawImage(image, dx, dy)
  end
  
  def testDrawBitmap
    bitmap, dx, dy = FXBitmap.new(@app), 0, 0
    @dc.drawBitmap(bitmap, dx, dy)
  end
  
  def testDrawIcon
    icon, dx, dy = FXIcon.new(@app), 0, 0
    @dc.drawIcon(icon, dx, dy)
  end
  
  def testDrawIconSunken
    icon, dx, dy = FXIcon.new(@app), 0, 0
    @dc.drawIconSunken(icon, dx, dy)
  end
  
  def testDrawIconShaded
    icon, dx, dy = FXIcon.new(@app), 0, 0
    @dc.drawIconShaded(icon, dx, dy)
  end
  
  def testDrawText
    x, y, str = 0, 0, "Hello"
    @dc.drawText(x, y, str)
  end
  
  def testDrawImageText
    x, y, str = 0, 0, "Hello"
    @dc.drawImageText(x, y, str)
  end
  
  def testForeground
    fg = FXRGB(192, 192, 192)
    @dc.setForeground(fg)
    assert_equal(fg, @dc.foreground)
    assert_equal(fg, @dc.getForeground)
    @dc.foreground = fg
    assert_equal(fg, @dc.foreground)
    assert_equal(fg, @dc.getForeground)
  end

  def testBackground
    bg = FXRGB(192, 192, 192)
    @dc.setBackground(bg)
    assert_equal(bg, @dc.background)
    assert_equal(bg, @dc.getBackground)
    @dc.background = bg
    assert_equal(bg, @dc.background)
    assert_equal(bg, @dc.getBackground)
  end
  
  def testDashes
    dashOffset, dashPattern = 0, [1, 2, 3, 4]
    @dc.setDashes(dashOffset, dashPattern)
    assert_equal(dashPattern, @dc.dashPattern)
    assert_equal(dashPattern, @dc.getDashPattern())
    assert_equal(dashOffset, @dc.dashOffset)
    assert_equal(dashOffset, @dc.getDashOffset())
  end
  
  def testLineWidth
    lineWidth = 2
    @dc.setLineWidth(lineWidth)
    assert_equal(lineWidth, @dc.lineWidth)
    assert_equal(lineWidth, @dc.getLineWidth())
    @dc.lineWidth = lineWidth
    assert_equal(lineWidth, @dc.lineWidth)
    assert_equal(lineWidth, @dc.getLineWidth())
  end

  def testLineCap
    for lineCap in [CAP_NOT_LAST, CAP_BUTT, CAP_ROUND, CAP_PROJECTING]
      @dc.setLineCap(lineCap)
      assert_equal(lineCap, @dc.lineCap)
      assert_equal(lineCap, @dc.getLineCap())
      @dc.lineCap = lineCap
      assert_equal(lineCap, @dc.lineCap)
      assert_equal(lineCap, @dc.getLineCap())
    end
  end

  def testLineJoin
    for lineJoin in [JOIN_MITER, JOIN_ROUND, JOIN_BEVEL]
      @dc.setLineJoin(lineJoin)
      assert_equal(lineJoin, @dc.lineJoin)
      assert_equal(lineJoin, @dc.getLineJoin())
      @dc.lineJoin = lineJoin
      assert_equal(lineJoin, @dc.lineJoin)
      assert_equal(lineJoin, @dc.getLineJoin())
    end
  end

  def testLineStyle
    for lineStyle in [LINE_SOLID, LINE_ONOFF_DASH, LINE_DOUBLE_DASH]
      @dc.setLineStyle(lineStyle)
      assert_equal(lineStyle, @dc.lineStyle)
      assert_equal(lineStyle, @dc.getLineStyle())
      @dc.lineStyle = lineStyle
      assert_equal(lineStyle, @dc.lineStyle)
      assert_equal(lineStyle, @dc.getLineStyle())
    end
  end

  def testFillStyle
    for fillStyle in [FILL_SOLID, FILL_TILED, FILL_STIPPLED, FILL_OPAQUESTIPPLED]
      @dc.setFillStyle(fillStyle)
      assert_equal(fillStyle, @dc.fillStyle)
      assert_equal(fillStyle, @dc.getFillStyle())
      @dc.fillStyle = fillStyle
      assert_equal(fillStyle, @dc.fillStyle)
      assert_equal(fillStyle, @dc.getFillStyle())
    end
  end

  def testFillRule
    for fillRule in [RULE_EVEN_ODD, RULE_WINDING]
      @dc.setFillRule(fillRule)
      assert_equal(fillRule, @dc.fillRule)
      assert_equal(fillRule, @dc.getFillRule())
      @dc.fillRule = fillRule
      assert_equal(fillRule, @dc.fillRule)
      assert_equal(fillRule, @dc.getFillRule())
    end
  end
  
  def testFunction
    for func in [BLT_CLR, BLT_SRC_AND_DST, BLT_SRC_AND_NOT_DST, BLT_SRC,
                 BLT_NOT_SRC_AND_DST, BLT_DST, BLT_SRC_XOR_DST, BLT_SRC_OR_DST,
                 BLT_NOT_SRC_AND_NOT_DST, BLT_NOT_SRC_XOR_DST, BLT_NOT_DST,
                 BLT_SRC_OR_NOT_DST, BLT_NOT_SRC, BLT_NOT_SRC_OR_DST,
                 BLT_NOT_SRC_OR_NOT_DST, BLT_SET]
      @dc.setFunction(func)
      assert_equal(func, @dc.function)
      assert_equal(func, @dc.getFunction())
      @dc.function = func
      assert_equal(func, @dc.function)
      assert_equal(func, @dc.getFunction())
    end
  end
  
  def testTile
    image, dx, dy = FXImage.new(@app), 0, 0
    @dc.setTile(image)
    @dc.setTile(image, dx)
    @dc.setTile(image, dx, dy)
    assert_same(image, @dc.tile)
    assert_same(image, @dc.getTile())
  end
  
  def testStippleBitmap
    bitmap, dx, dy = FXBitmap.new(@app), 0, 0
    @dc.setStipple(bitmap)
    @dc.setStipple(bitmap, dx)
    @dc.setStipple(bitmap, dx, dy)
    assert_same(bitmap, @dc.stippleBitmap)
    assert_same(bitmap, @dc.getStippleBitmap())
  end
  
  def testStipplePattern
    dx, dy = 0, 0
    patterns = [STIPPLE_0, STIPPLE_NONE, STIPPLE_BLACK, STIPPLE_1,
                STIPPLE_2, STIPPLE_3, STIPPLE_4, STIPPLE_5, STIPPLE_6,
                STIPPLE_7, STIPPLE_8, STIPPLE_GRAY, STIPPLE_9, STIPPLE_10,
                STIPPLE_11, STIPPLE_12, STIPPLE_13, STIPPLE_14, STIPPLE_15,
                STIPPLE_16, STIPPLE_WHITE, STIPPLE_HORZ, STIPPLE_VERT, STIPPLE_CROSS,
                STIPPLE_DIAG, STIPPLE_REVDIAG, STIPPLE_CROSSDIAG]
    for pat in patterns
      @dc.setStipple(pat)
      @dc.setStipple(pat, dx)
      @dc.setStipple(pat, dx, dy)
      assert_equal(pat, @dc.stipplePattern)
      assert_equal(pat, @dc.getStipplePattern())
    end
  end
  
# def testClipRegion
#   region = FXRegion.new(0, 0, 10, 10)
#   @dc.setClipRegion(region)
# end
  
  def testClipRectangle
    clipX, clipY, clipWidth, clipHeight = 0, 0, 10, 20
    clipRectangle = FXRectangle.new(clipX, clipY, clipWidth, clipHeight)

    @dc.setClipRectangle(clipX, clipY, clipWidth, clipHeight)
#   assert_equal(clipX, @dc.clipX)
#   assert_equal(clipY, @dc.clipY)
#   assert_equal(clipWidth, @dc.clipWidth)
#   assert_equal(clipHeight, @dc.clipHeight)
#   assert_equal(clipRectangle, @dc.clipRectangle)

    @dc.setClipRectangle(clipRectangle)
#   assert_equal(clipX, @dc.clipX)
#   assert_equal(clipY, @dc.clipY)
#   assert_equal(clipWidth, @dc.clipWidth)
#   assert_equal(clipHeight, @dc.clipHeight)
#   assert_equal(clipRectangle, @dc.clipRectangle)
    
    @dc.clearClipRectangle
  end
  
  def testClipMask
    bitmap, dx, dy = FXBitmap.new(@app), 0, 0
    @dc.setClipMask(bitmap)
    @dc.setClipMask(bitmap, dx)
    @dc.setClipMask(bitmap, dx, dy)
    @dc.clearClipMask
  end
  
  def testTextFont
    textFont = @app.normalFont
    @dc.setFont(textFont)
    assert_same(textFont, @dc.font)
    assert_same(textFont, @dc.getFont())
    @dc.font = textFont
    assert_same(textFont, @dc.font)
    assert_same(textFont, @dc.getFont())
  end
  
  def testClipChildren
    @dc.clipChildren(true)
    @dc.clipChildren(false)
  end
end
