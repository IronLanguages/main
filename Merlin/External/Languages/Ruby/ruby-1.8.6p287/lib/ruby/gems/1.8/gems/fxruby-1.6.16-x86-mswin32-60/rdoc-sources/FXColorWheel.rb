module Fox
  #
  # A Color Wheel is a widget which controls the hue and saturation values of a
  # color. It is most often used together with a Color Bar which controls the
  # brightness.
  #
  # === Events
  #
  # The following messages are sent by FXColorWheel to its target:
  #
  # +SEL_CHANGED+::		sent continuously, while the color is changing; the message data is a 3-element array of floats containing the hue, saturation and value.
  # +SEL_COMMAND+::		sent when the new color is set; the message data is a 3-element array of floats containing the hue, saturation and value.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  class FXColorWheel < FXFrame

    # Hue [Float]
    attr_accessor :hue
    
    # Saturation [Float]
    attr_accessor :sat
    
    # Value [Float]
    attr_accessor :val
  
    # Status line help text [String]  
    attr_accessor :helpText
  
    # Tool tip message [String]  
    attr_accessor :tipText

    #
    # Construct color wheel
    #
    # ==== Parameters:
    #
    # +p+::	Parent widget [FXComposite]
    # +target+::	Message target object [FXObject]
    # +selector+::	Message identifier [Integer]
    # +opts+::	Options [Integer]
    # +x+::
    # +y+::
    # +width+::
    # +height+::
    # +padLeft+::
    # +padRight+::
    # +padTop+::
    # +padBottom+::
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theColorWheel
    end

    # Set hue, saturation and value all in one shot.
    def setHueSatVal(h, s, v); end
  end
end
