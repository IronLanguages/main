module Fox
  #
  # Seven-segment (eg LCD/watch style) widget, useful for making
  # indicators and timers.  Besides numbers, the seven-segment
  # display widget can also display some letters and punctuations.
  #
  # === 7 Segment styles
  #
  # +SEVENSEGMENT_NORMAL+::	Draw segments normally
  # +SEVENSEGMENT_SHADOW+::	Draw shadow under the segments
  #
  class FX7Segment < FXFrame
    # The text for this label [String]
    attr_accessor :text
    
    # The text color [FXColor]
    attr_accessor :textColor
    
    # Cell width, in pixels [Integer]
    attr_accessor :cellWidth
    
    # Cell height, in pixels [Integer]
    attr_accessor :cellHeight
    
    # Segment thickness, in pixels [Integer]
    attr_accessor :thickness
    
    # Current text-justification mode [Integer]
    attr_accessor :justify
    
    # Status line help text [String]
    attr_accessor :helpText
    
    # Tool tip message [String]
    attr_accessor :tipText
    
    # Create a seven segment display
    def initialize(p, text, opts=SEVENSEGMENT_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: the7Segment
    end
  
    #
    # Change 7 segment style, where _style_ is either +SEVENSEGMENT_NORMAL+ or
    # +SEVENSEGMENT_SHADOW+.
    #
    def set7SegmentStyle(style); end

    #
    # Return the current 7 segment style, which is either +SEVENSEGMENT_NORMAL+
    # or +SEVENSEGMENT_SHADOW+.
    #
    def get7SegmentStyle(); end
  end
end

