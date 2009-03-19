module Fox

  #
  # The four-way splitter is a layout manager which manages
  # four children like four panes in a window.
  # You can use a four-way splitter for example in a CAD program
  # where you may want to maintain three orthographic views, and
  # one oblique view of a model.
  # The four-way splitter allows interactive repartitioning of the
  # panes by means of moving the central splitter bars.
  # When the four-way splitter is itself resized, each child is
  # proportionally resized, maintaining the same split-percentage.
  #
  # === Events
  #
  # The following messages are sent by FX4Splitter to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::		sent at the end of a resize operation, to signal that the resize is complete
  # +SEL_CHANGED+::		sent continuously while a resize operation is occurring
  #
  # === Splitter options
  #
  # +FOURSPLITTER_TRACKING+::   Track continuously during split
  # +FOURSPLITTER_NORMAL+::     Normal mode (no continuous tracking)
  #
  # === Expansion options
  #
  # +ExpandNone+::		None expanded
  # +ExpandTopLeft+::		Expand top left child
  # +ExpandTopRight+::		Expand top right child
  # +ExpandBottomLeft+::	Expand bottom left child
  # +ExpandBottomRight+::	Expand bottom right child
  # +ExpandTop+::		Expand top children
  # +ExpandBottom+::		Expand bottom children
  # +ExpandLeft+::		Expand left children
  # +ExpandRight+::		Expand right children
  # +ExpandAll+::		Expand all children
  #
  # === Message identifiers
  #
  # +ID_EXPAND_NONE+::		None expanded
  # +ID_EXPAND_TOP+::		Expand top left and top right panes
  # +ID_EXPAND_BOTTOM+::	Expand bottom left and bottom right panes
  # +ID_EXPAND_LEFT+::		Expand leftmost panes
  # +ID_EXPAND_RIGHT+::		Expand rightmost panes
  # +ID_EXPAND_TOPLEFT+::	Expand top left pane
  # +ID_EXPAND_TOPRIGHT+::	Expand top right pane
  # +ID_EXPAND_BOTTOMLEFT+::	Expand bottom left pane
  # +ID_EXPAND_BOTTOMRIGHT+::	Expand bottom right pane
  # +ID_EXPAND_ALL+::		Expand all panes
  #
  class FX4Splitter < FXComposite

    # Horizontal split fraction [Integer]
    attr_accessor :hSplit
    
    # Vertical split fraction [Integer]
    attr_accessor :vSplit
    
    # Current splitter style, either +FOURSPLITTER_TRACKING+ or +FOURSPLITTER_NORMAL+
    attr_accessor :splitterStyle
    
    # Splitter bar width, in pixels [Integer]
    attr_accessor :barSize
    
    # Currently expanded child (some combination of the expansion flags, or zero if no panes are expanded)
    attr_accessor :expanded
    
    # Top left child window, if any [FXWindow]
    attr_reader :topLeft
    
    # Top right child window, if any [FXWindow]
    attr_reader :topRight
    
    # Bottom left child window, if any [FXWindow]
    attr_reader :bottomLeft
    
    # Bottom right child window, if any [FXWindow]
    attr_reader :bottomRight

    #
    # Return an initialized FX4Splitter instance, initially shown as four unexpanded panes
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this splitter [FXComposite]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, opts=FOURSPLITTER_NORMAL, x=0, y=0, width=0, height=0) # :yields: theSplitter
    end
    
    #
    # Return an initialized FX4Splitter instance, initially shown as four unexpanded panes;
    # notifies _tgt_ about size changes.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this splitter [FXComposite]
    # +target+::	message target [FXObject]
    # +selector+::	message identifier [Integer]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, tgt, sel, opts=FOURSPLITTER_NORMAL, x=0, y=0, width=0, height=0) # :yields: theSplitter
    end

    #
    # Change horizontal split fraction. The split fraction _s_ is
    # an integer value between 0 and 10000 (inclusive), indicating
    # how much space to allocate to the leftmost panes. For example,
    # to split the panes at 35 percent, use:
    #
    #   fourSplitter.setHSplit(3500)
    #
    # or just:
    #
    #   fourSplitter.hSplit = 3500
    #
    def setHSplit(s); end

    #
    # Return the horizontal split fraction, an integer between 0 and
    # 10000 inclusive. See FX4Splitter#setHSplit for more information.
    #
    def getHSplit(); end

    #
    # Change vertical split fraction. The split fraction _s_ is
    # an integer value between 0 and 10000 (inclusive), indicating
    # how much space to allocate to the topmost panes. For example,
    # to split the panes at 35 percent, use:
    #
    #   fourSplitter.setVSplit(3500)
    #
    # or just:
    #
    #   fourSplitter.vSplit = 3500
    #
    def setVSplit(s); end

    #
    # Return the vertical split fraction, an integer between 0 and
    # 10000 inclusive. See FX4Splitter#setVSplit for more information.
    #
    def getVSplit(); end
  end
end
