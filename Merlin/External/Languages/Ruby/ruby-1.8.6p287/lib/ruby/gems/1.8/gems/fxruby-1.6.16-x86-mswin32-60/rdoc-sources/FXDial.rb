module Fox
  #
  # The Dial widget is a valuator widget which is able to provide a cyclic
  # value range when the <tt>DIAL_CYCLIC</tt> is passed, or a simple linear value range.
  # While being turned, the dial sends a <tt>SEL_CHANGED</tt> message to its target;
  # at the end of the interaction, a <tt>SEL_COMMAND</tt> message is sent.
  # The message data represents the current value (an integer). The options
  # <tt>DIAL_VERTICAL</tt> and <tt>DIAL_HORIZONTAL</tt> control the orientation of the dial.
  # An optional notch can be used to indicate the zero-position of
  # the dial; display of the notch is controlled by the <tt>DIAL_HAS_NOTCH</tt> option.
  #
  # === Events
  #
  # The following messages are sent by FXDial to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_CHANGED+::             sent when the dial's value changes; the message data is the new value (an integer).
  # +SEL_COMMAND+::		sent when the user stops changing the dial's value and releases the mouse button; the message data is the new value (an integer).
  #
  # === Dial style options
  #
  # +DIAL_VERTICAL+::     Vertically oriented
  # +DIAL_HORIZONTAL+::   Horizontal oriented
  # +DIAL_CYCLIC+::       Value wraps around
  # +DIAL_HAS_NOTCH+::    Dial has a Center Notch
  # +DIAL_NORMAL+::       same a +DIAL_VERTICAL+
  #
  class FXDial < FXFrame

    # Dial value [Integer]
    attr_accessor :value
    
    # Dial range [Range]
    attr_accessor :range

    #
    # The revolution increment is the amount of change in the position
    # for revolution of the dial; the dial may go through multiple revolutions
    # to go through its whole range. By default it takes one 360 degree turn of
    # the dial to go from the lower to the upper range. [Integer]
    #
    attr_accessor :revolutionIncrement

    #
    # The spacing for the small notches; this should be set 
    # in tenths of degrees in the range [1,3600], and the value should
    # be a divisor of 3600, so as to make the notches come out evenly. [Integer]
    #
    attr_accessor :notchSpacing

    #
    # The notch offset is the position of the center notch; the value should
    # be tenths of degrees in the range [-3600,3600]. [Integer]
    #
    attr_accessor :notchOffset
    
    # Current dial style [Integer]
    attr_accessor :dialStyle
    
    # Center notch color [FXColor]
    attr_accessor :notchColor
    
    # Status line help text for this dial [String]
    attr_accessor :helpText
    
    # Tool tip message for this dial
    attr_accessor :tipText

    #
    # Construct a dial widget
    #
    # ==== Parameters:
    #
    # +p+::	parent widget for this dial [FXComposite]
    # +target+::	message target object for this dial [FXObject]
    # +selector+::	message identifier [Integer]
    # +opts+::
    # +x+::
    # +y+::
    # +width+::
    # +height+::
    # +padLeft+::
    # +padRight+::
    # +padTop+::
    # +padBottom+::
    #
    def initialize(p, target=nil, selector=0, opts=DIAL_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theDial
    end
    
    #
    # Set the dial value. If _notify_ is +true+, a +SEL_COMMAND+ message is
    # sent to the dial's message target after the value is changed.
    #
    def setValue(value, notify=false); end
    
    #
    # Set the dial's range. If _notify_ is +true+, and the range modification
    # causes the dial's value to change, a +SEL_COMMAND+ message is sent
    # to the dial's message target after the value is changed.
    #
    def setRange(range, notify=false); end
  end
end

