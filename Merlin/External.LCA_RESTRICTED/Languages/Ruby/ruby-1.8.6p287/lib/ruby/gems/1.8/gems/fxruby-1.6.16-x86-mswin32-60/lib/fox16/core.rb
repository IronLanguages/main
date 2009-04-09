module Fox
  
  class FXIconItem
    def <=>(otherItem)
      text <=> otherItem.text
    end
  end

  class FXListItem
    def <=>(otherItem)
      text <=> otherItem.text
    end
  end
  
  class FXTreeItem
    def <=>(otherItem)
      text <=> otherItem.text
    end
  end
  
  class FXTreeList
    def addItemFirst(*args) # :nodoc:
      warn "addItemFirst() is deprecated; use prependItem() instead"
      prependItem(*args)
    end

    def addItemLast(*args) # :nodoc:
      warn "addItemLast() is deprecated; use appendItem() instead"
      appendItem(*args)
    end

    def addItemAfter(other, *args) # :nodoc:
      warn "addItemAfter() is deprecated; use insertItem() instead"
      insertItem(other.next, other.parent, *args)
    end

    def addItemBefore(other, *args) # :nodoc:
      warn "addItemBefore() is deprecated; use insertItem() instead"
      insertItem(other, other.parent, *args)
    end

    def reparentItem(item, father) # :nodoc:
      warn "reparentItem() is deprecated; use moveItem() instead"
      moveItem(nil, father, item)
    end
  
    def moveItemBefore(other, item) # :nodoc:
      warn "moveItemBefore() is deprecated; use moveItem() instead"
      moveItem(other, other.parent, item)
    end
  
    def moveItemAfter(other, item) # :nodoc:
      warn "moveItemAfter() is deprecated; use moveItem() instead"
      moveItem(other.next, other.parent, item)
    end
  end

  class FXTreeListBox
    def addItemFirst(*args) # :nodoc:
      warn "addItemFirst() is deprecated; use prependItem() instead"
      prependItem(*args)
    end
    def addItemLast(*args) # :nodoc:
      warn "addItemLast() is deprecated; use appendItem() instead"
      appendItem(*args)
    end
    def addItemAfter(other, *args) # :nodoc:
      warn "addItemAfter() is deprecated; use insertItem() instead"
      insertItem(other.next, other.parent, *args)
    end
    def addItemBefore(other, *args) # :nodoc:
      warn "addItemBefore() is deprecated; use insertItem() instead"
      insertItem(other, other.parent, *args)
    end
  end

  class FXDataTarget
    #
    # Returns the stringified representation of this
    # data target's value.
    #
    def to_s
      value.to_s
    end
  end
  
  class FXDockBar
    # Allow docking on the specified side, where _side_ is one of the +ALLOW+
    # constants listed above.
    def allowSide(side)
      self.allowedSides = self.allowedSides | side
    end
    
    # Disallow docking on the specified side, where _side_ is one of the
    # +ALLOW+ constants listed above.
    def disallowSide(side)
      self.allowedSides = self.allowedSides & ~side
    end
    
    # Return +true+ if docking is allowed on the specified side, where _side_
    # is one of the +ALLOW+ constants listed above.
    #
    def allowedSide?(side)
      (allowedSides & side) != 0
    end
  end
  
  class FXFileDialog
    # Allow navigation for this file dialog
    def allowNavigation
      self.navigationAllowed = true
    end
    
    # Disallow navigation for this file dialog
    def disallowNavigation
      self.navigationAllowed = false
    end
  end
  
  class FXFileList
    #
    # Show parent directories.
    #
    def showParentDirs
      self.parentDirsShown = true
    end
    
    #
    # Hide parent directories
    #
    def hideParentDirs
      self.parentDirsShown = false
    end
  end
  
  class FXFileSelector
    # Allow navigation for this file selector
    def allowNavigation
      self.navigationAllowed = true
    end
    
    # Disallow navigation for this file selector
    def disallowNavigation
      self.navigationAllowed = false
    end
  end

  class FXHeader
    #
    # Returns true if the specified header item's arrow points
    # up. Raises IndexError if _index_ is out of bounds.
    #
    def arrowUp?(index)
      if index < 0 || index >= numItems 
        raise IndexError, "header item index out of bounds"
      else
        getArrowDir(index) == Fox::TRUE
      end
    end

    #
    # Returns true if the specified header item's arrow points
    # down. Raises IndexError if _index_ is out of bounds.
    #
    def arrowDown?(index)
      if index < 0 || index >= numItems 
        raise IndexError, "header item index out of bounds"
      else
        getArrowDir(index) == Fox::FALSE
      end
    end

    #
    # Returns true if the specified header item does not display
    # any arrow. Raises IndexError if _index_ is out of bounds.
    #
    def arrowMaybe?(index)
      if index < 0 || index >= numItems 
        raise IndexError, "header item index out of bounds"
      else
        getArrowDir(index) == Fox::MAYBE
      end
    end
  end
  
  class FXHiliteStyle
    #
    # Construct a new FXHiliteStyle instance, with fields initialized from
    # an FXText instance.
    #
    def FXHiliteStyle.from_text(textw)
      hs = new
      hs.activeBackColor = textw.activeBackColor
      hs.hiliteBackColor = textw.hiliteBackColor
      hs.hiliteForeColor = textw.hiliteTextColor
      hs.normalBackColor = textw.backColor
      hs.normalForeColor = textw.textColor
      hs.selectBackColor = textw.selBackColor
      hs.selectForeColor = textw.selTextColor
      hs.style = 0
      hs
    end
  end

  class FXScrollArea
	  # Returns a reference to the scroll corner (an FXScrollCorner instance) for this window.
	  def scrollCorner
		  verticalScrollBar.next
		end
	end

  class FXSettings
    #
    # Read a boolean registry entry from the specified _section_ and _key_.
    # If no value is found, the _default_ value is returned.
    #
    def readBoolEntry(section, key, default=false)
      default = default ? 1 : 0
      readIntEntry(section, key, default) != 0
    end
    
    #
    # Write a boolean registry _value_ to the specified _section_ and _key_.
    #
    def writeBoolEntry(section, key, value)
      writeIntEntry(section, key, value ? 1 : 0)
    end
  end
  
  class FXVec2d
    # Convert to array
    def to_a
      [x, y]
    end
    
    # Convert to string
    def to_s
      to_a.to_s
    end
    
    def inspect; to_a.inspect; end
  end
  
  class FXVec2f
    # Convert to array
    def to_a; [x, y]; end
    
    # Convert to string
    def to_s; to_a.to_s; end
    
    def inspect; to_a.inspect; end
  end
  
  class FXVec3d
    # Convert to array
    def to_a; [x, y, z]; end
    
    # Convert to string
    def to_s; to_a.to_s; end
    
    def inspect; to_a.inspect; end
  end
  
  class FXVec3f
    # Convert to array
    def to_a; [x, y, z]; end
    
    # Convert to string
    def to_s; to_a.to_s; end
    
    def inspect; to_a.inspect; end
  end
  
  class FXVec4d
    # Convert to array
    def to_a; [x, y, z, w]; end
    
    # Convert to string
    def to_s; to_a.to_s; end
    
    def inspect; to_a.inspect; end
  end
  
  class FXVec4f
    # Convert to array
    def to_a; [x, y, z, w]; end
    
    # Convert to string
    def to_s; to_a.to_s; end
    
    def inspect; to_a.inspect; end
  end

  class FXWindow
    #
    # Iterate over the child windows for this window.
    # Note that this only reaches the direct child windows for this window
    # and no deeper descendants. To traverse the entire widget tree,
    # use #each_child_recursive.
    #
    def each_child # :yields: child
      child = self.first
      while child
        next_child = child.next
        yield child
        child = next_child
      end
    end
    
    #
    # Traverse the widget tree starting from this window
    # using depth-first traversal.
    # 
    def each_child_recursive # :yields: child
      each_child do |child|
        yield child
        child.each_child_recursive do |subchild|
          yield subchild
        end
      end
    end
    

    # Returns an array containing all child windows of this window
    def children
      kids = []
      each_child { |kid| kids << kid }
      kids
    end
    
    # Return +true+ if this window (self) comes before sibling window _other_.
    def before?(other)
      FXWindow.before?(other)
    end

    # Return +true+ if this window (_self_) comes after sibling window _other_,
    def after?(other)
      FXWindow.after?(other)
    end

    # Relink this window before sibling window _other_, in the parent's window list.
    def linkBefore(other)
      reparent(self.parent, other)
    end
    
    # Relink this window after sibling window _other_, in the parent's window list.
    def linkAfter(other)
      reparent(self.parent, other.next)
    end
    
    # Setting visible to +true+ calls #show, setting it to +false+ calls #hide.
    def visible=(vis)
      if vis
        show
      else
        hide
      end
    end
  end

  #
  # The drag-and-drop data used for colors is a sequence of unsigned short
  # integers, in native byte ordering. Here, we use the 'S' directive for
  # String#unpack (which treats two successive characters as an unsigned short
  # in native byte order) to decode the R, G, B and A values.
  #
  def Fox.fxdecodeColorData(data)
    clr = data.unpack('S4')
    Fox.FXRGBA((clr[0]+128)/257, (clr[1]+128)/257, (clr[2]+128)/257, (clr[3]+128)/257)
  end

  #
  # The drag-and-drop data used for colors is a sequence of unsigned short
  # integers, in native byte ordering. Here, we use the 'S' directive for
  # Array#pack (which treats two successive characters as an unsigned short
  # in native byte order) to encode the R, G, B and A values.
  #
  def Fox.fxencodeColorData(rgba)
    clr = [ 257*Fox.FXREDVAL(rgba), 257*Fox.FXGREENVAL(rgba), 257*Fox.FXBLUEVAL(rgba), 257*Fox.FXALPHAVAL(rgba) ]
    clr.pack('S4')
  end

  #
  # The drag-and-drop data used for clipboard strings (i.e. when the
  # drag type is FXWindow.stringType) is either a null-terminated
  # string (for Microsoft Windows) or a non-null terminated string
  # (for X11). Use this method to convert string data from the
  # clipboard back into a Ruby string.
  #
  def Fox.fxdecodeStringData(data)
    if /mswin/ =~ PLATFORM
      data.chop
    else
      data
    end
  end

  #
  # The drag-and-drop data used for clipboard strings (i.e. when the
  # drag type is FXWindow.stringType) is either a null-terminated
  # string (for Microsoft Windows) or a non-null terminated string
  # (for X11). Use this method to convert Ruby strings into a format
  # appropriate for the current platform.
  #
  def Fox.fxencodeStringData(str)
    if /mswin/ =~ PLATFORM
      str + "\0"
    else
      str
    end
  end

  #
  # FXStreamError is the base class for exceptions which can occur when
  # working with FXStream and its subclasses.
  #
  class FXStreamError < StandardError
    #
    # This is a factory method that takes an FXStreamStatus code
    # as its input and returns the appropriate exception class.
    #
    def FXStreamError.makeStreamError(status)
      case status
	when FXStreamEnd
	  FXStreamEndError
	when FXStreamFull
	  FXStreamFullError
	when FXStreamNoWrite
	  FXStreamNoWriteError
	when FXStreamNoRead
	  FXStreamNoReadError
	when FXStreamFormat
	  FXStreamFormatError
	when FXStreamUnknown
	  FXStreamUnknownError
	when FXStreamAlloc
	  FXStreamAllocError
	when FXStreamFailure
	  FXStreamFailureError
	else
	  FXStreamError
      end
    end
  end
  
  # Tried to read past the end of a stream
  class FXStreamEndError < FXStreamError ; end
  
  # Filled up a stream's internal buffer, or the disk is full
  class FXStreamFullError < FXStreamError ; end
  
  # Unable to open for write
  class FXStreamNoWriteError < FXStreamError ; end
  
  # Unable to open for read
  class FXStreamNoReadError < FXStreamError ; end
  
  # Stream format error
  class FXStreamFormatError < FXStreamError ; end
  
  # Trying to read unknown class
  class FXStreamUnknownError < FXStreamError ; end
  
  # Alloc failed
  class FXStreamAllocError < FXStreamError ; end
  
  # General failure
  class FXStreamFailureError < FXStreamError ; end
  
  class FXCheckButton
    # Return +true+ if this check button is in the checked state.
    def checked?
      self.checkState == TRUE
    end
    
    # Return +true+ if this check button is in the unchecked state.
    def unchecked?
      self.checkState == FALSE
    end
    
    # Return +true+ if this check button is in the indeterminate, or "maybe", state.
    def maybe?
      self.checkState == MAYBE
    end
  end
  
  class FXComboTableItem < FXTableItem
    #
    # Construct new combobox table item
    #
    def initialize(text, ic=nil, ptr=nil)
      super(nil, ic, ptr)
      self.selections = text
    end

    # Create input control for editing this item
    def getControlFor(table)
      combo = FXComboBox.new(table, 1, :opts => COMBOBOX_STATIC, :padLeft => table.marginLeft, :padRight => table.marginRight, :padTop => table.marginTop, :padBottom => table.marginBottom)
      combo.create
      justify = 0
      justify |= JUSTIFY_LEFT   if (self.justify & FXTableItem::LEFT) != 0
      justify |= JUSTIFY_RIGHT  if (self.justify & FXTableItem::RIGHT) != 0
      justify |= JUSTIFY_TOP    if (self.justify & FXTableItem::TOP) != 0
      justify |= JUSTIFY_BOTTOM if (self.justify & FXTableItem::BOTTOM) != 0
      combo.justify = justify
      combo.font = table.font
      combo.backColor = table.backColor
      combo.textColor = table.textColor
      combo.selBackColor = table.selBackColor
      combo.selTextColor = table.selTextColor
      combo.fillItems(selections)
      combo.text = text
      combo.numVisible = [20, combo.numItems].min
      combo
    end

    # Set value from input control
    def setFromControl(comboBox)
      self.text = comboBox.text
    end
  
    # Set selections as an array of strings
    def selections=(strings)
      @selections = strings
      if @selections.empty?
        self.text = nil
      else
        self.text = @selections[0]
      end
    end
  
    # Return selections
    def selections
      @selections
    end
  end
  
  class FXMenuCheck
    # Return +true+ if this menu check button is in the checked state.
    def checked?
      self.checkState == TRUE
    end
    
    # Return +true+ if this menu check button is in the unchecked state.
    def unchecked?
      self.checkState == FALSE
    end
    
    # Return +true+ if this menu check button is in the indeterminate, or "maybe", state.
    def maybe?
      self.checkState == MAYBE
    end
  end
  
  class FXRadioButton
    # Return +true+ if this radio button is in the checked state.
    def checked?
      self.checkState == TRUE
    end

    # Return +true+ if this radio button is in the unchecked state.
    def unchecked?
      self.checkState == FALSE
    end

    # Return +true+ if this radio button is in the indeterminate, or "maybe", state.
    def maybe?
      self.checkState == MAYBE
    end
  end
  
  class FXMenuRadio
    # Return +true+ if this menu radio button is in the checked state.
    def checked?
      self.checkState == TRUE
    end

    # Return +true+ if this menu radio button is in the unchecked state.
    def unchecked?
      self.checkState == FALSE
    end

    # Return +true+ if this menu radio button is in the indeterminate, or "maybe", state.
    def maybe?
      self.checkState == MAYBE
    end
  end
  
  class FXObject
    require 'enumerator'
    def self.subclasses
      ObjectSpace.enum_for(:each_object, class << self; self; end).to_a
    end
  end

  class FXDC
    #
    # Draw a circle centered at (_x_, _y_), with specified radius.
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the circle's center [Integer]
    # +y+::	y-coordinate of the circle's center [Integer]
    # +r+::	radius of the circle, in pixels [Integer]
    #
    # See also #fillCircle.
    #
    def drawCircle(x, y, r)
      drawArc(x-r, y-r, 2*r, 2*r, 0, 360*64)
    end

    #
    # Draw a filled circle centered at (_x_, _y_), with specified radius.
    #
    # === Parameters:
    #
    # +x+::	x-coordinate of the circle's center [Integer]
    # +y+::	y-coordinate of the circle's center [Integer]
    # +r+::	radius of the circle, in pixels [Integer]
    #
    # See also #drawCircle.
    #
    def fillCircle(x, y, r)
      fillArc(x-r, y-r, 2*r, 2*r, 0, 360*64)
    end
  end
  
  class FXHVec
    def normalize!
      normalized = self.normalize
      0.upto(3) { |idx| self[idx] = normalized[idx] }
      self
    end
  end
  
  class FXTable
    #
    # Append _numColumns_ columns to the right of the table..
    # If _notify_ is +true+, a <tt>SEL_INSERTED</tt> message is sent to the
    # table�s message target for each cell that is inserted.
    #
    def appendColumns(numColumns=1, notify=false)
      insertColumns(self.numColumns, numColumns, notify)
    end
    
    #
    # Append _numRows_ rows to the bottom of the table..
    # If _notify_ is +true+, a <tt>SEL_INSERTED</tt> message is sent to the
    # table�s message target for each cell that is inserted.
    #
    def appendRows(numRows=1, notify=false)
      insertRows(self.numRows, numRows, notify)
    end
    
    # Select cell at (_row_, _col_).
    # If _notify_ is +true+, a +SEL_SELECTED+ message is sent to the table's message target
    # after the item is selected.
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def selectItem(row, col, notify=false)
      selectRange(row, row, col, col, notify)
    end

