module Fox
  #
  # The menu separator is a simple decorative groove
  # used to delineate items in a popup menu.
  #
  class FXMenuSeparator < FXWindow

    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor

    #
    # Construct a menu separator
    #
    def initialize(parent, opts=0) # :yields: theMenuSeparator
    end
  end
end

