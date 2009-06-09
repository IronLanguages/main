module Fox
  #
  # The scroll bar is used when a document has a larger content than may be made
  # visible.  The range is the total size of the document, the page is the part
  # of the document which is visible.  The size of the scrollbar thumb is adjusted
  # to give feedback of the relative sizes of each.
  # The scroll bar may be manipulated by the left mouse button (normal scrolling), by the
  # middle mouse button (same as the left mouse only the scroll position can jump to the 
  # place where the click is made), or by the right mouse button (vernier- or fine-scrolling).
  # Holding down the control key while scrolling with the left or middle mouse button also 
  # enables vernier-scrolling mode.  The vernier-scrolling mode is very useful for accurate
  # positioning in large documents.
  # Finally, if the mouse sports a wheel, the scroll bar can be manipulated by means
  # of the mouse wheel as well.  Holding down the Control-key during wheel motion
  # will cause the scrolling to go faster than normal.
  #
  # === Events
  #
  # The following messages are sent by FXScrollBar to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  # +SEL_CHANGED+::
  #   sent continuously while the scroll bar is moving; the message data is an integer
  #   indicating the current position of the scroll bar.
  # +SEL_COMMAND+::
  #   sent at the end of a scrolling operation, to signal that the scrolling is complete.
  #   The message data is an integer indicating the new position of the scroll bar.
  #
  # === Scrollbar styles
  #
  # +SCROLLBAR_HORIZONTAL+::	Horizontally oriented
  # +SCROLLBAR_VERTICAL+::	Vertically oriented (the default)
  #
  # === Message identifiers
  #
  # +ID_TIMEWHEEL+::	x
  # +ID_AUTOINC_LINE+::	x
  # +ID_AUTODEC_LINE+::	x
  # +ID_AUTOINC_PAGE+::	x
  # +ID_AUTODEC_PAGE+::	x
  # +ID_AUTOINC_PIX+::	x
  # +ID_AUTODEC_PIX+::	x
  #
  class FXScrollBar < FXWindow
    # Content size range [Integer]
    attr_accessor :range
    
    # Viewport page size [Integer]
    attr_accessor :page
    
    # Scroll increment for line [Integer]
    attr_accessor :line
    
    # Current scroll position [Integer]
    attr_accessor :position
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor

    # Shadow color [FXColor]
    attr_accessor :shadowColor

    # Border color [FXColor]
    attr_accessor :borderColor

    # Scroll bar style [Integer]
    attr_accessor :scrollbarStyle
    
    # Bar size [Integer]
    attr_accessor :barSize

    #
    # Return an initialized FXScrollBar instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this scroll bar [FXComposite]
    # +target+::	the initial message target (if any) for this scroll bar [FXObject]
    # +selector+::	the message identifier for this scroll bar [Integer]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position, when the +LAYOUT_FIX_X+ layout hint is in effect [Integer]
    # +y+::	initial y-position, when the +LAYOUT_FIX_Y+ layout hint is in effect [Integer]
    # +width+::	initial width, when the +LAYOUT_FIX_WIDTH+ layout hint is in effect [Integer]
    # +height+::	initial height, when the +LAYOUT_FIX_HEIGHT+ layout hint is in effect [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=SCROLLBAR_VERTICAL, x=0, y=0, width=0, height=0) # :yields: theScrollBar
    end
  end
  
  #
  # Corner between scroll bars
  #
  class FXScrollCorner < FXWindow
    #
    # Return an initialized FXScrollCorner instance, where _p_ is the
    # parent window (an FXComposite instance).
    #
    def initialize(p) # :yields: theScrollCorner
    end
  end
end

