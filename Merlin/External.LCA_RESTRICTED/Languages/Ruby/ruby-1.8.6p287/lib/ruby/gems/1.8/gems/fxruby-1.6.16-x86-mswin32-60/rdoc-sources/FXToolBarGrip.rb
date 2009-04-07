module Fox
  #
  # A tool bar grip is used to move its container, a dock bar.
  # The grip draws either a single or double bar; it is customary
  # to use the single bar grip for toolbar-rearrangements only,
  # and use the double-bar when the toolbar needs to be floated
  # or docked.
  # The tool bar grip is automatically oriented properly by the
  # the toolbar widget, similar to the FXSeparator widget.
  # Holding the Control Key while dragging the grip will prevent
  # the toolbar from docking when it is near a dock site.
  #
  # === Toolbar Grip styles
  #
  # +TOOLBARGRIP_SINGLE+::	Single bar mode for movable tool bars
  # +TOOLBARGRIP_DOUBLE+::	Double bar mode for dockable tool bars
  #
  class FXToolBarGrip < FXDockHandler

    # Indicates whether the grip is displayed as a double-bar [Boolean]
    attr_writer	:doubleBar
    
    # Active color [FXColor]
    attr_accessor :activeColor

    #
    # Return an initialized FXToolBarGrip instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tool bar grip [FXComposite]
    # +target+::	the message target, if any, for this tool bar grip [FXObject]
    # +selector+::	the message identifier for this tool bar grip [Integer]
    # +opts+::	tool bar grip options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=TOOLBARGRIP_SINGLE, x=0, y=0, width=0, height=0) # :yields: theToolBarGrip
    end

    #
    # Return +true+ if this tool bar grip is displayed as a double bar (i.e. as
    # opposed to a single bar).
    #
    def doubleBar? ; end
  end
end

