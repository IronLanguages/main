module Fox
  #
  # Cursor class
  #
  # === Stock cursors
  #
  # +CURSOR_ARROW+::		Default left pointing arrow
  # +CURSOR_RARROW+::		Right arrow
  # +CURSOR_IBEAM+::		Text I-Beam
  # +CURSOR_WATCH+::		Stopwatch or hourglass
  # +CURSOR_CROSS+::		Crosshair
  # +CURSOR_UPDOWN+::		Move up, down
  # +CURSOR_LEFTRIGHT+::	Move left, right
  # +CURSOR_MOVE+::		    Move up, down, left, right
  #
  # === Cursor options
  #
  # +CURSOR_KEEP+::			Keep pixel data in client
  # +CURSOR_OWNED+::		Pixel data is owned by cursor
  #
  class FXCursor < FXId

    # Width of cursor, in pixels (returns zero for stock cursors) [Integer]
    attr_reader	:width
    
    # Height of cursor, in pixels (returns zero for stock cursors) [Integer]
    attr_reader	:height
    
    # Hotspot x-coordinate (returns zero for stock cursors) [Integer]
    attr_accessor :hotX
    
    # Hotspot y-coordinate (returns zero for stock cursors) [Integer]
    attr_accessor :hotY

    #
    # Make stock cursor, where _stockCursorId_ is one of the stock cursors
    # (+CURSOR_ARROW+, +CURSOR_RARROW+, etc.)
    #
    def initialize(a, curid=CURSOR_ARROW) # :yields: theCursor
    end
  
    #
    # Make cursor from _src_ and _msk_; cursor size should be 32x32 for portability!
    #
    def initialize(a, pix, width=32, height=32, hotX=-1, hotY=-1) # :yields: theCursor
    end
  
    #
    # Make cursor from FXColor pixels; cursor size should be 32x32 for portability!
    #
    def initialize(a, pixels, width=32, height=32, hotX=-1, hotY=-1) # :yields: theCursor
    end

    #
    # Save pixel data only.
    #
    def savePixels(stream) ; end
  
    #
    # Load pixel data only.
    #
    def loadPixels(stream) ; end
    
    # Return +true+ if there is color in the cursor.
    def color?; end
  end
end
