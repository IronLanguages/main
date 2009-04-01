module Fox
  #
  # Status bar
  #
  # === Status bar options
  #
  # +STATUSBAR_WITH_DRAGCORNER+:: Causes the drag corner to be shown
  #
  class FXStatusBar < FXHorizontalFrame
  
    # The status line widget [FXStatusLine]
    attr_reader :statusLine
    
    # The drag corner widget [FXDragCorner]
    attr_reader :dragCorner
    
    # If +true+, the drag corner is shown [Boolean]
    attr_accessor :cornerStyle
  
    #
    # Return an initialized FXStatusBar instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this status bar [FXComposite]
    # +opts+::	status bar options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    # +hSpacing+::	horizontal spacing between widgets, in pixels [Integer]
    # +vSpacing+::	vertical spacing between widgets, in pixels [Integer]
    #
    def initialize(p, opts=0, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2, hSpacing=4, vSpacing=0) # :yields: theStatusBar
    end
  end
end

