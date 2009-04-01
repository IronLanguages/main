module Fox
  # Hopefully Helpful Hint message
  #
  # = Tooltip styles
  # TOOLTIP_PERMANENT:: Tooltip stays up indefinitely
  # TOOLTIP_VARIABLE::  Tooltip stays up variable time, depending on the length of the string
  # TOOLTIP_NORMAL::
  #
  # = Message identifiers
  #
  # ID_TIP_SHOW:: Show it
  # ID_TIP_HIDE:: Hide it
  #
  class FXToolTip < FXShell
    # Construct a tool tip
    def initialize(app, opts=TOOLTIP_NORMAL, x=0, y=0, width=0, height=0); end

    # Set the text for this tip
    def text=(text); end

    # Get the text for this tip
    def text() ; end

    # Set the tip text font
    def font=(font) ; end

    # Get the tip text font
    def font() ; end

    # Get the current tip text color
    def textColor() ; end

    # Set the current tip text color
    def textColor=(color); end
    
    # Return the tool tip's text
    def to_s; text; end
  end
end
