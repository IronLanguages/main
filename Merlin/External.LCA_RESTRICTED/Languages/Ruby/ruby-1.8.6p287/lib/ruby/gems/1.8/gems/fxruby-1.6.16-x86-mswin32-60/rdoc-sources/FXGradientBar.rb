module Fox
  #
  # An FXGradientBar widget is defined in part by its array of "segments",
  # each of which is an FXGradient instance. An FXGradient instance defines
  # the properties of one segment, namely, the lower, middle and upper
  # values (all Floats); the lower and upper color values; and the blending
  # mode for the segment.
  #
  class FXGradient
    # Lower value [Float]
    attr_accessor :lower
    
    # Middle value [Float]
    attr_accessor :middle
    
    # Upper value [Float]
    attr_accessor :upper
    
    # Lower color [FXColor]
    attr_accessor :lowerColor
    
    # Upper color [FXColor]
    attr_accessor :upperColor
    
    # Blend mode [Integer]
    attr_accessor :blend
  end

  #
  # The FXGradientBar is a control that is used to edit color gradient,
  # such as used in texture mapping and shape filling.
  #
  # === Events
  #
  # The following messages are sent by FXGradientBar to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_CHANGED+::		sent when anything about a segment changes; the message data is an integer indicating the segment number
  # +SEL_SELECTED+::		sent when one or more segments are selected.
  # +SEL_DESELECTED+::		sent when one or more segments are deselected.
  #
  # === Gradient bar orientation
  #
  # +GRADIENTBAR_HORIZONTAL+::		Gradient bar shown horizontally
  # +GRADIENTBAR_VERTICAL+::		Gradient bar shown vertically
  # +GRADIENTBAR_NO_CONTROLS+::		No controls shown
  # +GRADIENTBAR_CONTROLS_TOP+::	Controls on top
  # +GRADIENTBAR_CONTROLS_BOTTOM+::	Controls on bottom
  # +GRADIENTBAR_CONTROLS_LEFT+::	Controls on left
  # +GRADIENTBAR_CONTROLS_RIGHT+::	Controls on right
  #
  # === Blend modes
  #
  # +GRADIENT_BLEND_LINEAR+::		Linear blend
  # +GRADIENT_BLEND_POWER+::		Power law blend
  # +GRADIENT_BLEND_SINE+::		Sine blend
  # +GRADIENT_BLEND_INCREASING+::	Quadratic increasing blend
  # +GRADIENT_BLEND_DECREASING+::	Quadratic decreasing blend
  #
  # === Message identifiers
  #
  # +ID_LOWER_COLOR+::			write me
  # +ID_UPPER_COLOR+::			write me
  # +ID_BLEND_LINEAR+::			write me
  # +ID_BLEND_POWER+::			write me
  # +ID_BLEND_SINE+::			write me
  # +ID_BLEND_INCREASING+::		write me
  # +ID_BLEND_DECREASING+::		write me
  # +ID_RECENTER+::			write me
  # +ID_SPLIT+::			write me
  # +ID_MERGE+::		 	write me
  # +ID_UNIFORM+::			write me
  #
  class FXGradientBar < FXFrame
  
    #
    # Gradient bar style, some combination of +GRADIENTBAR_HORIZONTAL+,
    # +GRADIENTBAR_VERTICAL+, +GRADIENTBAR_NO_CONTROLS+,
    # +GRADIENTBAR_CONTROLS_TOP+, +GRADIENTBAR_CONTROLS_BOTTOM+,
    # +GRADIENTBAR_CONTROLS_LEFT+ and +GRADIENTBAR_CONTROLS_RIGHT+.
    #
    attr_accessor :barStyle
    
    # Selection color [FXColor]
    attr_accessor :selectColor
    
    # Status line help text [String]
    attr_accessor :helpText
    
    # Tool tip text [String]
    attr_accessor :tipText

    #
    # Return an initialized FXGradientBar instance.
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theGradientBar
    end
    
    #
    # Return the zero-based index of the segment containing location (_x_, _y_).
    # Returns -1 if no matching segment was found.
    #
    def getSegment(x, y); end

    #
    # Return the grip in segment _seg_ which is closest to location (_x_, _y_),
    # one of +GRIP_LOWER+, +GRIP_SEG_LOWER+, +GRIP_MIDDLE+, +GRIP_SEG_UPPER+,
    # +GRIP_UPPER+ or +GRIP_NONE+.
    #
    def getGrip(seg, x, y); end

    # Return the number of segments
    def numSegments(); end

    #
    # Replace the current gradient segments with _segments_, an array of
    # FXGradient instances.
    #
    def gradients=(segments); end

    #
    # Return a reference to the array of gradient segments (an array of
    # FXGradient instances).
    #
    def gradients(); end

    #
    # Change current segment to _index_. Use an _index_ of -1 to indicate that there
    # is no current segment.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the current segment is changed.
    # Raises IndexError if _index_ is out of bounds.
    # 
    def setCurrentSegment(index, notify=false); end

    #
    # Return the index of the current segment, or -1 if there is no current segment.
    #
    def getCurrentSegment(); end

    #
    # Change anchor segment to _seg_.
    # Use a _seg_ value of -1 to indicate that there is no anchor segment.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def anchorSegment=(seg); end

    #
    # Return the index of the anchor segment, or -1 if there is no anchor segment.
    #
    def anchorSegment(); end

    #
    # Select segment(s) _fm_ through _to_ and return +true+ if the selected range
    # is different than it was.
    # If _notify_ is +true+, a +SEL_SELECTED+ message is sent to the gradient bar's
    # message target after the current segment is changed.
    # Raises ArgumentError if _fm_ is greater than _to_, and
    # IndexError if either _fm_ or _to_ is out of bounds.
    #
    def selectSegments(fm, to, notify=false); end

    #
    # Deselect all segments, and return +true+ if there was a previously
    # selected range.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the gradient bar's
    # message target after the current selection is deselected.
    #
    def deselectSegments(notify); end

    #
    # Return +true+ if the specified segment is selected.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def segmentSelected?(seg); end

    #
    # Set lower color of the segment with index _seg_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segment's lower color is changed.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def setSegmentLowerColor(seg, clr, notify=false); end
  
    #
    # Set upper color of the segment with index _seg_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segment's upper color is changed.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def setSegmentUpperColor(seg, clr, notify=false); end

    #
    # Return lower color of the segment with index _seg_.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentLowerColor(seg); end
  
    #
    # Return upper color of the segment with index _seg_.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentUpperColor(seg); end

    #
    # Move lower point of segment _seg_ to _val_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segment's lower value is changed.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def moveSegmentLower(seg, val, notify=false); end

    #
    # Move middle point of segment _seg_ to _val_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segment's middle value is changed.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def moveSegmentMiddle(seg, val, notify=false); end

    #
    # Move upper point of segment _seg_ to _val_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segment's upper value is changed.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def moveSegmentUpper(seg, val, notify=false); end

    #
    # Move segments _sglo_ to _sghi_ to new position _val_.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after the segments' values are changed.
    #
    def moveSegments(sglo, sghi, val, notify=false); end

    #
    # Return lower value of segment _seg_.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentLower(seg); end
  
    #
    # Return middle value of segment _seg_.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentMiddle(seg); end
  
    #
    # Return upper value of segment _seg_.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentUpper(seg); end

    #
    # Return a gradient ramp of size _nramp_ based on the settings for this
    # gradient bar. The return value is an array of color values corresponding
    # to this gradient bar.
    #
    def gradient(nramp); end

    #
    # Return the blend mode of segment _seg_, one of +GRADIENT_BLEND_LINEAR+,
    # +GRADIENT_BLEND_POWER+, +GRADIENT_BLEND_SINE+, +GRADIENT_BLEND_INCREASING+
    # or +GRADIENT_BLEND_DECREASING+.
    # Raises IndexError if _seg_ is out of bounds.
    #
    def getSegmentBlend(seg); end

    #
    # Split segment at the midpoint
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after this change is completed.
    #
    def splitSegments(sglo, sghi, notify=false); end

    #
    # Merge segments.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after this change is completed.
    #
    def mergeSegments(sglo, sghi, notify=false); end

    #
    # Make segments uniformly distributed.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after this change is completed.
    #
    def uniformSegments(sglo, sghi, notify=false); end

    #
    # Set the blend mode for segments _sglo_ through _sghi_ to _blend_, where
    # _blend_ is one of +GRADIENT_BLEND_LINEAR+,
    # +GRADIENT_BLEND_POWER+, +GRADIENT_BLEND_SINE+, +GRADIENT_BLEND_INCREASING+
    # or +GRADIENT_BLEND_DECREASING+.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the gradient bar's
    # message target after this change is completed.
    #
    def blendSegments(sglo, sghi, blend=GRADIENT_BLEND_LINEAR, notify=false); end
  end
end
