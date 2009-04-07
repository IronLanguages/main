module Fox
  #
  # The Bitmap View widget display a scrollable view of a monochrome bitmap image;
  # the bitmap is not owned by the bitmap frame so it must be explicitly deleted
  # elsewhere.  Thus, a single bitmap image can be displayed inside multiple bitmap
  # view widgets.
  #
  # === Bitmap alignment styles
  #
  # +BITMAPVIEW_NORMAL+::	Normal mode is centered
  # +BITMAPVIEW_CENTER_X+::	Centered horizontally
  # +BITMAPVIEW_LEFT+::		Left-aligned
  # +BITMAPVIEW_RIGHT+::	Right-aligned
  # +BITMAPVIEW_CENTER_Y+::	Centered vertically
  # +BITMAPVIEW_TOP+::		Top-aligned
  # +BITMAPVIEW_BOTTOM+::	Bottom-aligned
  #
  # === Events
  #
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  #
  class FXBitmapView < FXScrollArea
  
    # The bitmap [FXBitmap]
    attr_accessor :bitmap
    
    # The color used for the "on" bits in the bitmap [FXColor]
    attr_accessor :onColor

    # The color used for the "off" bits in the bitmap [FXColor]
    attr_accessor :offColor

    # Current alignment [Integer]
    attr_accessor :alignment

    #
    # Return an initialized FXBitmapView instance.
    #
    def initialize(p, bmp=nil, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theBitmapView
    end
  end
end

