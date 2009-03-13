module Fox
  #
  # The Ruler View provides viewing of a document with rulers.
  # It is intended to be subclassed in order to draw actual contents
  # and provide editing behavior for the document.
  # The ruler view itself simply manages the geometry of the document
  # being edited, and coordinates the movement of the ruler displays
  # as the document is being scrolled.
  #
  class FXRulerView < FXScrollArea

    # Return a reference to the horizontal ruler [FXRuler]
    attr_reader :horizontalRuler

    # Return a reference to the vertical ruler [FXRuler]
    attr_reader :verticalRuler

    # Get document position X [Integer]
    attr_reader :documentX

    # Get document position Y [Integer]
    attr_reader :documentY

    # Current document color [FXColor]
    attr_accessor :documentColor

    # X arrow position, relative to document position [Integer]
    attr_accessor :arrowPosX

    # Y arrow position in document, relative to document position [Integer]
    attr_accessor :arrowPosY

    # Horizontal ruler style[Integer]
    attr_accessor :hRulerStyle

    # Vertical ruler style [Integer]
    attr_accessor :vRulerStyle

    # Status line help text [String]
    attr_accessor :helpText

    # Tool tip message [String]
    attr_accessor :tipText
    
    # Document width [Integer]
    attr_accessor :documentWidth
    
    # Document height [Integer]
    attr_accessor :documentHeight
    
    # Horizontal edge spacing around document [Integer]
    attr_accessor :hEdgeSpacing
    
    # Vertical edge spacing around document [Integer]
    attr_accessor :vEdgeSpacing
    
    # Horizontal lower margin [Integer]
    attr_accessor :hMarginLower
    
    # Horizontal upper margin [Integer]
    attr_accessor :hMarginUpper
    
    # Vertical lower margin [Integer]
    attr_accessor :vMarginLower
    
    # Vertical upper margin [Integer]
    attr_accessor :vMarginUpper
    
    # Horizontal alignment; the default is +RULER_ALIGN_NORMAL+ [Integer]
    attr_accessor :hAlignment
    
    # Vertical alignment; the default is +RULER_ALIGN_NORMAL+ [Integer]
    attr_accessor :vAlignment

    # Horizontal ruler font [FXFont]
    attr_accessor :hRulerFont
    
    # Vertical ruler font [FXFont]
    attr_accessor :vRulerFont
    
    # Horizontal document number placement [Integer]
    attr_accessor :hNumberTicks
    
    # Vertical document number placement [Integer]
    attr_accessor :vNumberTicks
    
    # Horizontal major ticks [Integer]
    attr_accessor :hMajorTicks
    
    # Vertical major ticks [Integer]
    attr_accessor :vMajorTicks

    # Horizontal medium ticks [Integer]
    attr_accessor :hMediumTicks
    
    # Vertical medium ticks [Integer]
    attr_accessor :vMediumTicks

    # Horizontal tiny ticks [Integer]
    attr_accessor :hTinyTicks
    
    # Vertical tiny ticks [Integer]
    attr_accessor :vTinyTicks
    
    # Horizontal pixels per tick spacing [Float]
    attr_accessor :hPixelsPerTick
    
    # Vertical pixels per tick spacing [Float]
    attr_accessor :vPixelsPerTick

    #
    # Return an initialized FXRulerView instance.
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theRulerView
    end
 
    # Set document width (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the document size is changed.
    def setDocumentWidth(w, notify=false); end
    
    # Set document height (in pixels).
    def setDocumentHeight(h, notify=false); end
    
    # Set horizontal edge spacing around document (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the edge spacing is changed.
    def setHEdgeSpacing(es, notify=false); end
    
    # Set vertical edge spacing around document (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the edge spacing is changed.
    def setVEdgeSpacing(es, notify=false); end
    
    # Set horizontal lower margin (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the margin is changed.
    def setHMarginLower(marg, notify=false); end
    
    # Set horizontal upper margin (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the margin is changed.
    def setHMarginUpper(marg, notify=false); end
    
    # Set vertical lower margin (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the margin is changed.
    def setVMarginLower(marg, notify=false); end
    
    # Set vertical upper margin (in pixels).
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the margin is changed.
    def setVMarginUpper(marg, notify=false); end
    
    # Set horizontal alignment; the default is +RULER_ALIGN_NORMAL+.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the alignment is changed.
    def setHAlignment(align, notify=false); end
    
    # Set vertical alignment; the default is +RULER_ALIGN_NORMAL+.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the alignment is changed.
    def setVAlignment(align, notify=false); end

    # Set horizontal ruler font.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the font is changed.
    def setHRulerFont(font, notify=false); end
    
    # Set vertical ruler font.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the font is changed.
    def setVRulerFont(font, notify=false); end
    
    # Set number of horizontal "number" ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the number of ticks is changed.
    def setHNumberTicks(ticks, notify=false); end
    
    # Set number of vertical "number" ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the number of ticks is changed.
    def setVNumberTicks(ticks, notify=false); end
    
    # Set number of horizontal major ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the number of ticks is changed.
    def setHMajorTicks(ticks, notify=false); end
    
    # Set number of vertical major ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the number of ticks is changed.
    def setVMajorTicks(ticks, notify=false); end

    # Set number of horizontal medium ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the number of ticks is changed.
    def setHMediumTicks(ticks, notify=false); end
    
    # Set number of vertical medium ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the number of ticks is changed.
    def setVMediumTicks(ticks, notify=false); end

    # Set number of horizontal tiny ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the number of ticks is changed.
    def setHTinyTicks(ticks, notify=false); end
    
    # Set number of vertical tiny ticks.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the number of ticks is changed.
    def setVTinyTicks(ticks, notify=false); end
    
    # Set horizontal pixels per tick spacing
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the horizontal
    # ruler's target after the spacing is changed.
    def setHPixelsPerTick(space, notify=false); end
    
    # Set vertical pixels per tick spacing
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the vertical
    # ruler's target after the spacing is changed.
    def setVPixelsPerTick(space, notify=false); end
  end
end
