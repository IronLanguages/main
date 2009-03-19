module Fox
  #
  # The FXScrollWindow widget scrolls an arbitrary child window.
  # Use the scroll window when parts of the user interface itself
  # need to be scrolled, for example when applications need to run
  # on small screens.  The scroll window observes some layout hints of 
  # its content-window; it observes +LAYOUT_FIX_WIDTH+, +LAYOUT_FIX_HEIGHT+
  # at all times.  The hints +LAYOUT_FILL_X+, +LAYOUT_LEFT+, +LAYOUT_RIGHT+, 
  # +LAYOUT_CENTER_X+, as well as +LAYOUT_FILL_Y+, +LAYOUT_TOP+, +LAYOUT_BOTTOM+, 
  # +LAYOUT_CENTER_Y+ are however only interpreted if the content size
  # is smaller than the viewport size, because if the content size is
  # larger than the viewport size, then content must be scrolled.
  # Note that this means that the content window's position is not 
  # necessarily equal to the scroll position of the scroll window!
  #
  class FXScrollWindow < FXScrollArea
    #
    # Return an initialized FXScrollWindow instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this scroll window [FXComposite]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position, when the +LAYOUT_FIX_X+ layout hint is in effect [Integer]
    # +y+::	initial y-position, when the +LAYOUT_FIX_Y+ layout hint is in effect [Integer]
    # +width+::	initial width, when the +LAYOUT_FIX_WIDTH+ layout hint is in effect [Integer]
    # +height+::	initial height, when the +LAYOUT_FIX_HEIGHT+ layout hint is in effect [Integer]
    #
    def initialize(p, opts=0, x=0, y=0, width=0, height=0) # :yields: theScrollWindow
    end
  
    #
    # Return a reference to the contents window (an FXWindow instance).
    #
    def contentWindow; end
  end
end

