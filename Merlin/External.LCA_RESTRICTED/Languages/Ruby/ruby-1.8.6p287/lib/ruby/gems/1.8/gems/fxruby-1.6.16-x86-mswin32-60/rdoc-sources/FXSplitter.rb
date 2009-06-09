module Fox
  #
  # A splitter window is used to interactively repartition
  # two or more subpanels.
  # Space may be subdivided horizontally (+SPLITTER_HORIZONTAL+, which
  # the default) or vertically (+SPLITTER_VERTICAL+ option).
  # When the splitter is itself resized, the right-most (or bottom-most)
  # child window will be resized unless the splitter window is _reversed_;
  # if the splitter is reversed, the left-most (or top-most) child window
  # will be resized instead.
  # Normally, children are resizable from size 0 upwards; however, if the child
  # in a horizontally-oriented splitter has +LAYOUT_FILL_X+ in combination with 
  # +LAYOUT_FIX_WIDTH+, it will not be made smaller than its default width,
  # except when the child is the last visible widget (or first when the
  # +SPLITTER_REVERSED+ option has been passed to the splitter).
  # In a vertically-oriented splitter, children with +LAYOUT_FILL_Y+ and 
  # +LAYOUT_FIX_HEIGHT+ behave analogously.
  #
  # === Events
  #
  # The following messages are sent by FXSplitter to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::		sent at the end of a resize operation, to signal that the resize is complete
  # +SEL_CHANGED+::		sent continuously while a resize operation is occurring
  #
  # === Splitter options
  #
  # +SPLITTER_HORIZONTAL+::   Split horizontally
  # +SPLITTER_VERTICAL+::     Split vertically
  # +SPLITTER_REVERSED+::     Reverse-anchored
  # +SPLITTER_TRACKING+::     Track continuous during split
  # +SPLITTER_NORMAL+::       same as +SPLITTER_HORIZONTAL+
  #
  class FXSplitter < FXComposite
  
    # Splitter style [Integer]
    attr_accessor :splitterStyle
    
    # Splitter bar size, in pixels [Integer]
    attr_accessor :barSize
    
    #
    # Return an initialized FXSplitter instance.
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
    def initialize(p, opts=SPLITTER_NORMAL, x=0, y=0, width=0, height=0) # :yields: theSplitter
    end

    #
    # Return an initialized FXSplitter instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this splitter [FXComposite]
    # +target+::	the message target for this splitter [FXObject]
    # +selector+::	the message identifier for this splitter [Integer]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, tgt, sel, opts=SPLITTER_NORMAL, x=0, y=0, width=0, height=0) # :yields: theSplitter
    end
    
    #
    # Return size of the panel at index.
    # Raises IndexError if _index_ is out of range.
    #
    def getSplit(index); end

    #
    # Change the size of panel at the given index.
    # Raises IndexError if _index_ is out of range.
    #
    def setSplit(index, size); end
  end
end

