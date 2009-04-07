module Fox
  #
  # Spinner control
  #
  # === Events
  #
  # The following messages are sent by FXSpinner to its target:
  #
  # +SEL_KEYPRESS+::	sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::	sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::
  #   sent whenever the spinner's value changes; the message data is an integer
  #   indicating the new spinner value.
  # +SEL_CHANGED+::
  #   sent whenever the text in the spinner's text field changes; the message
  #   data is an integer indicating the new spinner value.
  #
  # === Spinner options
  #
  # +SPIN_NORMAL+::	Normal, non-cyclic
  # +SPIN_CYCLIC+::	Cyclic spinner
  # +SPIN_NOTEXT+::	No text visible
  # +SPIN_NOMAX+::	Spin all the way up to infinity
  # +SPIN_NOMIN+::	Spin all the way down to -infinity
  #
  # === Message identifiers
  #
  # +ID_INCREMENT+::	x
  # +ID_DECREMENT+::	x
  # +ID_ENTRY+::	x
  #
  class FXSpinner < FXPacker
    # Current value [Integer]
    attr_accessor :value

    # Spinner range (low and high values) [Range]
    attr_accessor :range

    # Text font for this spinner [FXFont]
    attr_accessor :font

    # Status line help text for this spinner [String]
    attr_accessor :helpText

    # Tool tip text for this spinner [String]
    attr_accessor :tipText
    
    # Spinner style [Integer]
    attr_accessor :spinnerStyle
    
    # Color of the "up" arrow [FXColor]
    attr_accessor :upArrowColor

    # Color of the "down" arrow [FXColor]
    attr_accessor :downArrowColor

    # Normal text color [FXColor]
    attr_accessor :textColor

    # Background color for selected text [FXColor]
    attr_accessor :selBackColor

    # Foreground color for selected text [FXColor]
    attr_accessor :selTextColor

    # Cursor color [FXColor]
    attr_accessor :cursorColor

    # Number of columns (i.e. width of spinner's text field, in terms of number of columns of 'm') [Integer]
    attr_accessor :numColumns

    #
    # Return an initialized FXSpinner instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this spinner [FXComposite]
    # +cols+::	number of columns to display in the text field [Integer]
    # +target+::	the message target, if any, for this spinner [FXObject]
    # +selector+::	the message identifier for this spinner [Integer]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, cols, target=nil, selector=0, opts=SPIN_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theSpinner
    end
  
    # Increment spinner
    def increment(notify=FALSE); end
    
    # Increment spinner by certain amount
    def incrementByAmount(amt, notify=false); end
  
    # Decrement spinner
    def decrement(notify=FALSE); end
    
    # Decrement spinner by certain amount
    def decrementByAmount(amt, notify=false); end
  
    # Return +true+ if the spinner is in cyclic mode.
    def cyclic?; end
  
    #
    # Set to cyclic mode, i.e. wrap around at maximum/minimum.
    #
    def cyclic=(cyc); end
  
    # Return +true+ if this spinner's text field is visible.
    def textVisible?; end
  
    # Set the visibility of this spinner's text field.
    def textVisible=(shown); end
  
    #
    # Change the spinner increment value, i.e. the amount by which the spinner's
    # value increases when the up arrow is clicked.
    #
    def setIncrement(inc); end

    # Get the spinner increment value.
    def getIncrement(); end

    # Set the "editability" of this spinner's text field.
    def editable=(ed); end
  
    # Return +true+ if the spinner's text field is editable.
    def editable?; end
  end
end

