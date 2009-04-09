module Fox
  #
  # A Color Ring widget provides an intuitive way to specify a color.
  # The outer ring of the widget is rotated to control the hue of the color
  # being specified, while the inner triangle varies the color saturation
  # and the brightness of the color.  The color saturation axis of the
  # triangle goes from a fully saturated "pure" color to "pastel" color;
  # the brightness goes from black to a bright color.
  #
  # === Events
  #
  # The following messages are sent by FXColorRing to its target:
  #
  # +SEL_CHANGED+::		sent continuously while the user is dragging the spot around; the message data is a three-element array containing the hue, saturation and value values.
  # +SEL_COMMAND+::		sent when the user releases the mouse button and "drops" the spot at its new location; the message data is a three-element array containing the hue, saturation and value values.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  class FXColorRing < FXFrame
  
    # Hue [Float]
    attr_accessor :hue
    
    # Saturation [Float]
    attr_accessor :sat
    
    # Value [Float]
    attr_accessor :val
    
    # Width of hue ring in pixels [Integer]
    attr_accessor :ringWidth
    
    # Status line help text [String]
    attr_accessor :helpText
    
    # Tool tip message [String]
    attr_accessor :tipText
    
    #
    # Return an initialized FXColorRing instance.
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theColorRing
    end
    
    # Set the hue, saturation and value (all floating point values)
    def setHueSatVal(h, s, v); end
  end
end