=begin

    # Deselect cell at (_row_, _col_).
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's message target
    # after the item is deselected.
    # Raises IndexError if either _row_ or _col_ is out of bounds.
    #
    def deselectItem(row, col, notify=false)
      raise IndexError, "row index out of bounds" if row < 0 || row >= numRows
      raise IndexError, "column index out of bounds" if col < 0 || col >= numColumns
      deselectRange(row, row, col, col, notify)
    end

    # Deselect range.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the table's message
    # target for each previously selected cell that becomes deselected as a result of
    # this operation.
    # Raises IndexError if _startRow_, _endRow_, _startColumn_ or _endColumn_ is out of bounds.
    def deselectRange(startRow, endRow, startColumn, endColumn, notify=false)
      raise IndexError, "starting row index out of bounds"    if startRow < 0 || startRow >= numRows
      raise IndexError, "ending row index out of bounds"      if endRow < 0 || endRow >= numRows
      raise IndexError, "starting column index out of bounds" if startColumn < 0 || startColumn >= numColumns
      raise IndexError, "ending column index out of bounds"   if endColumn < 0 || endColumn >= numColumns
      changes = false
      for row in startRow..endRow
        for col in startColumn..endColumn
          changes |= deselectItem(row, col, notify)
        end
      end
      changes
    end
    
=end

  end
end

