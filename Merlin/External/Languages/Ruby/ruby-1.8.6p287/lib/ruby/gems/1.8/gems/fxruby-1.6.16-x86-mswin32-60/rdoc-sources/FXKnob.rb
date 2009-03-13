module Fox
  #
  # The knob widget is a valuator widget which provides simple linear value range.
  # While being moved, the knob sends +SEL_CHANGED+ messages to its target;
  # at the end of the interaction, a final +SEL_COMMAND+ message is sent.
  # The message data represents the current knob value (an integer).
  #
  # === Events
  #
  # The following messages are sent by FXKnob to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::
  #   sent at the end of a knob move; the message data is the new value of the knob.
  # +SEL_CHANGED+::
  #   sent continuously while the knob is being moved; the message data is an integer indicating
  #   the current knob value.
  #
  # === Knob Control styles
  #
  # +KNOB_NEEDLE+::	Use a needle as indicator
  # +KNOB_DOT+::	Use a dot as indicator
  # +KNOB_TICKS+::	Show ticks around the knob
  # +KNOB_INDICATOR+::	Show only the indicator (like a speedometer)
  # +KNOB_NORMAL+::	Normal knob looks
  #
  class FXKnob < FXFrame

    # Knob value [Integer]
    attr_accessor :value
    
    # Knob range [Range]
    attr_accessor :range
    
    # Knob style [Integer]
    attr_accessor :knobStyle
    
    # Knob auto-increment/decrement value [Integer]
    attr_accessor :increment
    
    # Delta between ticks [Integer]
    attr_accessor :tickDelta
    
    # Indicator needle color [FXColor]
    attr_accessor :lineColor
    
    # Help text displayed on the status line [String]
    attr_accessor :helpText
    
    # Tooltip text value [String]
    attr_accessor :tipText
    
    #
    # Return a new FXKnob instance.
    #
    def initialize(p, target=nil, selector=0, opts=KNOB_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theKnob
    end

    # 
    # Change the knob's movement limits (start and ending angles)
    # Accept values in degrees from 0 (south) to 360.
    #
    def setLimits(start_angle, end_angle, notify=false); end

    #
    # Return the knob's current limits as a two-element array.
    #
    def getLimits(); end
  end
end

