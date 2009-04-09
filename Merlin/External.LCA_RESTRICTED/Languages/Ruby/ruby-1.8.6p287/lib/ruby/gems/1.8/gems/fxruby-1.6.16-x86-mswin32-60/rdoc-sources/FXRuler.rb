module Fox
  #
  # The ruler widget is placed alongside a document to measure position
  # and size of entities within the document, such as margins, paragraph
  # indents, and tickmarks.
  # The ruler widget sends a +SEL_CHANGED+ message when the indentation or margins
  # are interactively changed by the user.
  # If the document size exceeds the available space, it is possible to
  # scroll the document using setPosition().  When the document size is
  # less than the available space, the alignment options can be used to
  # center, left-adjust, or right-adjust the document.
  # Finally, a special option exists to stretch the document to the
  # available space, that is to say, the document will always be fitten
  # with given left and right edges substracted from the available space.
  #
  # === Events
  #
  # The following messages are sent by FXRuler to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_CHANGED+::		sent whenever something about the ruler changes
  #
  # === Ruler options
  #
  # +RULER_NORMAL+::		Default appearance (default)
  # +RULER_HORIZONTAL+::	Ruler is horizontal (default)
  # +RULER_VERTICAL+::		Ruler is vertical
  # +RULER_TICKS_OFF+::		Tick marks off (default)
  # +RULER_TICKS_TOP+::		Ticks on the top (if horizontal)
  # +RULER_TICKS_BOTTOM+::	Ticks on the bottom (if horizontal)
  # +RULER_TICKS_LEFT+::	Ticks on the left (if vertical)
  # +RULER_TICKS_RIGHT+::	Ticks on the right (if vertical)
  # +RULER_TICKS_CENTER+::	Tickmarks centered
  # +RULER_NUMBERS+::		Show numbers
  # +RULER_ARROW+::		Draw small arrow for cursor position
  # +RULER_MARKERS+::		Draw markers for indentation settings
  # +RULER_METRIC+::		Metric subdivision (default)
  # +RULER_ENGLISH+::		English subdivision
  # +RULER_MARGIN_ADJUST+::	Allow margin adjustment
  # +RULER_ALIGN_CENTER+::	Center document horizontally
  # +RULER_ALIGN_LEFT+::	Align document to the left
  # +RULER_ALIGN_RIGHT+::	Align document to the right
  # +RULER_ALIGN_TOP+::		Align document to the top
  # +RULER_ALIGN_BOTTOM+::	Align document to the bottom
  # +RULER_ALIGN_STRETCH+::	Stretch document to fit horizontally
  # +RULER_ALIGN_NORMAL+::	Normally, document is centered both ways
  #
  # === Message identifiers:
  #
  # +ID_ARROW+::		write me
  #
  class FXRuler < FXFrame
  
    # Current position [Integer]
    attr_accessor :position
    
    # Content size [Integer]
    attr_accessor :contentSize
    
    # Document size [Integer]
    attr_accessor :documentSize
  
    # Document size [Integer]
    attr_accessor :edgeSpacing
    
    # Lower document margin [Integer]
    attr_accessor :marginLower

    # Upper document margin [Integer]
    attr_accessor :marginUpper
    
    # First line indent [Integer]
    attr_accessor :indentFirst
    
    # Lower indent [Integer]
    attr_accessor :indentLower
    
    # Upper indent [Integer]
    attr_accessor :indentUpper
    
    # Document number placement [Integer]
    attr_accessor :numberTicks

    # Document major ticks [Integer]
    attr_accessor :majorTicks

    # Document minor ticks [Integer]
    attr_accessor :minorTicks

    # Document tiny ticks [Integer]
    attr_accessor :tinyTicks

    # Pixels per tick spacing [Float]
    attr_accessor :pixelsPerTick
    
    # The text font [FXFont]
    attr_accessor :font
    
    # The slider value [Integer]
    attr_accessor :value
    
    # The ruler style [Integer]
    attr_accessor :rulerStyle
    
    # Ruler alignment [Integer]
    attr_accessor :rulerAlignment

    # The current text color [FXColor]
    attr_accessor :textColor
    
    # The status line help text for this ruler [String]
    attr_accessor :helpText
    
    # The tool tip message for this ruler [String]
    attr_accessor :tipText
    
    #
    # Return an initialized FXRuler instance.
    #
    def initialize(p, target=nil, selector=0, opts=RULER_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theRuler
    end
    
    # Return lower edge of document (an integer)
    def documentLower; end
    
    # Return upper edge of document (an integer)
    def documentUpper; end
  end
end
