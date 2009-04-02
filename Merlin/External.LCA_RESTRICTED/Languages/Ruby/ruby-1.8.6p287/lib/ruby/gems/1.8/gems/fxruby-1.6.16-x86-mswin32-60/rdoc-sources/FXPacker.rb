module Fox
  #
  # FXPacker is a layout manager which automatically places child windows
  # inside its area against the left, right, top, or bottom side.
  # Each time a child is placed, the remaining space is decreased by the
  # amount of space taken by the child window.
  # The side against which a child is placed is determined by the +LAYOUT_SIDE_TOP+,
  # +LAYOUT_SIDE_BOTTOM+, +LAYOUT_SIDE_LEFT+, and +LAYOUT_SIDE_RIGHT+ hints given by
  # the child window.  Other layout hints from the child are observed as far as
  # sensible.  So for example, a child placed against the right edge can still
  # have +LAYOUT_FILL_Y+ or +LAYOUT_TOP+, and so on.
  # The last child may have both +LAYOUT_FILL_X+ and +LAYOUT_FILL_Y+, in which
  # case it will be placed to take all remaining space.
  #
  class FXPacker < FXComposite
  
    # Current frame style [Integer]
    attr_accessor :frameStyle
    
    # Packing hints [Integer]
    attr_accessor :packingHints
    
    # Border width, in pixels [Integer]
    attr_reader :borderWidth
    
    # Top padding, in pixels [Integer]
    attr_accessor :padTop
    
    # Bottom padding, in pixels [Integer]
    attr_accessor :padBottom
    
    # Left padding, in pixels [Integer]
    attr_accessor :padLeft
    
    # Right padding, in pixels [Integer]
    attr_accessor :padRight
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Border color [FXColor]
    attr_accessor :borderColor
    
    # Base GUI color [FXColor]
    attr_accessor :baseColor
    
    # Horizontal inter-child spacing, in pixels [Integer]
    attr_accessor :hSpacing
    
    # Vertical inter-child spacing, in pixels [Integer]
    attr_accessor :vSpacing

    #
    # Construct a packer layout manager
    #
    def initialize(parent, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: thePacker
    end
  end
end

