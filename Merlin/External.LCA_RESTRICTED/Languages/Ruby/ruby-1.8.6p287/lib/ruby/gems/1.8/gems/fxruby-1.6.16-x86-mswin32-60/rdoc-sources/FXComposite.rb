module Fox
  #
  # Base composite
  #
  # === Events
  #
  # The following messages are sent from FXComposite to its target:
  #
  # +SEL_KEYPRESS+::
  #   sent when a key goes down, but only if there is no other widget with the
  #   focus (or if the focused widget doesn't handle this keypress). The message
  #   data is an FXEvent instance.
  # +SEL_KEYRELEASE+::
  #   sent when a key goes up, but only if there is no other widget with the
  #   focus (or if the focused widget doesn't handle this key release). The message
  #   data is an FXEvent instance.
  #
  class FXComposite < FXWindow
    # Constructor
    def initialize(parent, opts=0, x=0, y=0, width=0, height=0) # :yields: theComposite
    end
  
    #
    # Return the width of the widest child window.
    #
    def maxChildWidth() ; end
  
    #
    # Return the height of the tallest child window.
    #
    def maxChildHeight() ; end
  end
end

