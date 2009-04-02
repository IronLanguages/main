module Fox
  # Color selection widget
  #
  # === Events
  #
  # The following messages are sent by FXColorSelector to its target:
  #
  # +SEL_CHANGED+::	sent continuously, while the color is changing
  # +SEL_COMMAND+::	sent when the new color is set
  #
  # === Message identifiers
  #
  # +ID_CUSTOM_FIRST+::			x
  # +ID_CUSTOM_LAST+::			x
  # +ID_RGB_RED_SLIDER+::		x
  # +ID_RGB_GREEN_SLIDER+::		x
  # +ID_RGB_BLUE_SLIDER+::		x
  # +ID_RGB_RED_TEXT+::			x
  # +ID_RGB_GREEN_TEXT+::		x
  # +ID_RGB_BLUE_TEXT+::		x
  # +ID_HSV_HUE_SLIDER+::		x
  # +ID_HSV_SATURATION_SLIDER+::	x
  # +ID_HSV_VALUE_SLIDER+::		x
  # +ID_HSV_HUE_TEXT+::			x
  # +ID_HSV_SATURATION_TEXT+::		x
  # +ID_HSV_VALUE_TEXT+::		x
  # +ID_CMY_CYAN_SLIDER+::		x
  # +ID_CMY_MAGENTA_SLIDER+::		x
  # +ID_CMY_YELLOW_SLIDER+::		x
  # +ID_CMY_CYAN_TEXT+::		x
  # +ID_CMY_MAGENTA_TEXT+::		x
  # +ID_CMY_YELLOW_TEXT+::		x
  # +ID_DIAL_WHEEL+::			x
  # +ID_COLOR_BAR+::			x
  # +ID_COLOR_LIST+::			x
  # +ID_WELL_CHANGED+::			x
  # +ID_COLOR+::			x
  # +ID_ACTIVEPANE+::			x
  # +ID_ALPHA_SLIDER+::			x
  # +ID_ALPHA_TEXT+::			x
  # +ID_ALPHA_LABEL+::			x
  # +ID_COLORPICK+::			x
  
  class FXColorSelector < FXPacker

    # The "Accept" button [FXButton]
    attr_reader	:acceptButton
    
    # The "Cancel" button [FXButton]
    attr_reader	:cancelButton
  
    # The color [FXColor]
    attr_accessor :rgba
    
    # Only opaque colors allowed [Boolean]
    attr_writer	:opaqueOnly

    # Construct a new color selector
    def initialize(parent, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theColorSelector
    end

    # Return +true+ if only opaque colors allowed
    def opaqueOnly?() ; end
  end
end
