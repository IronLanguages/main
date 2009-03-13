module Fox
  #
  # Notify header?
  #
  class NotifyHeader
    # idFrom [Integer]
    attr_reader :idFrom
    
    # code [Integer]
    attr_reader :code
  end

  #
  # SCNotification
  #
  class SCNotification
    # Header [NotifyHeader]
    attr_reader :nmhdr
    
    # Position, one of SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND [Integer]
    attr_reader :position
    
    # Character, one of SCN_CHARADDED or SCN_KEY [Integer]
    attr_reader :ch
    
    # Modifiers, one of SCN_KEY, ... [Integer]
    attr_reader :modifiers
    
    # Modification type (SCN_MODIFIED) [Integer]
    attr_reader :modificationType

    # Text [String]
    attr_reader :text

    # Length [Integer]
    attr_reader :length
    
    # Lines added [Integer]
    attr_reader :linesAdded
    
    # Message [Integer]
    attr_reader :message
    
    # Line [Integer]
    attr_reader :line
    
    # Fold level now [Integer]
    attr_reader :foldLevelNow
    
    # Previous fold level [Integer]
    attr_reader :foldLevelPrev
    
    # Margin [Integer]
    attr_reader :margin
    
    # List type [Integer]
    attr_reader :listType
    
    # x [Integer]
    attr_reader :x
    
    # y [Integer]
    attr_reader :y

    # wParam [Integer]
    attr_reader :wParam
    
    # lParam [Integer]
    attr_reader :lParam
  end

  class TextRange
    # The text [String]
    attr_reader :lpstrText

    #
    # Return an initialized TextRange instance.
    #
    def initialize(start, last, size); end
  end

  #
  # FXScintilla is a FOX widget, developed by Gilles Filippini, that provides
  # an interface to Neil Hodgson's Scintilla (http://www.scintilla.org) source
  # code editing component. The Scintilla component is a very complicated beast,
  # and for best results you should read the very fine documentation at
  # http://www.scintilla.org/ScintillaDoc.html.
  #
  # === Events
  #
  # The following messages are sent by FXScintilla to its target:
  #
  # +SEL_COMMAND+::
  #   sent when the Scintilla component calls NotifyParent to signal some event.
  #   The message data is an SCNotification instance.
  # +SEL_CHANGED+::
  #   sent when the Scintilla component calls NotifyChange to signal some event.
  # +SEL_RIGHTBUTTONPRESS+::
  #   sent when the right mouse button goes down; the message data is an FXEvent instance.
  #
  class FXScintilla < FXScrollArea
    #
    # Return an initialized FXScintilla instance.
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theScintilla
    end

    #
    # Set the identifier for this widget's embedded Scintilla component.
    #
    def setScintillaID(id); end
  
    #
    # Send a message (_iMsg_) to the Scintilla control, with optional _wParam_
    # and _lParam_ values. Note that in most cases, it will be easier to use
    # one of the convenience methods defined in the 'scintilla' library module.
    #
    def sendMessage(iMsg, wParam=nil, lParam=nil); end
  end
end

