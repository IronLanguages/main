module Fox
  #
  # The scroll area widget manages a content area and a viewport
  # area through which the content is viewed.  When the content area
  # becomes larger than the viewport area, scrollbars are placed to
  # permit viewing of the entire content by scrolling the content.
  # Depending on the mode, scrollbars may be displayed on an as-needed
  # basis, always, or never.
  # Normally, the scroll area's size and the content's size are independent;
  # however, it is possible to disable scrolling in the horizontal
  # (vertical) direction.  In this case, the content width (height)
  # will influence the width (height) of the scroll area widget.
  # For content which is time-consuming to repaint, continuous
  # scrolling may be turned off.
  #
  # === Scrollbar options
  #
  # +SCROLLERS_NORMAL+::	Show the scrollbars when needed
  # +HSCROLLER_ALWAYS+::	Always show horizontal scrollers
  # +HSCROLLER_NEVER+::		Never show horizontal scrollers
  # +VSCROLLER_ALWAYS+::	Always show vertical scrollers
  # +VSCROLLER_NEVER+::		Never show vertical scrollers
  # +HSCROLLING_ON+::		Horizontal scrolling turned on (default)
  # +HSCROLLING_OFF+::		Horizontal scrolling turned off
  # +VSCROLLING_ON+::		Vertical scrolling turned on (default)
  # +VSCROLLING_OFF+::		Vertical scrolling turned off
  # +SCROLLERS_TRACK+::		Scrollers track continuously for smooth scrolling
  # +SCROLLERS_DONT_TRACK+::	Scrollers don't track continuously
  #
  class FXScrollArea < FXComposite

    # Viewport width, in pixels [Integer]
    attr_reader	:viewportWidth
    
    # Viewport height, in pixels [Integer]
    attr_reader	:viewportHeight
    
    # Content width, in pixels [Integer]
    attr_reader	:contentWidth
    
    # Content height, in pixels [Integer]
    attr_reader	:contentHeight
    
    # Scroll style [Integer]
    attr_accessor :scrollStyle
    
    # Horizontal scrollbar [FXScrollBar]
    attr_reader	:horizontalScrollBar
    
    # Vertical scrollbar [FXScrollBar]
    attr_reader	:verticalScrollBar
    
    # Current x-position [Integer]
    attr_reader	:xPosition
    
    # Current y-position [Integer]
    attr_reader	:yPosition

    #
    # Return an initialized FXScrollArea instance.
    #
    # ==== Parameters:
    #
    # +parent+::	the parent widget for this scroll area [FXComposite]
    # +opts+::		the options [Integer]
    # +x+::		initial x-position, when the +LAYOUT_FIX_X+ layout hint is in effect [Integer]
    # +y+::		initial y-position, when the +LAYOUT_FIX_Y+ layout hint is in effect [Integer]
    # +width+::		initial width, when the +LAYOUT_FIX_WIDTH+ layout hint is in effect [Integer]
    # +height+::	initial height, when the +LAYOUT_FIX_HEIGHT+ layout hint is in effect [Integer]
    #
    def initialize(parent, opts=0, x=0, y=0, width=0, height=0) # :yields: theScrollArea
    end
    
    # Return +true+ if horizontally scrollable
    def horizontalScrollable?() ; end

    # Return +true+ if vertically scrollable
    def verticalScrollable?() ; end

    # Set the current position to (_x_, _y_)
    def setPosition(x, y) ; end

    # Get the current position as an array [x, y]
    def position() ; end
  end
end
