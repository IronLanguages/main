module Fox
  #
  # The Frame widget provides borders around some contents. Borders may be raised, sunken,
  # thick, ridged or etched.  They can also be turned off completely.
  # In addition, a certain amount of padding may be specified between the contents of
  # the widget and the borders.  The contents may be justified inside the widget using the
  # justification options. 
  # The Frame widget is sometimes used by itself as a place holder, but most often is used
  # as a convenient base class for simple controls.
  #
  # === Constants
  #
  # +DEFAULT_PAD+::   Default padding
  #
  class FXFrame < FXWindow
  
    # Frame style [Integer]
    attr_accessor :frameStyle
    
    # Border width, in pixels [Integer]
    attr_reader	:borderWidth
    
    # Top interior padding, in pixels [Integer]
    attr_accessor :padTop
    
    # Bottom interior padding, in pixels [Integer]
    attr_accessor :padBottom
    
    # Left interior padding, in pixels [Integer]
    attr_accessor :padLeft
    
    # Right interior padding, in pixels [Integer]
    attr_accessor :padRight
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Border color [FXColor]
    attr_accessor :borderColor
    
    # Base GUI color [FXColor]
    attr_accessor :baseColor

    #
    # Construct frame window.
    #
    def initialize(parent, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theFrame
    end
  end
end
