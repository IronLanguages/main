module Fox
  #
  # Progress bar widget
  #
  # === Progress bar styles
  #
  # +PROGRESSBAR_HORIZONTAL+::	Horizontal display
  # +PROGRESSBAR_VERTICAL+::	Vertical display
  # +PROGRESSBAR_PERCENTAGE+::	Show percentage done
  # +PROGRESSBAR_DIAL+::	Show as a dial instead of bar
  # +PROGRESSBAR_NORMAL+::	same as <tt>FRAME_SUNKEN|FRAME_THICK</tt>
  #
  class FXProgressBar < FXFrame
    # Amount of progress [Integer]
    attr_accessor :progress
    
    # Maximum value for progress [Integer]
    attr_accessor :total

    # Bar width [Integer]
    attr_accessor :barSize

    # Bar color [FXColor]
    attr_accessor :barColor

    # Bar background color [FXColor]
    attr_accessor :barBGColor

    # Text color [FXColor]
    attr_accessor :textColor
    
    # Alternate text color [FXColor]
    attr_accessor :textAltColor
    
    # Text font [FXFont]
    attr_accessor :font
    
    # Progress bar style [Integer]
    attr_accessor :barStyle

    #
    # Construct progress bar.
    #
    def initialize(p, target=nil, selector=0, opts=PROGRESSBAR_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theProgressBar
    end
  
    # Increment progress by given _amount_.
    def increment(amount); end
  
    # Hide progress percentage
    def hideNumber; end
  
    # Show progress percentage
    def showNumber; end
  end
end

