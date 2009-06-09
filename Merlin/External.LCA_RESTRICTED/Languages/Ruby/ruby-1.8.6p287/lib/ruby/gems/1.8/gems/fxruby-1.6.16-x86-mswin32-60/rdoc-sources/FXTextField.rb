module Fox
  #
  # A text field is a single-line text entry widget.
  # The text field widget supports clipboard for cut-and-paste
  # operations.
  # The text field also sends SEL_COMMAND when the focus moves to another control.
  # TEXTFIELD_ENTER_ONLY can be passed to suppress this feature. Typically, this
  # flag is used in dialogs that close when ENTER is hit in a text field.
  #
  # === Events
  #
  # The following messages are sent from FXTextField to its target:
  #
  # +SEL_COMMAND+::		sent when the user presses the *Enter* key or tabs out of the text field; the message data is a String containing the text.
  # +SEL_CHANGED+::		sent when the text changes; the message data is a String containing the text.
  # +SEL_VERIFY+::		sent when the user attempts to enter new text in the text field; the message data is a String containing the proposed new text.
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  #
  # === Textfield styles
  #
  # +TEXTFIELD_PASSWD+::      Password mode
  # +TEXTFIELD_INTEGER+::     Integer mode
  # +TEXTFIELD_REAL+::        Real mode
  # +TEXTFIELD_READONLY+::    NOT editable
  # +TEXTFIELD_ENTER_ONLY+::  Only callback when enter hit
  # +TEXTFIELD_LIMITED+::     Limit entry to given number of columns
  # +TEXTFIELD_OVERSTRIKE+::  Overstrike mode
  # +TEXTFIELD_NORMAL+::      <tt>FRAME_SUNKEN|FRAME_THICK</tt>
  #
  # === Message identifiers
  #
  # +ID_CURSOR_HOME+::
  # +ID_CURSOR_END+::
  # +ID_CURSOR_RIGHT+::
  # +ID_CURSOR_LEFT+::
  # +ID_MARK+::
  # +ID_EXTEND+::
  # +ID_SELECT_ALL+::
  # +ID_DESELECT_ALL+::
  # +ID_CUT_SEL+::
  # +ID_COPY_SEL+::
  # +ID_PASTE_SEL+::
  # +ID_DELETE_SEL+::
  # +ID_OVERST_STRING+::
  # +ID_INSERT_STRING+::
  # +ID_BACKSPACE+::
  # +ID_DELETE+::
  # +ID_TOGGLE_EDITABLE+::
  # +ID_TOGGLE_OVERSTRIKE+::
  # +ID_BLINK+::
  #
  class FXTextField < FXFrame

    # Text field editability [Boolean]
    attr_writer		:editable

    # Cursor position [Integer]
    attr_accessor	:cursorPos

    # Anchor position [Integer]
    attr_accessor	:anchorPos

    # Text [String]
    attr_accessor	:text

    # Text font [FXFont]
    attr_accessor	:font

    # Text color [FXColor]
    attr_accessor	:textColor

    # Background color for selected text [FXColor]
    attr_accessor	:selBackColor

    # Foreground color for selected text [FXColor]
    attr_accessor	:selTextColor

    # Default width of this text field, in terms of a number of columns times the width of the numeral '8' [Integer]
    attr_accessor	:numColumns

    # Text justification mode, a combination of horizontal justification (JUSTIFY_LEFT, JUSTIFY_RIGHT, or JUSTIFY_CENTER_X), and vertical justification (JUSTIFY_TOP, JUSTIFY_BOTTOM, JUSTIFY_CENTER_Y) [Integer]
    attr_accessor	:justify

    # Status line help text [String]
    attr_accessor	:helpText

    # Tool tip message [String]
    attr_accessor	:tipText

    # Text style [Integer]
    attr_accessor	:textStyle

    #
    # Return an initialized FXTextField instance.
    # It should be wide enough to display _ncols_ columns.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this text field [FXComposite]
    # +ncols+::	number of visible items [Integer]
    # +target+::	the message target, if any, for this text field [FXObject]
    # +selector+::	the message identifier for this text field [Integer]
    # +opts+::	text field options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, ncols, target=nil, selector=0, opts=TEXTFIELD_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theTextField
    end
  
    # Return +true+ if text field may be edited
    def editable?() ; end
  
    # Set overstrike mode to +true+ or +false+.
    def overstrike=(os); end
    
    # Return +true+ if overstrike mode is set.
    def overstrike?; end

    # Select all text
    def selectAll(); end
  
    # Select _len_ characters starting at given position _pos_.
    def setSelection(pos, len) ; end
  
    # Extend the selection from the anchor to the given position _pos_.
    def extendSelection(pos) ; end
  
    # Unselect the text
    def killSelection() ; end
  
    # Return +true+ if position _pos_ is selected.
    def posSelected?(pos) ; end
  
    # Return +true+ if position _pos_ is fully visible.
    def posVisible?(pos) ; end
  
    # Scroll text to make the given position _pos_ visible.
    def makePositionVisible(pos) ; end
  end
end
