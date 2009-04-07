module Fox
  #
  # FXColorDialog is a standard dialog panel used to edit colors.
  # Colors can be edited via RGB (Red, Green, Blue additive color model),
  # via HSV (Hue, Saturation, Value color modal), via CMY (Cyan, Magenta,
  # Yellow subtractive color model), or by name.
  # Commonly used colors can be dragged into a number of small color wells
  # to be used repeatedly; colors dropped into the small color wells are
  # automatically saved into the registry for future use.
  #
  # === Events
  #
  # The following messages are sent by FXColorDialog to its target:
  #
  # +SEL_CHANGED+::	sent continuously, while the color selector's color is changing
  # +SEL_COMMAND+::	sent when the new color is set
  #
  # === Message identifiers
  #
  # +ID_COLORSELECTOR+::	used internally to identify messages from the FXColorSelector

  class FXColorDialog < FXDialogBox

    # The color [FXColor]
    attr_accessor :rgba
   
    # Only opaque colors allowed [Boolean]
    attr_writer :opaqueOnly

    # Construct color dialog
    def initialize(owner, title, opts=0, x=0, y=0, width=0, height=0) # :yields: theColorDialog
    end
  
    # Return +true+ if only opaque colors allowed
    def opaqueOnly?() ; end
  end
end
