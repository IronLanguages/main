module Fox
  #
  # Highlight style entry
  #
  class FXHiliteStyle 
    # Normal text foreground color [FXColor]
    attr_accessor	:normalForeColor

    # Normal text background color [FXColor]
    attr_accessor	:normalBackColor

    # Selected text foreground color [FXColor]
    attr_accessor	:selectForeColor

    # Selected text background color [FXColor]
    attr_accessor	:selectBackColor

    # Highlight text foreground color [FXColor]
    attr_accessor	:hiliteForeColor

    # Highlight text background color [FXColor]
    attr_accessor	:hiliteBackColor

    # Active text background color [FXColor]
    attr_accessor	:activeBackColor

    # Highlight text style [Integer]
    attr_accessor	:style
  end

  #
  # Text mutation callback data passed with the SEL_INSERTED,
  # SEL_REPLACED, and SEL_DELETED messages; both old and new
  # text is available on behalf of the undo system as well as
  # syntax highlighting.
  #
  class FXTextChange
    # Position in buffer [Integer]
    attr_accessor :pos
    
    # Number of characters deleted at _pos_ [Integer]
    attr_accessor :ndel
    
    # Number of characters inserted at _pos_ [Integer]
    attr_accessor :nins
    
    # Text inserted at _pos_ [String]
    attr_accessor :ins
    
    # Text deleted at _pos_ [String]
    attr_accessor :del
  end

  #
  # The text widget supports editing of multiple lines of text.
  # An optional style table can provide text coloring based on
  # the contents of an optional parallel style buffer, which is
  # maintained as text is edited.  In a typical scenario, the
  # contents of the style buffer is either directly written when
  # the text is added to the widget, or is continually modified
  # by editing the text via syntax-based highlighting engine which
  # colors the text based on syntactical patterns.
  #
  # === Events
  #
  # The following messages are sent by FXText to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key is pressed; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key is released; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  # +SEL_INSERTED+::
  #   sent after text is inserted into the text buffer; the message data
  #   is a reference to an FXTextChange instance.
  # +SEL_DELETED+::
  #   sent after text is removed from the text buffer; the message data is
  #   a reference to an FXTextChange instance.
  # +SEL_REPLACED+::
  #   sent after some text is replaced in the text buffer; the message data is
  #   a reference to an FXTextChange instance.
  # +SEL_CHANGED+::
  #   sent when the contents of the text buffer change in any way;
  #   the message data is an integer indicating the cursor position.
  # +SEL_SELECTED+::
  #   sent after text is selected; the message data is a two-element array
  #   indicating the starting position of the selected text and the number
  #   of characters selected.
  # +SEL_DESELECTED+::
  #   sent before text is deselected; the message data is a two-element array
  #   indicating the starting position of the deselected text and the number
  #   of characters deselected.
  #
  # === Text widget options
  #
  # +TEXT_READONLY+::	Text is _not_ editable
  # +TEXT_WORDWRAP+::	Wrap at word breaks
  # +TEXT_OVERSTRIKE+::	Overstrike mode
  # +TEXT_FIXEDWRAP+::	Fixed wrap columns
  # +TEXT_NO_TABS+::	Insert spaces for tabs
  # +TEXT_AUTOINDENT+::	Autoindent
  # +TEXT_SHOWACTIVE+::	Show active line
  # +TEXT_AUTOSCROLL+::	Logging mode, keeping last line visible
  #
  # === Selection modes
  #
  # +SELECT_CHARS+
  # +SELECT_WORDS+
  # +SELECT_LINES+
  #
  # === Text styles
  #
  # +STYLE_UNDERLINE+::		underline text
  # +STYLE_STRIKEOUT+::		strike out text
  # +STYLE_BOLD_+::			bold text
  #
  # === Message identifiers
  #
  # +ID_CURSOR_TOP+::
  # +ID_CURSOR_BOTTOM+::
  # +ID_CURSOR_HOME+::
  # +ID_CURSOR_END+::
  # +ID_CURSOR_RIGHT+::
  # +ID_CURSOR_LEFT+::
  # +ID_CURSOR_UP+::
  # +ID_CURSOR_DOWN+::
  # +ID_CURSOR_WORD_LEFT+::
  # +ID_CURSOR_WORD_RIGHT+::
  # +ID_CURSOR_PAGEDOWN+::
  # +ID_CURSOR_PAGEUP+::
  # +ID_CURSOR_SCRNTOP+::
  # +ID_CURSOR_SCRNBTM+::
  # +ID_CURSOR_SCRNCTR+::
  # +ID_CURSOR_PAR_HOME+::
  # +ID_CURSOR_PAR_END+::
  # +ID_SCROLL_UP+::
  # +ID_SCROLL_DOWN+::
  # +ID_MARK+::
  # +ID_EXTEND+::
  # +ID_OVERST_STRING+::
  # +ID_INSERT_STRING+::
  # +ID_INSERT_NEWLINE+::
  # +ID_INSERT_TAB+::
  # +ID_CUT_SEL+::
  # +ID_COPY_SEL+::
  # +ID_PASTE_SEL+::
  # +ID_DELETE_SEL+::
  # +ID_SELECT_CHAR+::
  # +ID_SELECT_WORD+::
  # +ID_SELECT_LINE+::
  # +ID_SELECT_ALL+::
  # +ID_SELECT_MATCHING+::
  # +ID_SELECT_BRACE+::
  # +ID_SELECT_BRACK+::
  # +ID_SELECT_PAREN+::
  # +ID_SELECT_ANG+::
  # +ID_DESELECT_ALL+::
  # +ID_BACKSPACE+::
  # +ID_BACKSPACE_WORD+::
  # +ID_BACKSPACE_BOL+::
  # +ID_DELETE+::
  # +ID_DELETE_WORD+::
  # +ID_DELETE_EOL+::
  # +ID_DELETE_LINE+::
  # +ID_TOGGLE_EDITABLE+::
  # +ID_TOGGLE_OVERSTRIKE+::
  # +ID_CURSOR_ROW+::
  # +ID_CURSOR_COLUMN+::
  # +ID_CLEAN_INDENT+::
  # +ID_SHIFT_LEFT+::
  # +ID_SHIFT_RIGHT+::
  # +ID_SHIFT_TABLEFT+::
  # +ID_SHIFT_TABRIGHT+::
  # +ID_UPPER_CASE+::
  # +ID_LOWER_CASE+::
  # +ID_GOTO_MATCHING+::
  # +ID_GOTO_SELECTED+::
  # +ID_GOTO_LINE+::
  # +ID_SEARCH_FORW_SEL+::
  # +ID_SEARCH_BACK_SEL+::
  # +ID_SEARCH+::
  # +ID_REPLACE+::
  # +ID_LEFT_BRACE+::
  # +ID_LEFT_BRACK+::
  # +ID_LEFT_PAREN+::
  # +ID_LEFT_ANG+::
  # +ID_RIGHT_BRACE+::
  # +ID_RIGHT_BRACK+::
  # +ID_RIGHT_PAREN+::
  # +ID_RIGHT_ANG+::
  # +ID_BLINK+::
  # +ID_FLASH+::

  class FXText < FXScrollArea
  
    # Top margin [Integer]
    attr_accessor	:marginTop

    # Bottom margin [Integer]
    attr_accessor	:marginBottom

    # Left margin [Integer]
    attr_accessor	:marginLeft

    # Right margin [Integer]
    attr_accessor	:marginRight

    # Wrap columns [Integer]
    attr_accessor	:wrapColumns

    # Tab columns [Integer]
    attr_accessor	:tabColumns

    # Number of columns used for line numbers [Integer]
    attr_accessor	:barColumns

    # Indicates whether text is modified [Boolean]
    attr_writer		:modified

    # Indicates whether text is editable [Boolean]
    attr_writer		:editable

    # Indicates whether text is styled [Boolean]
    attr_writer		:styled

    # Word delimiters [String]
    attr_accessor	:delimiters

    # Text font [FXFont]
    attr_accessor	:font

    # Text color [FXColor]
    attr_accessor	:textColor

    # Selected text background color [FXColor]
    attr_accessor	:selBackColor

    # Selected text color [FXColor]
    attr_accessor	:selTextColor

    # Highlight text color [FXColor]
    attr_accessor	:hiliteTextColor

    # Highlight text background color [FXColor]
    attr_accessor	:hiliteBackColor

    # Active background color [FXColor]
    attr_accessor	:activeBackColor

    # Cursor color [FXColor]
    attr_accessor	:cursorColor

    # Line number color [FXColor]
    attr_accessor	:numberColor

    # Bar color [FXColor]
    attr_accessor	:barColor

    # Status line help text [String]
    attr_accessor	:helpText

    # Tool tip message [String]
    attr_accessor	:tipText

    # The text buffer [String]
    attr_accessor	:text

    # The length of the text buffer [Integer]
    attr_reader		:length

    # Anchor position [Integer]
    attr_accessor	:anchorPos

    # Cursor row [Integer]
    attr_accessor	:cursorRow

    # Cursor column [Integer]
    attr_accessor	:cursorCol

    # Cursor position [Integer]
    attr_reader		:cursorPos

    # Selection start position [Integer]
    attr_reader		:selStartPos

    # Selection end position [Integer]
    attr_reader		:selEndPos

    # Text widget style [Integer]
    attr_accessor	:textStyle

    # Number of visible rows [Integer]
    attr_accessor	:visibleRows

    # Number of visible columns [Integer]
    attr_accessor	:visibleColumns

    #
    # Brace and parenthesis match highlighting time, in milliseconds [Integer].
    # A _hiliteMatchTime_ of 0 disables brace matching.
    #
    attr_accessor	:hiliteMatchTime

    # Array of hilite styles [an Array of FXHiliteStyle instances]
    attr_accessor	:hiliteStyles

    #
    # Return an initialized FXText instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this text widget [FXComposite]
    # +target+::	the message target, if any, for this text widget [FXObject]
    # +selector+::	the message identifier for this text widget [Integer]
    # +opts+::	text options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2) # :yields: theText
    end
    
    # Return the text buffer's value
    def to_s; text; end
    
    # Return +true+ if text was modified
    def modified? ; end
    
    # Return +true+ if text is editable
    def editable? ; end
    
    # Set overstrike mode to +true+ or +false+.
    def overstrike=(os); end
    
    # Return +true+ if overstrike mode is activated.
    def overstrike? ; end

    # Return +true+ if styled text
    def styled? ; end

    # Get character at position _pos_ in text buffer
    def getByte(pos); end
  
    # Get wide character at position _pos_.
    def getChar(pos); end

    # Get length of wide character at position _pos_.
    def getCharLen(pos); end

    # Get style at position _pos_ in style buffer
    def getStyle(pos); end

    # Extract _n_ bytes of text from position _pos_ in the text buffer
    def extractText(pos, n); end
    
    # Extract _n_ bytes of style info from position _pos_ in the style buffer
    def extractStyle(pos, n); end

    # Replace the _m_ characters at _pos_ with _text_.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the text
    # widget's message target before the old text is removed, and a
    # +SEL_INSERTED+ and +SEL_CHANGED+ message is sent after the new
    # text is inserted.
    def replaceText(pos, m, text, notify=false); end
  
    # Replace the _m_ characters at _pos_ with _text_.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the text
    # widget's message target before the old text is removed, and a
    # +SEL_INSERTED+ and +SEL_CHANGED+ message is sent after the new
    # text is inserted.
    def replaceStyledText(pos, m, text, style=0, notify=false); end
  
    # Append _text_ to the end of the text buffer.
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # added.
    def appendText(text, notify=false); end
  
    # Append _text_ to the end of the text buffer.
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # added.
    def appendStyledText(text, style=0, notify=false); end
  
    # Insert _text_ at position _pos_ in the text buffer.
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # inserted.
    def insertText(pos, text, notify=false); end
  
    # Insert _text_ at position _pos_ in the text buffer.
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # inserted.
    def insertStyledText(pos, text, style=0, notify=false); end

    # Remove _n_ characters of text at position _pos_ in the buffer
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the
    # text widget's message target before the text is removed and a
    # +SEL_CHANGED+ message is sent after the change occurs.
    def removeText(pos, n, notify=false); end
  
    # Change the style of _n_ characters at position _pos_ in the text
    # buffer to _style_. Here, _style_ is  an integer index into the
    # style table, indicating the new style for all the affected characters;
    def changeStyle(pos, n, style); end
  
    # Change the style of text range at position _pos_ in the text
    # buffer to _style_. Here, _style_ an array of bytes indicating
    # the new style.
    def changeStyle(pos, style); end

    # Change the text
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # set.
    def setText(text, notify=false); end
  
    # Change the text in the buffer to new text
    # If _notify_ is +true+, +SEL_INSERTED+ and +SEL_CHANGED+ messages
    # are sent to the text widget's message target after the new text is
    # set.
    def setStyledText(text, style=0, notify=false); end
  
    # Shift block of lines from position _startPos_ up to _endPos_ by given _amount_.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the text
    # widget's message target before the old text is removed, and a
    # +SEL_INSERTED+ and +SEL_CHANGED+ message is sent after the new
    # text is inserted.
    def shiftText(startPos, endPos, amount, notify=false); end

    #
    # Search for _string_ in text buffer, and return the extent of
    # the string as a two-element array of arrays.
    # The first array contains the beginning index (or indices)
    # and the second array contains the ending index (or indices).
    # The search starts from the given
    # _start_ position, scans forward (+SEARCH_FORWARD+) or backward
    # (+SEARCH_BACKWARD+), and wraps around if +SEARCH_WRAP+ has been
    # specified.  The search type is either a plain search (+SEARCH_EXACT+),
    # case insensitive search (+SEARCH_IGNORECASE+), or regular expression
    # search (+SEARCH_REGEX+).
    #
    def findText(string, start=0, flags=SEARCH_FORWARD|SEARCH_WRAP|SEARCH_EXACT); end

    # Return +true+ if position _pos_ is selected
    def positionSelected?(pos); end
  
    # Return +true+ if position _pos_ is fully visible
    def positionVisible?(pos); end
  
    # Return text position at given visible (_x_, _y_) coordinate
    def getPosAt(x, y); end
  
    # Count number of rows; _start_ should be on a row start
    def countRows(start, end); end

    # Count number of columns; _start_ should be on a row start
    def countCols(start, end); end

    # Count number of newlines
    def countLines(start, end); end

    # Return position of beginning of line containing position _pos_
    def lineStart(pos); end
  
    # Return position of end of line containing position _pos_
    def lineEnd(pos); end
  
    # Return start of next line
    def nextLine(pos, nl=1); end
  
    # Return start of previous line
    def prevLine(pos, nl=1); end
  
    # Return row start
    def rowStart(pos); end
  
    # Return row end
    def rowEnd(pos); end

    # Return start of next row
    def nextRow(pos, nr=1); end
  
    # Return start of previous row
    def prevRow(pos, nr=1); end
  
    # Return end of previous word
    def leftWord(pos); end
  
    # Return begin of next word
    def rightWord(pos); end
  
    # Return begin of word
    def wordStart(pos); end
  
    # Return end of word
    def wordEnd(pos); end
  
    # Return validated UTF8 character start position
    def validPos(pos); end

    # Retreat to the previous valid UTF8 character start
    def dec(pos); end
  
    # Advance to the next valid UTF8 character start
    def inc(pos); end

    # Make line containing _pos_ the top line
    def setTopLine(pos); end
  
    # Return position of top line
    def getTopLine(); end
  
    # Make line containing _pos_ the bottom line
    def setBottomLine(pos); end
  
    # Return the position of the bottom line
    def getBottomLine(); end
  
    # Make line containing _pos_ the center line
    def setCenterLine(pos); end

    # Set cursor row.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the text
    # widget's message target after the change occurs.
    def setCursorRow(row, notify=false); end
  
    # Set cursor column.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the text
    # widget's message target after the change occurs.
    def setCursorCol(col, notify=false); end

    # Set cursor position.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the text
    # widget's message target after the change occurs.
    def setCursorPos(pos, notify=false); end

    # Select all text.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the text
    # widget's message target before any previously selected text is
    # deselected, then a +SEL_SELECTED+ message is sent after the new text
    # is selected.
    def selectAll(notify=false); end
  
    # Select _len_ characters starting at position _pos_.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the text
    # widget's message target before any previously selected text is
    # deselected, then a +SEL_SELECTED+ message is sent after the new text
    # is selected.
    def setSelection(pos, len, notify=false); end
  
    # Extend selection to _pos_.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the text
    # widget's message target before any previously selected text is
    # deselected, then a +SEL_SELECTED+ message is sent after the new text
    # is selected.
    def extendSelection(pos, textSelectionMode=SELECT_CHARS, notify=false); end
  
    # Kill the selection.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the text
    # widget's message target before the text is deselected.
    def killSelection(notify=false); end

    # Highlight _len_ characters starting at position _pos_
    def setHighlight(pos, len); end
  
    # Unhighlight the text
    def killHighlight(); end
  
    # Scroll text to make the given position visible
    def makePositionVisible(pos); end
    
    # Return number of rows in buffer.
    def numRows; end
  end
end
