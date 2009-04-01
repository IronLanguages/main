module Fox
  #
  # Represents a cell position in an FXTable.
  #
  class FXTablePos
    # Cell row (zero-based) [Integer]
    attr_accessor :row
    
    # Cell column (zero-based) [Integer]
    attr_accessor :col
    
    #
    # Returns an initialized FXTablePos instance.
    #
    def initialize; end
  end
  
  #
  # Represents a range of cells in an FXTable.
  #
  class FXTableRange
    # Starting position for this range of cells [FXTablePos]
    attr_accessor :fm
    
    # Ending position for this range of cells [FXTablePos]
    attr_accessor :to

    #
    # Returns an initialized FXTableRange instance.
    #
    def initialize; end
  end

  #
  # Represents a particular cell in an FXTable.
  #
  class FXTableItem < FXObject

    # Text associated with this cell [String]
    attr_accessor :text
    
    # Icon associated with this cell [FXIcon]
    attr_accessor :icon
    
    # User data associated with this cell [Object]
    attr_accessor :data
    
    # Indicates whether this item has the focus [Boolean]
    attr_writer :focus
    
    # Indicates whether this item is selected [Boolean]
    attr_writer :selected
    
    # Indicates whether this item is enabled [Boolean]
    attr_writer :enabled
    
    # Indicates whether this item is draggable [Boolean]
    attr_writer :draggable
    
    #
    # Indicates how the text in the cell will be justified.
    # This value is some combination of the horizontal justification
    # flags +LEFT+, +CENTER_X+ and +RIGHT+, and the vertical
    # justification flags +TOP+, +CENTER_Y+ and +BOTTOM+.
    #
    attr_accessor :justify
    
    # The icon's position in the cell, relative to the text (one
    # of +BEFORE+, +AFTER+, +ABOVE+ or +BELOW+) [Integer]
    attr_accessor :iconPosition
    
    # Which borders will be drawn for this cell (some combination of
    # +LBORDER+, +RBORDER+, +TBORDER+ and +BBORDER+) [Integer]
    attr_accessor :borders
    
    # The background stipple pattern for this cell [Integer]
    attr_accessor :stipple
    
    #
    # Return an initialized FXTableItem instance.
    #
    # ==== Parameters:
    #
    # +text+::	the text for this table item [String]
    # +icon+::	the icon, if any, for this table item [FXIcon]
    # +data+::	the user data for this table item [Object]
    #
    def initialize(text, icon=nil, data=nil); end

    # Return the width of this item (in pixels)
    def getWidth(table); end

    # Return the height of this item (in pixels)
    def getHeight(table); end
    
    # Return true if this item has the focus
    def hasFocus?; end
    
    # Return true if this item is selected
    def selected?; end
    
    # Return true if this item is enabled
    def enabled?; end
    
    # Return true if this item is draggable
    def draggable?; end
    
    # Return the text for this table item
    def to_s
      text
    end
    
    # Change item icon, deleting the previous item icon if it was owned
    # by this table item.
    def setIcon(icn, owned=false); end
    
    # Draw this table item
    def draw(table, dc, x, y, w, h); end
    
    # Draw borders
    def drawBorders(table, dc, x, y, w, h); end 
    
    # Draw content
    def drawContent(table, dc, x, y, w, h); end 
    
    # Draw hatch pattern
    def drawPattern(table, dc, x, y, w, h); end 
    
    # Draw background behind the cell
    def drawBackground(table, dc, x, y, w, h)
      hg = table.horizontalGridShown? ? 1 : 0
      vg = table.verticalGridShown? ? 1 : 0
      dc.fillRectangle(x + vg, y + hg, w - vg, h - hg)
    end 

    #
    # Create input control for editing this item.
    # Should return a new instance of some subclass of FXWindow.
    #
    def getControlFor(table); end
    
    #
    # Set item value from input _control_ (an instance of some subclass
    # of FXWindow).
    #
    def setFromControl(control); end

    # Create the server-side resources associated with this table item
    def create; end
    
    # Detach the server-side resources associated with this table item
    def detach; end
    
    # Destroy the server-side resources associated with this table item
    def destroy; end
  end
  
  #
  # The FXTable widget displays a table of items, each with some text and optional
  # icon.  A column Header control provide captions for each column, and a row
  # Header control provides captions for each row.  Columns are resizable by
  # means of the column Header control if the TABLE_COL_SIZABLE option is passed.
  # Likewise, rows in the table are resizable if the TABLE_ROW_SIZABLE option is
  # specified.  An entire row (column) can be selected by clicking on the a button
  # in the row (column) Header control.  Passing TABLE_NO_COLSELECT disables column
  # selection, and passing TABLE_NO_ROWSELECT disables column selection.
  # When TABLE_COL_RENUMBER is specified, columns are automatically renumbered when
  # columns are added or removed.  Similarly, TABLE_ROW_RENUMBER will cause row numbers
  # to be recalculated automatically when rows are added or removed.
  # To disable editing of cells in the table, the TABLE_READONLY can be specified.
  # Cells in the table may or may not have items in them.  When populating a cell
  # for the first time, an item will be automatically created if necessary.  Thus,
  # a cell in the table takes no space unless it has actual contents.
  # Moreover, a contiguous, rectangular region of cells in the table may refer to
  # one single item; in that case, the item will be stretched to cover all the
  # cells in the region, and no grid lines will be drawn interior to the spanning
  # item.
  #
  # The Table widget issues SEL_SELECTED or SEL_DESELECTED when cells are selected
  # or deselected, respectively.  The table position affected is passed along as the
  # 3rd parameter of these messages.
  # Whenever the current (focus) item is changed, a SEL_CHANGED message is sent with
  # the new table position as a parameter.
  # When items are added to the table, a SEL_INSERTED message is sent, with the table
  # range of the newly added cells as the parameter in the message.
  # When items are removed from the table, a SEL_DELETED message is sent prior to the
  # removal of the items, and the table range of the removed cells is passed as a parameter.
  # A SEL_REPLACED message is sent when the contents of a cell are changed, either through
  # editing or by other means; the parameter is the range of affected cells.  This message
  # is sent prior to the change.
  # SEL_CLICKED, SEL_DOUBLECLICKED, and SEL_TRIPLECLICKED messages are sent when a cell
  # is clicked, double-clicked, or triple-clicked, respectively. 
  # A SEL_COMMAND is sent when an enabled item is clicked inside the table.
  #
  # === Events
  #
  # The following messages are sent by FXTable to its target:
  #
  # +SEL_COMMAND+::		sent when a new item is clicked; the message data is an FXTablePos instance indicating the current cell.
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_SELECTED+::		sent when a cell is selected; the message data is an FXTablePos instance indicating the position of the selected cell.
  # +SEL_DESELECTED+::		sent when a cell is deselected; the message data is an FXTablePos instance indicating the position of the deselected cell.
  # +SEL_CHANGED+::		sent when the current cell changes; the message data is an FXTablePos instance indicating the current cell.
  # +SEL_CLICKED+::		sent when a cell is single-clicked; the message data is an FXTablePos instance indicating the current cell.
  # +SEL_DOUBLECLICKED+::	sent when a cell is double-clicked; the message data is an FXTablePos instance indicating the current cell.
  # +SEL_TRIPLECLICKED+::	sent when a cell is triple-clicked; the message data is an FXTablePos instance indicating the current cell.
  # +SEL_DELETED+::		sent when a range of cells is about to be removed; the message data is an FXTableRange instance indicating the cells to be removed.
  # +SEL_INSERTED+::		sent when a range of cells has been inserted; the message data is an FXTableRange instance indicating the cells inserted.
  # +SEL_REPLACED+::		sent when a range of cells has been replaced; the message data is an FXTableRange instance indicating the cells replaced.
  #
  # === Table options
  #
  # +TABLE_COL_SIZABLE+::	Columns are resizable
  # +TABLE_ROW_SIZABLE+::	Rows are resizable
  # +TABLE_NO_COLSELECT+::	Disallow column selections
  # +TABLE_NO_ROWSELECT+::	Disallow row selections
  # +TABLE_READONLY+::		Table is not editable
  # +TABLE_COL_RENUMBER+::	Renumber columns
  # +TABLE_ROW_RENUMBER+::	Renumber rows
  #
  # === Message identifiers
  #
  # +ID_HORZ_GRID+::		x
  # +ID_VERT_GRID+::		x
  # +ID_TOGGLE_EDITABLE+::	x
  # +ID_DELETE_COLUMN+::	x
  # +ID_DELETE_ROW+::		x
  # +ID_INSERT_COLUMN+::	x
  # +ID_INSERT_ROW+::		x
  # +ID_SELECT_COLUMN_INDEX+::	x
  # +ID_SELECT_ROW_INDEX+::		x
  # +ID_SELECT_COLUMN+::	x
  # +ID_SELECT_ROW+::		x
  # +ID_SELECT_CELL+::		x
  # +ID_SELECT_ALL+::		x
  # +ID_DESELECT_ALL+::		x
  # +ID_MOVE_LEFT+::		x
  # +ID_MOVE_RIGHT+::		x
  # +ID_MOVE_UP+::		x
  # +ID_MOVE_DOWN+::		x
  # +ID_MOVE_HOME+::		x
  # +ID_MOVE_END+::		x
  # +ID_MOVE_TOP+::		x
  # +ID_MOVE_BOTTOM+::		x
  # +ID_MOVE_PAGEDOWN+::	x
  # +ID_MOVE_PAGEUP+::		x
  # +ID_START_INPUT+::		x
  # +ID_CANCEL_INPUT+::		x
  # +ID_ACCEPT_INPUT+::		x
  # +ID_MARK+::			x
  # +ID_EXTEND+::		x
  # +ID_CUT_SEL+::		x
  # +ID_COPY_SEL+::		x
  # +ID_PASTE_SEL+::		x
  # +ID_DELETE_SEL+::	x

  class FXTable < FXScrollArea

    # Button in the upper left corner [FXButton]
    attr_reader :cornerButton
    
    # Column header control [FXHeader]
    attr_reader :columnHeader
    
    # Row header control [FXHeader]
    attr_reader :rowHeader

    # Number of visible rows [Integer]
    attr_accessor	:visibleRows
    
    # Number of visible columns [Integer]
    attr_accessor	:visibleColumns
    
    # Number of rows [Integer]
    attr_reader		:numRows
    
    # Number of columns [Integer]
    attr_reader		:numColumns
    
    # Top cell margin, in pixels [Integer]
    attr_accessor	:marginTop
    
    # Bottom cell margin, in pixels [Integer]
    attr_accessor	:marginBottom
    
    # Left cell margin, in pixels [Integer]
    attr_accessor	:marginLeft
    
    # Right cell margin, in pixels [Integer]
    attr_accessor	:marginRight
    
    # Table style [Integer]
    attr_accessor	:tableStyle
    
    # The column header height mode is either fixed (LAYOUT_FIX_HEIGHT) or variable.
    # In variable height mode, the column header will size to fit the contents in it.
    # In fixed height mode, the size is explicitly set via the _columnHeaderHeight_
    # attribute.
    attr_accessor	:columnHeaderMode
    
    # The row header width mode is either fixed (LAYOUT_FIX_WIDTH) or variable.
    # In variable width mode, the row header will size to fit the contents in it.
    # In fixed width mode, the size is explicitly set via the _rowHeaderWidth_
    # attribute.
    attr_accessor	:rowHeaderMode
    
    # Row header font [FXFont]
    attr_accessor	:rowHeaderFont
    
    # Column header font [FXFont]
    attr_accessor	:columnHeaderFont
    
    # The fixed column header height, if _columnHeaderMode_ is +LAYOUT_FIX_HEIGHT+.
    attr_accessor	:columnHeaderHeight
    
    # The fixed row header width, if _rowHeaderMode_ is +LAYOUT_FIX_WIDTH+.
    attr_accessor	:rowHeaderWidth
    
    # Default column width, in pixels [Integer]
    attr_accessor	:defColumnWidth
    
    # Default row height, in pixels [Integer]
    attr_accessor	:defRowHeight
    
    # Row number for current cell [Integer]
    attr_reader		:currentRow
    
    # Column number for current cell [Integer]
    attr_reader		:currentColumn
    
    # Row number for anchor cell [Integer]
    attr_reader		:anchorRow
    
    # Column number for anchor cell [Integer]
    attr_reader		:anchorColumn
    
    # Starting row number for selection, or -1 if there is no selection [Integer]
    attr_reader		:selStartRow
    
    # Starting column number for selection, or -1 if there is no selection [Integer]
    attr_reader		:selStartColumn
    
    # Ending row number for selection, or -1 if there is no selection [Integer]
    attr_reader		:selEndRow
    
    # Ending column number for selection, or -1 if there is no selection [Integer]
    attr_reader		:selEndColumn
    
    # Text font [FXFont]
    attr_accessor	:font
    
    # Text color [FXColor]
    attr_accessor	:textColor
    
    # Base GUI color [FXColor]
    attr_accessor	:baseColor
    
    # Highlight color [FXColor]
    attr_accessor	:hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor	:shadowColor
    
    # Border color [FXColor]
    attr_accessor	:borderColor
    
    # Background color for selected cell(s) [FXColor]
    attr_accessor	:selBackColor
    
    # Text color for selected cell(s) [FXColor]
    attr_accessor	:selTextColor

    # Grid color [FXColor]
    attr_accessor	:gridColor

    # Stipple color [FXColor]
    attr_accessor	:stippleColor

    # Cell border color [FXColor]
    attr_accessor	:cellBorderColor

    # Cell border width, in pixels [Integer]
    attr_accessor	:cellBorderWidth

    # Status line help text [String]
    attr_accessor	:helpText

    # Returns the drag type for CSV data
    def FXTable.csvType; end
    
    # Returns the drag type name for CSV data
    def FXTable.csvTypeName; end

    #
    # Construct a new FXTable instance.
    # The table is initially empty, and reports a default size based on
    # the scroll areas's scrollbar placement policy.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this table [FXComposite]
    # +target+::	the message target (if any) for this table [FXObject]
    # +selector+::	the message identifier for this table [Integer]
    # +opts+::	table options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_MARGIN, padRight=DEFAULT_MARGIN, padTop=DEFAULT_MARGIN, padBottom=DEFAULT_MARGIN) # :yields: theTable
    end
  
    # Set visibility of horizontal grid to +true+ or +false+.
    def horizontalGridShown=(vis); end

    # Return +true+ if horizontal grid is shown.
    def horizontalGridShown? ; end

    # Set visibility of vertical grid to +true+ or +false+.
    def verticalGridShown=(vis); end

    # Is vertical grid shown?
    def verticalGridShown? ; end

    #
    # Show or hide horizontal grid.
    # Note that this is equivalent to the #horizontalGridShown=() method.
    #
    def showHorzGrid(on=true) ; end
  
    #
    # Show or hide vertical grid.
    # Note that this is equivalent to the #verticalGridShown=() method.
    #
    def showVertGrid(on=true) ; end
    
    # Set editability of this table to +true+ or +false+.
    def editable=(edit); end
    
    # Return +true+ if this table is editable.
    def editable? ; end

    #
    # Start input mode for the cell at the given position.
    # An input control is created which is used to edit the cell;
    # it is filled by the original item's contents if the cell contained
    # an item.  You can enter input mode also by sending the table an
    # <tt>ID_START_INPUT</tt> message.
    #
    def startInput(row, col); end

    #
    # Cancel input mode.  The input control is immediately deleted
    # and the cell will retain its old value. You can also cancel
    # input mode by sending the table an <tt>ID_CANCEL_INPUT</tt> message.
    #
    def cancelInput(); end

    #
    # End input mode and accept the new value from the control.
    # The item in the cell will be set to the value from the control,
    # and the control will be deleted. If +true+ is passed, a +SEL_REPLACED+
    # callback will be generated to signify to the target that this call
    # has a new value. You can also accept the input by sending the table
    # an <tt>ID_ACCEPT_INPUT</tt> message.
    #
    def acceptInput(notify=false); end

    #
    # Determine row containing _y_.
    # Returns -1 if _y_ is above the first row, and _numRows_ if _y_ is below the last row.
    # Otherwise, returns the row in the table containing _y_.
    #
    def rowAtY(y) ; end
    
    #
    # Determine column containing _x_.
    # Returns -1 if _x_ is to the left of the first column, and _numColumns_ if _x_ is
    # to the right of the last column. Otherwise, returns the column in the table
    # containing _x_.
    #
    def colAtX(x) ; end
  
    # Return the item (a reference to an FXTableItem) at the given _row_ and _column_.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def getItem(row, column) ; end

    # Replace the item at the given _row_ and _column_ with a (possibly subclassed) _item_.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def setItem(row, column, item) ; end

    #
    # Resize the table content to _numRows_ rows and _numCols_ columns.
    # Note that all existing items in the table will be destroyed and new
    # items will be constructed.
    # If _notify_ is +true+, then
    # * a +SEL_DELETED+ message will be sent to the table's message target
    #   indicating which cells (if any) are about to be destroyed as a result of the resize;
    # * a +SEL_INSERTED+ message will be sent to the table's message target
    #   indicating which cells (if any) were added as a result of the resize; and,
    # * a +SEL_CHANGED+ message will be sent to the table's message target
    #   indicating the new current cell.
    #
    # Raises ArgError if either _numRows_ or _numCols_ is less than zero.
    #
    def setTableSize(numRows, numCols, notify=false) ; end
  
    #
    # Insert _numRows_ rows beginning at the specified _row_ number.
    # If _row_ is equal to the number of rows in the table, the new
    # rows are added to the bottom of the table.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the table's
    # message target for each cell that is inserted.
    # Raises IndexError if _row_ is out of bounds.
    #
    def insertRows(row, numRows=1, notify=false) ; end
    
    #
    # Insert _numColumns_ columns beginning at the specified _column_ number.
    # If _column_ is equal to the number of columns in the table, the
    # new columns are added to the right of the table.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the table's
    # message target for each cell that is inserted.
    # Raises IndexError if _column_ is out of bounds.
    #
    def insertColumns(column, numColumns=1, notify=false) ; end
    
    #
    # Remove the _nr_ rows starting at the specified _row_.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the table's
    # message target for each cell that is removed.
    # Raises IndexError if _row_ is less than zero, or if _row_ + _nr_
    # is greater than the current number of table rows.
    #
    def removeRows(row, nr=1, notify=false) ; end
    
    #
    # Remove the _nc_ columns starting at the specified _column_.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the table's
    # message target for each cell that is removed.
    # Raises IndexError if _column_ is less than zero, or if
    # _column_ + _nc_ is greater than the current number of table columns.
    #
    def removeColumns(column, nc=1, notify=false) ; end
    
    #
    # Extract item from table and return a reference to it.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the table's
    # message target before this cell is removed.
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def extractItem(r, c, notify=false); end
    
    #
    # Remove item at (_row_, _col_), replacing it with +nil+.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the table's
    # message target before this cell is removed.
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def removeItem(row, col, notify=false) ; end

    #
    # Remove all cells in the specified range of rows and columns.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the table's
    # message target before each cell is removed.
    # Raises IndexError if _startrow_, _endrow_, _startcol_ or
    # _endcol_ is out of bounds.
    #
    def removeRange(startrow, endrow, startcol, endcol, notify=false); end

    #
    # Remove all items from table.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the table's
    # message target before the cells are removed.
    #
    def clearItems(notify=false); end

    # Scroll to make cell at (_row_, _column_) fully visible.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def makePositionVisible(row, column) ; end
  
    # Returns +true+ if the cell at position (_row_, _column_) is visible.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def itemVisible?(row, column) ; end
    
    # Set column width.
    # Raises IndexError if _column_ is out of bounds.
    def setColumnWidth(column, columnWidth) ; end
    
    # Get column width.
    # Raises IndexError if _column_ is out of bounds.
    def getColumnWidth(column) ; end
  
    # Set row height.
    # Raises IndexError if _row_ is out of bounds.
    def setRowHeight(row, rowHeight) ; end
    
    # Get row height.
    # Raises IndexError if _row_ is out of bounds.
    def getRowHeight(row) ; end
  
    # Set x-coordinate for column.
    # Raises IndexError if _column_ is out of bounds.
    def setColumnX(column, x) ; end
    
    # Get x-coordinate of column.
    # Raises IndexError if _column_ is out of bounds.
    def getColumnX(column) ; end
  
    # Set y-coordinate of row.
    # Raises IndexError if _row_ is out of bounds.
    def setRowY(row, y) ; end
    
    # Get y-coordinate of row.
    # Raises IndexError if _row_ is out of bounds.
    def getRowY(row) ; end
  
    # Return minimum row height for row _r_.
    # Raises IndexError if _r_ is out of bounds.
    def minRowHeight(r); end
    
    # Return minimum column width for column _c_.
    # Raises IndexError if _c_ is out of bounds.
    def minColumnWidth(c); end

    #
    # Fit row heights to contents, for the _nr_ rows beginning with row index
    # _row_.
    #
    def fitRowsToContents(row, nr=1); end

    #
    # Fit column widths to contents, for the _nc_ columns beginning with
    # column index _col_.
    #
    def fitColumnsToContents(col, nc=1); end
    
    # Set column header at _index_ to _text_.
    def setColumnText(index, text); end
    
    # Return text of column header at _index_.
    def getColumnText(index); end
    
    # Set row header at _index_ to _text_.
    def setRowText(index, text); end
    
    # Return text of row header at _index_.
    def getRowText(index); end

    # Change column header icon
    def setColumnIcon(FXint index,FXIcon* icon);

    # Return icon of column header at index
    def getColumnIcon(index); end

    # Change row header icon
    def setRowIcon(index, icon); end
    
    # Return icon of row header at index
    def getRowIcon(index); end

    # Change column header icon position, e.g. FXHeaderItem::BEFORE, etc.
    def setColumnIconPosition(index, mode); end

    # Return icon position of column header at index
    def getColumnIconPosition(index); end

    # Change row header icon position, e.g. FXHeaderItem::BEFORE, etc.
    def setRowIconPosition(index, mode); end

    # Return icon position of row header at index
    def getRowIconPosition(index); end
    
    # Change column header justify, e.g. FXHeaderItem::RIGHT, etc.
    def setColumnJustify(index, justify); end

    # Return justify of column header at index
    def getColumnJustify(index); end

    # Change row header justify, e.g. FXHeaderItem::RIGHT, etc.
    def setRowJustify(index, justify); end

    # Return justify of row header at index
    def getRowJustify(index); end
    
    #
    # Modify cell text for item at specified _row_ and _col_.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the table's
    # message target before the item's text is changed..
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def setItemText(row, col, text, notify=false) ; end

    # Return cell text for item at specified _row_ and _column_.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def getItemText(row, column) ; end

    #
    # Modify cell icon, deleting the old icon if it was owned.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the table's
    # message target before the item's icon is changed..
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def setItemIcon(row, col, icon, notify=false) ; end

    # Return item icon.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def getItemIcon(row, column) ; end
  
    # Modify cell user data.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def setItemData(row, column, data) ; end
    
    # Return cell user data.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def getItemData(row, column) ; end

    #
    # Extract the text from all the cells in the specified range and
    # return the result as a string.
    # Within the result string, each column's text is delimited by
    # the string specified by _cs_, and each row is delimited by
    # the string specified by _rs_.
    # To reverse this operation (i.e. set the table cells' text
    # from a string), see #overlayText.
    # Raises IndexError if any of _startrow_, _endrow_, _startcol_
    # or _endcol_ is out of bounds.
    #
    # ==== Parameters:
    #
    # +startrow+::	the starting row for the range [Integer]
    # +endrow+::	the ending row for the range [Integer]
    # +startcol+::	the starting column for the range [Integer]
    # +endcol+::	the ending column for the range [Integer]
    # +cs+::		the string to insert at each column break [String]
    # +rs+::		the string to insert at each row break [String]
    #
    def extractText(startrow, endrow, startcol, endcol, cs="\t", rs="\n"); end
    
    #
    # Overlay the text for the cells in the specified range with
    # the fields specified in _text_.
    # Within the _text_ string, each column's text should delimited by
    # the character specified by _cs_, and each row should be delimited by
    # the character specified by _rs_.
    # To reverse this operation (i.e. extract the table cells' text
    # into a string), see #extractText.
    # Raises IndexError if any of _startrow_, _endrow_, _startcol_
    # or _endcol_ is out of bounds.
    #
    # ==== Parameters:
    #
    # +startrow+::	the starting row for the range [Integer]
    # +endrow+::	the ending row for the range [Integer]
    # +startcol+::	the starting column for the range [Integer]
    # +endcol+::	the ending column for the range [Integer]
    # +text+::		the text containing the new cell text [String]
    # +cs+::		the character to insert at each column break [String]
    # +rs+::		the character to insert at each row break [String]
    #
    def overlayText(startrow, endrow, startcol, endcol, text, cs="\t", rs="\n", notify=false); end
    
    #
    # Determine the number of rows and columns in a block of text
    # where columns are separated by characters from the set _cs_, and rows
    # are separated by characters from the set _rs_.
    # Return a two-element array containing the number of rows and
    # columns, respectively.
    #
    def countText(text, cs="\t,", rs="\n"); end

    # Return +true+ if the cell at position (_r_, _c_) is a spanning cell.
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def itemSpanning?(r, c); end
    
    #
    # Repaint cells between grid lines (_startRow_, _endRow_) and grid lines
    # (_startCol_, _endCol_).
    # Raises IndexError if any of the starting or ending grid lines is out of bounds.
    #
    def updateRange(startRow, endRow, startCol, endCol) ; end
  
    # Repaint cell.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def updateItem(row, column) ; end
  
    # Enable cell.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def enableItem(row, column) ; end
    
    # Disable cell.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def disableItem(row, column) ; end
    
    # Returns +true+ if the cell at position (_row_, _column_) is enabled.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def itemEnabled?(row, column) ; end

    #
    # Change item justification for the cell at (_r_, _c_).
    # Horizontal justification is controlled by passing
    # FXTableItem::RIGHT,  FXTableItem::LEFT, or FXTableItem::CENTER_X.
    # Vertical justification is controlled by FXTableItem::TOP, FXTableItem::BOTTOM,
    # or FXTableItem::CENTER_Y.
    # The default is a combination of FXTableItem::RIGHT and FXTableItem::CENTER_Y.
    #
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    #
    def setItemJustify(r, c, justify); end
    
    # Return item justification for the cell at (_r_, _c_).
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def getItemJustify(r, c); end
    
    #
    # Change relative position of icon and text of item at (_r_, _c_).
    # Passing FXTableItem::BEFORE or FXTableItem::AFTER places the icon
    # before or after the text, and passing FXTableItem::ABOVE or
    # FXTableItem::BELOW places it above or below the text, respectively.
    # The default is 0 which places the text on top of the icon.
    #
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    #
    def setItemIconPosition(r, c, mode); end
    
    # Return the relative position of the icon and text for the cell at (_r_, _c_).
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def getItemIconPosition(r, c); end
  
    #
    # Change item borders style for the item at (_r_, _c_).
    # Borders on each side of the item can be turned
    # controlled individually using FXTableItem::LBORDER, FXTableItem::RBORDER,
    # FXTableItem::TBORDER and FXTableItem::BBORDER.
    #
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    #
    def setItemBorders(r, c, borders); end
    
    # Return the border style for the cell at (_r_, _c_).
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def getItemBorders(r, c); end

    # Set the background stipple style for the cell at (_r_, _c_).
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def setItemStipple(r, c, pat); end
    
    # Return the background stipple style for the cell at (_r_, _c_).
    # Raises IndexError if either _r_ or _c_ is out of bounds.
    def getItemStipple(r, c); end
    
    # Change current cell.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the table's
    # message target after the current item changes.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def setCurrentItem(row, column, notify=false) ; end

    # Returns +true+ if the cell at position (_row_, _column_) is the current cell.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def itemCurrent?(row, column) ; end
    
    # Change anchored cell.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def setAnchorItem(row, column) ; end

    # Returns +true+ if the cell at position (_row_, _column_) is selected.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def itemSelected?(row, column) ; end
    
    # Return +true+ if the specified row of cells is selected.
    # Raises IndexError if _r_ is out of bounds.
    def rowSelected?(r); end
    
    # Return +true+ if the specified column of cells is selected.
    # Raises IndexError if _c_ is out of bounds.
    def columnSelected?(c); end
    
    # Return +true+ if any cells are selected.
    def anythingSelected?; end
    
    # Select a row of cells.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's message
    # target for each previously selected cell that becomes deselected as a result of
    # this operation. Likewise, a +SEL_SELECTED+ message is sent to the table's
    # message target for each newly-selected cell.
    # Raises IndexError if _row_ is out of bounds.
    def selectRow(row, notify=false); end
    
    # Select a column of cells.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's message
    # target for each previously selected cell that becomes deselected as a result of
    # this operation. Likewise, a +SEL_SELECTED+ message is sent to the table's
    # message target for each newly-selected cell.
    # Raises IndexError if _col_ is out of bounds.
    def selectColumn(col, notify=false); end
    
    # Select range.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's message
    # target for each previously selected cell that becomes deselected as a result of
    # this operation. Likewise, a +SEL_SELECTED+ message is sent to the table's
    # message target for each newly-selected cell.
    # Raises IndexError if _startRow_, _endRow_, _startColumn_ or _endColumn_ is out of bounds.
    def selectRange(startRow, endRow, startColumn, endColumn, notify=false) ; end
  
    # Extend selection.
    # If _notify_ is +true+, a series of +SEL_SELECTED+ and +SEL_DESELECTED+ messages are sent to the table's message target
    # after each affected item is selected or deselected.
    # Raises IndexError if either _row_ or _column_ is out of bounds.
    def extendSelection(row, column, notify=false) ; end
  
    # Kill selection.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's
    # message target for each cell that was previously selected.
    def killSelection(notify=false) ; end

    #
    # Change cell background color.
    # The values for _row_ and _column_ are either zero or one.
    # If the value is zero, this background color is used for even-numbered
    # rows (columns). If the value is one, this background color is used
    # for odd-numbered rows (columns).
    # See also #getCellColor.
    #
    def setCellColor(row, column, color) ; end
  
    #
    # Obtain cell background color.
    # The values for _row_ and _column_ are either zero or one.
    # If the value is zero, returns the background color used for even-numbered
    # rows (columns). If the value is one, returns the background color used
    # for odd-numbered rows (columns).
    # See also #setCellColor.
    #
    def getCellColor(row, column) ; end
    
    # Create a new table item
    def createItem(text, icon, data) ; end
    
    # Draw a table cell
    def drawCell(dc, xlo, xhi, ylo, yhi, xoff, yoff, startRow, endRow, startCol, endCol) ; end
    
    # Draw a range of cells
    def drawRange(dc, xlo, xhi, ylo, yhi, xoff, yoff, rlo, rhi, clo, chi) ; end
    
    # Set column renumbering to +true+ or +false+.
    def columnRenumbering=(renumber); end
    
    # Get column renumbering
    def columnRenumbering? ; end
    
    # Set row renumbering to +true+ or +false+.
    def rowRenumbering=(renumber); end
    
    # Get row renumbering
    def rowRenumbering? ; end
  end
end

