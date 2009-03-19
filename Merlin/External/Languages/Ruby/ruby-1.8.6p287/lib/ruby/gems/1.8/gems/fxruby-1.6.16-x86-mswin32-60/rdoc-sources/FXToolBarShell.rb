module Fox
  #
  # An FXToolBarShell is a widget floating around over the main window.
  # It typically contains an undocked tool bar.
  #
  class FXToolBarShell < FXTopWindow
  
    # Frame style [Integer]  
    attr_accessor :frameStyle
    
    # Border width [Integer]
    attr_reader	:borderWidth
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Border color [FXColor]
    attr_accessor :borderColor
    
    # Base GUI color [FXColor]
    attr_accessor :baseColor

    #
    # Return an initialized FXToolBarShell instance.
    #
    # ==== Parameters:
    #
    # +owner+::	the owner window for this tool bar shell [FXWindow]
    # +opts+::	tool bar shell options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +hSpacing+::	horizontal spacing between widgets, in pixels [Integer]
    # +vSpacing+::	vertical spacing between widgets, in pixels [Integer]
    #
    def initialize(owner, opts=FRAME_RAISED|FRAME_THICK, x=0, y=0, width=0, height=0, hSpacing=4, vSpacing=4) # :yields: theToolBarShell
    end
  end
end

