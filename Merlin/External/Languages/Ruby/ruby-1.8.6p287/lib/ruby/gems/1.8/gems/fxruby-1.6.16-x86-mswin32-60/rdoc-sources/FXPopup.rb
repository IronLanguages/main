module Fox
  #
  # Popup window
  #
  # === Popup internal orientation
  #
  # +POPUP_VERTICAL+::          Vertical orientation
  # +POPUP_HORIZONTAL+::        Horizontal orientation
  # +POPUP_SHRINKWRAP+::        Shrinkwrap to content
  #
  class FXPopup < FXShell
  
    # Frame style [Integer]
    attr_accessor :frameStyle
    
    # Border width [Integer]
    attr_reader :borderWidth
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Border color [FXColor]
    attr_accessor :borderColor

    # Base color [FXColor]
    attr_accessor :baseColor
    
    # Current grab owner [FXWindow]
    attr_reader :grabOwner
    
    # Popup orientation [Integer]
    attr_accessor :orientation
    
    # Shrinkwrap mode [Boolean]
    attr_accessor :shrinkWrap
    
    #
    # Construct popup pane
    #
    def initialize(owner, opts=POPUP_VERTICAL|FRAME_RAISED|FRAME_THICK, x=0, y=0, width=0, height=0) # :yields: thePopup
    end
  
    # Pop it up
    def popup(grabto, x, y, width=0, height=0); end
    
    # Pop it down
    def popdown(); end
  end
end

