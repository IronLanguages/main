module Fox
  #
  # A Color Bar is a widget which controls the brightness (value) of a
  # color by means of the hue, saturation, value specification system.
  # It is most useful when used together with the Color Wheel which controls
  # the hue and saturation.
  # The options <tt>COLORBAR_HORIZONTAL</tt> and <tt>COLORBAR_VERTICAL</tt> control the orientation
  # of the bar.
  #
  # === Events
  #
  # The following messages are sent by FXColorBar to its target:
  #
  # +SEL_CHANGED+::		sent continuously while the user is dragging the spot around; the message data is a three-element array containing the hue, saturation and value values.
  # +SEL_COMMAND+::		sent when the user releases the mouse button and "drops" the spot at its new location; the message data is a three-element array containing the hue, saturation and value values.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  # === Color bar orientation
  #
  # +COLORBAR_HORIZONTAL+::	Color bar shown horizontally
  # +COLORBAR_VERTICAL+::	Color bar shown vertically

  class FXColorBar < FXFrame

    # Hue [Float]
    attr_accessor :hue
    
    # Saturation [Float]
    attr_accessor :sat
    
    # Value [Float]
    attr_accessor :val
    
    # Color bar style (one of +COLORBAR_HORIZONTAL+ or +COLORBAR_VERTICAL+) [Integer]
    attr_accessor	:barStyle
    
    # Status line help text [String]
    attr_accessor	:helpText
    
    # Tool tip message [String]
    attr_accessor	:tipText

    # Construct color bar
    def initialize(parent, target=nil, selector=0, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theColorBar
    end
  end
end
