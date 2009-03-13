module Fox
  #
  # An FXImageFrame is a simple frame widget that displays an FXImage image.
  #
  class FXImageFrame < FXFrame
  
    # The current image being displayed [FXImage]
    attr_accessor :image
    
    #
    # The current justification mode, some combination of the flags
    # +JUSTIFY_LEFT+, +JUSTIFY_RIGHT+, +JUSTIFY_TOP+ and +JUSTIFY_BOTTOM+ [Integer]
    #
    attr_accessor :justify
    
    #
    # Return an initialized FXImageFrame instance.
    #
    # ==== Parameters:
    #
    # +p+::         the parent window for this image frame [FXComposite]
    # +img+::       the image to display [FXImage]
    # +opts+::	    frame options [Integer]
    # +x+::	        initial x-position [Integer]
    # +y+::	        initial y-position [Integer]
    # +width+::	    initial width [Integer]
    # +height+::	  initial height [Integer]
    # +padLeft+::	  internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	  internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, img, opts=FRAME_SUNKEN|FRAME_THICK, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0) # :yields: theImageFrame
    end
  end
end
