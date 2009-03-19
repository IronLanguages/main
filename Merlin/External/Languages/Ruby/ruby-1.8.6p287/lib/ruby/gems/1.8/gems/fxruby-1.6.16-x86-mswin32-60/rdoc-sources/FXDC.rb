module Fox
  #
  # Line segment
  # 
  class FXSegment 
    # x-coordinate of the starting point [Integer]
    attr_accessor :x1
    
    # y-coordinate of the starting point [Integer]
    attr_accessor :y1
    
    # x-coordinate of the endpoint [Integer]
    attr_accessor :x2
    
    # y-coordinate of the endpoint [Integer]
    attr_accessor :y2
  end

  #
  # Arc
  #
  class FXArc 
    # x-coordinate of center point [Integer]
    attr_accessor :x
    
    # y-coordinate of center point [Integer]
    attr_accessor :y
    
    # Width [Integer]
    attr_accessor :w
    
    # Height [Integer]
    attr_accessor :h
    
    # Start of the arc, relative to the three-o'clock position from the center, in units of degrees * 64 [Integer]
    attr_accessor :a
    
    # Path and extent of the arc, relative to the three-o'clock position from the center, in units of degrees * 64 [Integer]
    attr_accessor :b 
  end

  #
  # A device context is used to maintain the state of the graphics drawing system.
  # Defining your drawing code in terms of the Abstract Device Context allows the
  # drawing commands to be rendered on different types of surfaces, such as windows
  # and images (FXDCWindow), or on paper (FXDCPrint).
  # WYSYWYG may be obtained by using the same identical drawing code in your
  # application regardless of the actual device surface being utilized.
  #
  # === Drawing (BITBLT) functions
  #
  # +BLT_CLR+::                        D := 0
  # +BLT_SRC_AND_DST+::                D := S & D
  # +BLT_SRC_AND_NOT_DST+::            D := S & ~D
  # +BLT_SRC+::                        D := S
  # +BLT_NOT_SRC_AND_DST+::            D := ~S & D
  # +BLT_DST+::                        D := D
  # +BLT_SRC_XOR_DST+::                D := S ^ D
  # +BLT_SRC_OR_DST+::                 D := S | D
  # +BLT_NOT_SRC_AND_NOT_DST+::        D := ~S & ~D  ==  D := ~(S | D)
  # +BLT_NOT_SRC_XOR_DST+::            D := ~S ^ D
  # +BLT_NOT_DST+::                    D := ~D
  # +BLT_SRC_OR_NOT_DST+::             D := S | ~D
  # +BLT_NOT_SRC+::                    D := ~S
  # +BLT_NOT_SRC_OR_DST+::             D := ~S | D
  # +BLT_NOT_SRC_OR_NOT_DST+::         D := ~S | ~D  ==  ~(S & D)
  # +BLT_SET+::                        D := 1
  #
  # === Line Styles
  #
  # +LINE_SOLID+::        Solid lines
  # +LINE_ONOFF_DASH+::   On-off dashed lines
  # +LINE_DOUBLE_DASH+::  Double dashed lines
  #
  # === Line Cap Styles
  #
  # +CAP_NOT_LAST+::    Don't include last end cap
  # +CAP_BUTT+::        Butting line end caps
  # +CAP_ROUND+::       Round line end caps
  # +CAP_PROJECTING+::  Projecting line end caps
  #
  # === Line Join Styles
  #
  # +JOIN_MITER+::    Mitered or pointy joints
  # +JOIN_ROUND+::    Round line joints
  # +JOIN_BEVEL+::    Beveled or flat joints
  #
  # === Fill Styles
  #
  # +FILL_SOLID+::              Fill with solid color
  # +FILL_TILED+::              Fill with tiled bitmap 
  # +FILL_STIPPLED+::           Fill where stipple mask is 1
  # +FILL_OPAQUESTIPPLED+::     Fill with foreground where mask is 1, background otherwise
  #
  # === Fill Rules
  #
  # +RULE_EVEN_ODD+::   Even odd polygon filling
  # +RULE_WINDING+::    Winding rule polygon filling
  #
  # === Stipple/dither patterns
  #
  # <tt>STIPPLE_0</tt>::		Stipple pattern 0
  # <tt>STIPPLE_NONE</tt>::		Stipple pattern 0
  # <tt>STIPPLE_BLACK</tt>::		All ones
  # <tt>STIPPLE_1</tt>::		Stipple pattern 1
  # <tt>STIPPLE_2</tt>::		Stipple pattern 2
  # <tt>STIPPLE_3</tt>::		Stipple pattern 3
  # <tt>STIPPLE_4</tt>::		Stipple pattern 4
  # <tt>STIPPLE_5</tt>::		Stipple pattern 5
  # <tt>STIPPLE_6</tt>::		Stipple pattern 6
  # <tt>STIPPLE_7</tt>::		Stipple pattern 7
  # <tt>STIPPLE_8</tt>::		Stipple pattern 8
  # <tt>STIPPLE_GRAY</tt>::		50% gray
  # <tt>STIPPLE_9</tt>::		Stipple pattern 9
  # <tt>STIPPLE_10</tt>::		Stipple pattern 10
  # <tt>STIPPLE_11</tt>::		Stipple pattern 11
  # <tt>STIPPLE_12</tt>::		Stipple pattern 12
  # <tt>STIPPLE_13</tt>::		Stipple pattern 13
  # <tt>STIPPLE_14</tt>::		Stipple pattern 14
  # <tt>STIPPLE_15</tt>::		Stipple pattern 15
  # <tt>STIPPLE_16</tt>::		Stipple pattern 16
  # <tt>STIPPLE_WHITE</tt>::		All zeroes
  # <tt>STIPPLE_HORZ</tt>::		Horizontal hatch pattern
  # <tt>STIPPLE_VERT</tt>::		Vertical hatch pattern
  # <tt>STIPPLE_CROSS</tt>::		Cross-hatch pattern
  # <tt>STIPPLE_DIAG</tt>::		Diagonal // hatch pattern
  # <tt>STIPPLE_REVDIAG</tt>::		Reverse diagonal \\ hatch pattern
  # <tt>STIPPLE_CROSSDIAG</tt>::	Cross-diagonal hatch pattern

  class FXDC

    # Application [FXApp]
    attr_reader	:app
    
    # Foreground drawing color [FXColor]
    attr_accessor :foreground
    
    # Background drawing color [FXColor]
    attr_accessor :background
    
    # Dash pattern [String]
    attr_reader	:dashPattern
    
    # Dash offset [Integer]
    attr_reader	:dashOffset
    
    # Dash length [Integer]
    attr_reader	:dashLength
    
    # Line width; a line width of zero means thinnest and fastest possible [Integer]
    attr_accessor :lineWidth
    
    # Line cap style, one of +CAP_NOT_LAST+, +CAP_BUTT+, +CAP_ROUND+ or +CAP_PROJECTING+ [Integer]
    attr_accessor :lineCap
    
    # Line join style, one of +JOIN_MITER+, +JOIN_ROUND+ or +JOIN_BEVEL+ [Integer]
    attr_accessor :lineJoin
    
    # Line style, one of +LINE_SOLID+, +LINE_ONOFF_DASH+ or +LINE_DOUBLE_DASH+ [Integer]
    attr_accessor :lineStyle
    
    # Fill style, one of +FILL_SOLID+, +FILL_TILED+, +FILL_STIPPLED+ or +FILL_OPAQUESTIPPLED+ [Integer]
    attr_accessor :fillStyle
    
    # Fill rule, one of +RULE_EVEN_ODD+ or +RULE_WINDING+ [Integer]
    attr_accessor :fillRule
    
    # Raster op function, one of +BLT_CLR+, +BLT_SRC+, +BLT_DST+, etc. (see list above) [Integer]
    attr_accessor :function
    
    # Tile image [FXImage]
    attr_accessor :tile
    
    # Stipple pattern [FXBitmap or Integer]
    attr_accessor :stipple
    
    # Clip region [FXRegion]
    attr_writer	:clipRegion
    
    # Clip rectangle [FXRectangle]
    attr_reader	:clipRectangle
    
    # X-coordinate of clip rectangle [Integer]
    attr_reader	:clipX
    
    # Y-coordinate of clip rectangle [Integer]
    attr_reader	:clipY
    
    # Width of clip rectangle, in pixels [Integer]
    attr_reader	:clipWidth
    
    # Height of clip rectangle, in pixels [Integer]
    attr_reader	:clipHeight
    
    # Font to draw text with [FXFont]
    attr_accessor :font

    # Construct dummy DC
    def initialize(app) ; end

    #
    # Returns a color value (i.e. an FXColor) for the pixel at (_x_, _y_).
    #
    # ==== Parameters:
    #
    # +x+::	x-coordinate of the pixel of interest [Integer]
    # +y+::	y-coordinate of the pixel of interest [Integer]
    #
    def readPixel(x, y) ; end
  
    #
    # Draw a point at (_x_, _y_) in the current foreground drawing color.
    #
    # ==== Parameters:
    #
    # +x+::	x-coordinate of the point [Integer]
    # +y+::	y-coordinate of the point [Integer]
    #
    # See also #drawPoints and #drawPointsRel.
    #
    def drawPoint(x, y) ; end
    
    #
    # Draw multiple points, where _points_ is an array of FXPoint instances.
    #
    # ==== Parameters:
    #
    # +points+::	array of FXPoint instances [Array]
    #
    # See also #drawPoint and #drawPointsRel.
    #
    def drawPoints(points) ; end
    
    #
    # Draw multiple points, where _points_ is an array of FXPoint instances.
    # Unlike #drawPoints, where each of the points is drawn relative to the
    # origin, #drawPointsRel treats all coordinates after the first as relative
    # to the previous point.
    #
    # ==== Parameters:
    #
    # +points+::	array of FXPoint instances [Array]
    #
    # See also #drawPoint and #drawPoints.
    #
    def drawPointsRel(points) ; end
  
    #
    # Draw the line from (<em>x1</em>, <em>y1</em>) to (<em>x2</em>, <em>y2</em>).
    #
    # ==== Parameters:
    #
    # <tt>x1</tt>::	x-coordinate of the starting point [Integer]
    # <tt>y1</tt>::	y-coordinate of the starting point [Integer]
    # <tt>x2</tt>::	x-coordinate of the ending point [Integer]
    # <tt>y2</tt>::	y-coordinate of the ending point [Integer]
    #
    # See also #drawLines and #drawLinesRel.
    #
    def drawLine(x1, y1, x2, y2) ; end
    
    #
    # Draw connected lines, where _points_ is an array of FXPoint instances.
    # The number of lines drawn is equal to the size of the _points_
    # array minus one.
    # Treats all points' coordinates as relative to the origin.
    #
    # ==== Parameters:
    #
    # +points+::	array of FXPoint instances that defines all points on the line [Array]
    #
    # See also #drawLine and #drawLinesRel.
    #
    def drawLines(points) ; end
    
    #
    # Draw connected lines, where _points_ is an array of FXPoint instances.
    # The number of lines drawn is equal to the size of the _points_
    # array minus one.
    # Treats each point's coordinates (after the first) as relative to
    # the previous point.
    #
    # ==== Parameters:
    #
    # +points+::	array of FXPoint instances that defines all points on the line [Array]
    #
    # See also #drawLine and #drawLines.
    #
    def drawLinesRel(points) ; end
    
    #
    # Draw mutiple, unconnected lines (i.e. line segments), where _segments_ is
    # an array of FXSegment instances.
    #
    # === Parameters:
    #
    # +segments+::	an array of FXSegment instances [Array]
    #
    def drawLineSegments(segments) ; end
  
    #
    # Draw rectangle with upper-left corner at (_x_, _y_) and with width and height (_w_, _h_).
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of upper-left corner of the rectangle [Integer]
    # +y+::	y-coordinate of upper-left corner of the rectangle [Integer]
    # +width+::	width of the rectangle, in pixels [Integer]
    # +height+::	height of the rectangle, in pixels [Integer]
    #
    # See also #drawRectangles, #fillRectangle and #fillRectangles.
    #
    def drawRectangle(x, y, w, h) ; end
    
    #
    # Draw multiple rectangles, where _rectangles_ is an array of FXRectangle instances.
    #
    # === Parameters:
    #
    # +rectangles+::	an array of FXRectangle instances [Array]
    #
    # See also #drawRectangle, #fillRectangle and #fillRectangles.
    #
    def drawRectangles(rectangles) ; end
  
    #
    # Draw a rounded rectangle with ellipse width _ew_ and ellipse height _eh_.
    #
    # === Parameters:
    #
    # <tt>x</tt>::	x-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>y</tt>::	y-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>w</tt>::	width of the bounding rectangle, in pixels [Integer]
    # <tt>h</tt>::	height of the bounding rectangle, in pixels [Integer]
    #
    def drawRoundRectangle(x, y, w, h, ew, eh); end

    #
    # Draw an arc.
    # The argument <em>start</em> specifies the start of the arc relative to
    # the three-o'clock position from the center, in units of degrees*64.
    # The argument <em>extent</em> specifies the path and extent of the arc,
    # relative to the start of the arc (also in units of degrees*64).
    # The arguments _x_, _y_, _w_, and _h_ specify the bounding rectangle
    # of the arc.
    #
    # === Parameters:
    #
    # <tt>x</tt>::	x-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>y</tt>::	y-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>w</tt>::	width of the bounding rectangle, in pixels [Integer]
    # <tt>h</tt>::	height of the bounding rectangle, in pixels [Integer]
    # <tt>start</tt>::	starting angle of the arc, in 64ths of a degree [Integer]
    # <tt>extent</tt>::	the path and extent of the arc, relative to the start of the arc (in 64ths of a degree) [Integer]
    #
    # See also #drawArcs, #fillArc and #fillArcs.
    #
    def drawArc(x, y, w, h, start, extent) ; end
    
    #
    # Draw arcs, where _arcs_ is an array of FXArc instances.
    #
    # === Parameters:
    #
    # +arcs+::	an array of FXArc instances [Array]
    #
    # See also #drawArc, #fillArc and #fillArcs.
    #
    def drawArcs(arcs) ; end

    #
    # Draw an ellipse.
    #
    def drawEllipse(x, y, w, h); end

    #
    # Draw filled rectangle with upper-left corner at (_x_, _y_) and with width and height (_w_, _h_).
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the upper left corner of the rectangle [Integer]
    # +y+::	y-coordinate of the upper left corner of the rectangle [Integer]
    # +width+::	width of the rectangle, in pixels [Integer]
    # +height+::	height of the rectangle, in pixels [Integer]
    #
    # See also #drawRectangle, #drawRectangles and #fillRectangles.
    #
    def fillRectangle(x, y, w, h) ; end
    
    #
    # Draw filled rectangles, where _rectangles_ is an array of FXRectangle instances.
    #
    # === Parameters:
    #
    # +rectangles+::	an array of FXRectangle instances [Array]
    #
    # See also #drawRectangle, #drawRectangles and #fillRectangle.
    #
    def fillRectangles(rectangles) ; end

    #
    # Draw a filled rounded rectangle with ellipse width _ew_ and ellipse height _eh_.
    #
    # === Parameters:
    #
    # <tt>x</tt>::	x-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>y</tt>::	y-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>w</tt>::	width of the bounding rectangle, in pixels [Integer]
    # <tt>h</tt>::	height of the bounding rectangle, in pixels [Integer]
    #
    def fillRoundRectangle(x, y, w, h, ew, eh); end

    def fillChord(x, y, w, h, ang1, ang2) ; end
    def fillChords(chords, nchords) ; end
  
    #
    # Draw filled arc (see documentation for #drawArc).
    #
    # === Parameters:
    #
    # <tt>x</tt>::	x-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>y</tt>::	y-coordinate of the upper left corner of the bounding rectangle [Integer]
    # <tt>w</tt>::	width of the bounding rectangle, in pixels [Integer]
    # <tt>h</tt>::	height of the bounding rectangle, in pixels [Integer]
    # <tt>start</tt>::	starting angle of the arc, in 64ths of a degree [Integer]
    # <tt>extent</tt>::	the path and extent of the arc, relative to the start of the arc (in 64ths of a degree) [Integer]
    #
    # See also #drawArc, #drawArcs and #fillArcs.
    #
    def fillArc(x, y, w, h, start, extent) ; end
    
    #
    # Draw filled arcs, where _arcs_ is an array of FXArc instances.
    #
    # === Parameters:
    #
    # +arcs+::	an array of FXArc instances [Array]
    #
    # See also #drawArc, #drawArcs and #fillArc.
    #
    def fillArcs(arcs) ; end
  
    #
    # Draw a filled ellipse.
    #
    def fillEllipse(x, y, w, h); end

    #
    # Draw filled polygon, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillPolygon(points) ; end

    #
    # Draw filled polygon, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillConcavePolygon(points) ; end

    #
    # Draw filled polygon, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillComplexPolygon(points) ; end
  
    #
    # Draw filled polygon with relative points, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillPolygonRel(points) ; end

    #
    # Draw filled polygon with relative points, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillConcavePolygonRel(points) ; end

    #
    # Draw filled polygon with relative points, where _points_ is an array of FXPoint instances.
    #
    # === Parameters:
    #
    # +points+::	an array of FXPoint instances [Array]
    #
    def fillComplexPolygonRel(points) ; end
  
    #
    # Draw hashed box with upper-left corner at (_x_, _y_) and with width and height (_w_, _h_).
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the upper left corner [Integer]
    # +y+::	y-coordinate of the upper left corner [Integer]
    # +width+::	width of the box, in pixels [Integer]
    # +height+::	height of the box, in pixels [Integer]
    # +b+::	border width, in pixels [Integer]
    #
    def drawHashBox(x, y, w, h, b=1) ; end
  
    #
    # Draw focus rectangle with upper-left corner at (_x_, _y_) and with width and height (_w_, _h_).
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the upper left corner [Integer]
    # +y+::	y-coordinate of the upper left corner [Integer]
    # +width+::	width of the rectangle, in pixels [Integer]
    # +height+::	height of the rectangle, in pixels [Integer]
    #
    def drawFocusRectangle(x, y, w, h) ; end
    
    #
    # Copy some rectangular area from _source_ into the drawable attached to this
    # device context.
    #
    # === Parameters:
    #
    # +source+::	the source drawable from which to copy [FXDrawable]
    # +sx+::		x-coordinate of the upper left corner of the source rectangle [Integer]
    # +sy+::		y-coordinate of the upper left corner of the source rectangle [Integer]
    # +sw+::		width of the source rectangle, in pixels [Integer]
    # +sh+::		height of the source rectangle, in pixels [Integer]
    # +dx+::		x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::		y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawArea(source, sx, sy, sw, sh, dx, dy) ; end
  
    #
    # Copy some rectangular area from _source_ into the drawable attached to this
    # device context, stretching it to width _dw_ and height _dh_.
    #
    # === Parameters:
    #
    # +source+::	the source drawable from which to copy [FXDrawable]
    # +sx+::		x-coordinate of the upper left corner of the source rectangle [Integer]
    # +sy+::		y-coordinate of the upper left corner of the source rectangle [Integer]
    # +sw+::		width of the source rectangle, in pixels [Integer]
    # +sh+::		height of the source rectangle, in pixels [Integer]
    # +dx+::		x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::		y-coordinate of the the destination point in this drawable [Integer]
    # +dw+::		destination width, in pixels [Integer]
    # +dh+::		destination height, in pixels [Integer]
    #
    def drawArea(source, sx, sy, sw, sh, dx, dy, dw, dh) ; end

    #
    # Draw _image_ into the drawable attached to this device context.
    #
    # === Parameters:
    #
    # +image+::	image to draw [FXImage]
    # +dx+::	x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::	y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawImage(image, dx, dy) ; end
  
    #
    # Draw _bitmap_ into the drawable attached to this device context.
    #
    # === Parameters:
    #
    # +bitmap+::	bitmap to draw [FXBitmap]
    # +dx+::		x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::		y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawBitmap(bitmap, dx, dy) ; end
  
    #
    # Draw _icon_ into the drawable attached to this device context.
    #
    # === Parameters:
    #
    # +icon+::	icon to draw [FXIcon]
    # +dx+::	x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::	y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawIcon(icon, dx, dy) ; end
    
    #
    # Draw a shaded version of an icon into the drawable attached to this device context.
    # This is typically used for drawing disabled labels and buttons.
    #
    # === Parameters:
    #
    # +icon+::	icon to draw [FXIcon]
    # +dx+::	x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::	y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawIconShaded(icon, dx, dy) ; end
    
    #
    # Draw a sunken version of an icon into the drawable attached to this device context.
    #
    # === Parameters:
    #
    # +icon+::	icon to draw [FXIcon]
    # +dx+::	x-coordinate of the the destination point in this drawable [Integer]
    # +dy+::	y-coordinate of the the destination point in this drawable [Integer]
    #
    def drawIconSunken(icon, dx, dy) ; end
  
    #
    # Draw _string_ at position (_x_, _y_).
    #
    # === Parameters:
    #
    # +x+::		x-coordinate of the upper left corner [Integer]
    # +y+::		y-coordinate of the upper left corner [Integer]
    # +string+::	the text string to draw [String]
    #
    # See also #drawImageText.
    #
    def drawText(x, y, string) ; end
    
    #
    # Draw _string_ at position (_x_, _y_).
    #
    # === Parameters:
    #
    # +x+::		x-coordinate of the upper left corner [Integer]
    # +y+::		y-coordinate of the upper left corner [Integer]
    # +string+::	the text string to draw [String]
    #
    # See also #drawText.
    #
    def drawImageText(x, y, string) ; end
  
    #
    # Set dash pattern and dash offset.
    # A dash pattern of [1, 2, 3, 4] is a repeating pattern of 1 foreground pixel, 
    # 2 background pixels, 3 foreground pixels, and 4 background pixels.
    # The offset is where in the pattern the system will start counting.
    # The maximum length of the dash pattern array is 32 elements.
    #
    # === Parameters:
    #
    # +dashOffset+::	indicates which element of the dash pattern to start with (zero for the beginning) [Integer]
    # +dashPattern+::	array of integers indicating the dash pattern [Array]
    #
    def setDashes(dashOffset, dashPattern) ; end
  
    #
    # Set clip rectangle.
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the upper left corner of the clip rectangle [Integer]
    # +y+::	y-coordinate of the upper left corner of the clip rectangle [Integer]
    # +width+::	width of the clip rectangle, in pixels [Integer]
    # +height+::	height of the clip rectangle, in pixels [Integer]
    #
    # See also #setClipMask.
    #
    def setClipRectangle(x, y, w, h) ; end
    
    #
    # Set clip rectangle.
    #
    # === Parameters:
    #
    # +rectangle+::	a rectangle that defines the clipping region [Integer]
    #
    # See also #setClipMask.
    #
    def setClipRectangle(rectangle) ; end
    
    #
    # Clear clipping.
    #
    def clearClipRectangle() ; end
  
    #
    # Set clip mask to _bitmap_.
    #
    # === Parameters:
    #
    # +bitmap+::	a bitmap to use for clipping [FXBitmap]
    # +dx+::		[Integer]
    # +dy+::		[Integer]
    #
    # See also #setClipRectangle.
    #
    def setClipMask(bitmap, dx=0, dy=0) ; end
  
    #
    # Clear clip mask.
    #
    def clearClipMask() ; end

    #
    # When you call #clipChildren with the argument +true+, anything that you
    # draw into this window will be clipped by its child windows. In other words,
    # the child windows "obscure" the parent window. This is the default behavior.
    # If you call #clipChildren with +false+, anything that you draw into this
    # window will be visible in its child windows (i.e. the drawing will *not*
    # be clipped).
    #
    # === Parameters:
    #
    # +yes+::	if +true+, drawing is clipped against child windows [Boolean]
    #
    def clipChildren(yes) ; end
  end
end

