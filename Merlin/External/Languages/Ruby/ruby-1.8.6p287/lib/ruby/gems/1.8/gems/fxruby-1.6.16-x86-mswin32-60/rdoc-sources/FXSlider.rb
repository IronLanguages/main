module Fox
  #
  # The slider widget is a valuator widget which provides simple linear value range.
  # Two visual appearances are supported:- the sunken look, which is enabled with
  # the SLIDER_INSIDE_BAR option and the regular look.  The latter may have optional
  # arrows on the slider thumb.
  #
  # === Events
  #
  # The following messages are sent by FXSlider to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::
  #   sent at the end of a slider move; the message data is the new position of the slider (an Integer).
  # +SEL_CHANGED+::
  #   sent continuously while the slider is being moved; the message data is an integer indicating
  #   the current slider position.
  #
  # === Slider control styles
  #
  # +SLIDER_HORIZONTAL+::	Slider shown horizontally
  # +SLIDER_VERTICAL+::		Slider shown vertically
  # +SLIDER_ARROW_UP+::		Slider has arrow head pointing up
  # +SLIDER_ARROW_DOWN+::	Slider has arrow head pointing down
  # +SLIDER_ARROW_LEFT+::	Slider has arrow head pointing left
  # +SLIDER_ARROW_RIGHT+::	Slider has arrow head pointing right
  # +SLIDER_INSIDE_BAR+::	Slider is inside the slot rather than overhanging
  # +SLIDER_TICKS_TOP+::	Ticks on the top of horizontal slider
  # +SLIDER_TICKS_BOTTOM+::	Ticks on the bottom of horizontal slider
  # +SLIDER_TICKS_LEFT+::	Ticks on the left of vertical slider
  # +SLIDER_TICKS_RIGHT+::	Ticks on the right of vertical slider
  # +SLIDER_NORMAL+::		same as <tt>SLIDER_HORIZONTAL</tt>
  #
  # === Message identifiers
  #
  # +ID_AUTOINC+::	x
  # +ID_AUTODEC+::	x
  #
  class FXSlider < FXFrame

    # Slider value [Integer]
    attr_accessor :value

    # Slider range [Range]
    attr_accessor :range

    # Slider style [Integer]
    attr_accessor :sliderStyle

    # Slider head size, in pixels [Integer]
    attr_accessor :headSize
    
    # Slider slot size, in pixels [Integer]
    attr_accessor :slotSize
    
    # Slider auto-increment (or decrement) value [Integer]
    attr_accessor :increment
    
    # Delta between ticks [Integer]
    attr_accessor :tickDelta
    
    # Color of the slot that the slider head moves in [FXColor]
    attr_accessor :slotColor
    
    # Status line help text for this slider [String]
    attr_accessor :helpText
    
    # Tool tip text for this slider [String]
    attr_accessor :tipText
    
    #
    # Return an initialized FXSlider instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this slider [FXComposite]
    # +target+::	the message target, if any, for this slider [FXObject]
    # +selector+::	the message identifier for this slider [Integer]
    # +opts+::	slider options [Integer]
    # +x+::	initial x-position, when the +LAYOUT_FIX_X+ layout hint is in effect [Integer]
    # +y+::	initial y-position, when the +LAYOUT_FIX_Y+ layout hint is in effect [Integer]
    # +width+::	initial width, when the +LAYOUT_FIX_WIDTH+ layout hint is in effect [Integer]
    # +height+::	initial height, when the +LAYOUT_FIX_HEIGHT+ layout hint is in effect [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=SLIDER_NORMAL, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0) # :yields: theSlider
    end
  end
end

