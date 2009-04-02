module Fox
  #
  # A dock title is used to move its container, a dock bar.
  # The dock title is also used simultaneously to provide a
  # caption above the dock bar.
  #
  class FXDockTitle < FXDockHandler
    # Caption text for the grip [String]
    attr_accessor :caption
    
    # Caption font [FXFont]
    attr_accessor :font
    
    # Caption color [FXColor]
    attr_accessor :captionColor
    
    # Current justification mode [Integer]
    attr_accessor :justify

    #
    # Construct dock bar title widget
    #
    def initialize(p, text, target=nil, selector=0, opts=FRAME_NORMAL|JUSTIFY_CENTER_X|JUSTIFY_CENTER_Y, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0)
    end
  end
end
