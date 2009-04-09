module Fox
  #
  # A drag corner widget may be placed in the bottom right corner
  # so as to allow the window to be resized more easily.
  #
  class FXDragCorner < FXWindow
  
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Construct a drag corner
    def initialize(p) # :yields: theDragCorner
    end
  end
end

