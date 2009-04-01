module Fox
  #
  # The bitmap frame is a simple frame widget displaying an monochrome bitmap
  # image; the bitmap is not owned by the bitmap frame so it must be explicitly
  # deleted elsewhere.
  #
  class FXBitmapFrame < FXFrame
  
    # The current image being displayed [FXBitmap]
    attr_accessor :bitmap
    
    # The color used for the "on" bits in the bitmap [FXColor]
    attr_accessor :onColor
    
    # The color used for the "off" bits in the bitmap [FXColor]
    attr_accessor :offColor
    
    #
    # The current justification mode, some combination of the flags
    # +JUSTIFY_LEFT+, +JUSTIFY_RIGHT+, +JUSTIFY_TOP+ and +JUSTIFY_BOTTOM+
    # [Integer]
    #
    attr_accessor :justify
    
    #
    # Return an initialized FXBitmapFrame instance.
    #
    def initialize(p, bmp, opts=FRAME_SUNKEN|FRAME_THICK, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0) # :yields: theBitmapFrame
    end
  end
end
