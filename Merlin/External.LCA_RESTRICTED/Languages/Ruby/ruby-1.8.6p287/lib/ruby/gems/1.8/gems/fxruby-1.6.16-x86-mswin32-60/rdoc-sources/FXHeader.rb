module Fox
  #
  # Header item
  #
  # === Alignment hints
  #
  # +RIGHT+::		Align on right
  # +LEFT+::		Align on left
  # +CENTER_X+::	Align centered horizontally (default)
  # +TOP+::		Align on top
  # +BOTTOM+::		Align on bottom
  # +CENTER_Y+::	Align centered vertically (default)
  #
  # === Icon position
  #
  # +BEFORE+::		Icon before the text
  # +AFTER+::		Icon after the text
  # +ABOVE+::		Icon above the text
  # +BELOW+::		Icon below the text
  #
  # === Arrow
  #
  # +ARROW_NONE+::	No arrow
  # +ARROW_UP+::	Arrow pointing up
  # +ARROW_DOWN+::	Arrow pointing down
  # +PRESSED+::		Pressed down
  #
  class FXHeaderItem < FXObject

    # Item's text label [String]
    attr_accessor :text

    # Item's icon [FXIcon]
    attr_accessor :icon

    # Item's user data [Object]
    attr_accessor :data

    # Size [Integer]
    attr_accessor :size
    
    # Sort direction (+FALSE+, +TRUE+ or +MAYBE+) [Integer]
    attr_accessor :arrowDir
    
    # Current position [Integer]
    attr_accessor :pos

    # Content justification (one of +LEFT+, +RIGHT+, +CENTER_X+, +TOP+, +BOTTOM+ or +CENTER_Y+) [Integer]
    attr_accessor :justification

    # Icon position (one of +BEFORE+, +AFTER+, +ABOVE+ or +BELOW+) [Integer]
    attr_accessor :iconPosition

    #
    # Construct new item with given text, icon, size, and user-data
    #
    def initialize(text, ic=nil, s=0, ptr=nil) # :yields: theHeaderItem
    end
    
    # Return the header item's text label
    def to_s; text; end
    
    # Return the item's content width in the header.
    def getWidth(header); end
    
    # Return the item's content height in the header.
    def getHeight(header); end
    
    # Create server-side resources
    def create; end
    
    # Detach from server-side resources
    def detach; end
    
    # Destroy server-side resources
    def destroy; end
    
    # Set pressed state to +true+ or +false+.
    def pressed=(p); end
    
    # Return +true+ if in pressed state.
    def pressed?; end
  end

  #
  # Header control may be placed over a table or list to provide a resizable
  # captions above a number of columns.
  # Each caption comprises a label and an optional icon; in addition, an arrow
  # may be shown to indicate whether the items in that column are sorted, and
  # if so, whether they are sorted in increasing or decreasing order.
  # Each caption can be interactively resized.  During the resizing, if the
  # HEADER_TRACKING was specified, the header control sends a SEL_CHANGED message
  # to its target, with the message data set to the caption number being resized,
  # of the type FXint.
  # If the HEADER_TRACKING was not specified the SEL_CHANGED message is sent at
  # the end of the resizing operation.
  # Clicking on a caption causes a message of type SEL_COMMAND to be sent to the
  # target, with the message data set to the caption number being clicked.
  # A single click on a split causes a message of type SEL_CLICKED to be sent to the
  # target; a typical response to this message would be to adjust the size of
  # the split to fit the contents displayed underneath it.
  # The contents may be scrolled by calling setPosition().
  #
  # === Events
  #
  # The following messages are sent by FXHeader to its target:
  #
  # +SEL_CHANGED+::
  #   sent continuously while a header item is being resized, if the
  #   +HEADER_TRACKING+ option was specified, or at the end of the resize
  #   if +HEADER_TRACKING+ was not specfied. The message data is an integer
  #   indicating the index of the item being resized.
  # +SEL_COMMAND+::
  #   sent when a header item is clicked; the message data is an integer
  #   indicating the index of the current item.
  # +SEL_CLICKED+::
  #   sent when a header item is clicked; the message data is an integer
  #   indicating the index of the current item.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_REPLACED+::		sent when a header item is about to be replaced; the message data is an Integer indicating the index of the item to be replaced.
  # +SEL_INSERTED+::		sent after a header item is inserted; the message data is an Integer indicating the index of the item that was inserted.
  # +SEL_DELETED+::		sent when a header item is about to be removed; the message data is an Integer indicating the index of the item to be removed.
  #
  # === Header style options
  #
  # +HEADER_BUTTON+::		Button style can be clicked
  # +HEADER_HORIZONTAL+::	Horizontal header control (default)
  # +HEADER_VERTICAL+::		Vertical header control
  # +HEADER_TRACKING+::		Tracks continuously while moving
  # +HEADER_RESIZE+::		Allow resizing sections
  # +HEADER_NORMAL+::		Normal options, same as <tt>HEADER_HORIZONTAL|FRAME_NORMAL</tt>
  #
  # === Message identifiers
  #
  # +ID_TIPTIMER+::		x
  #
  class FXHeader < FXFrame
  
    # Number of items [Integer]
    attr_reader :numItems
    
    # Total size of all items [Integer]
    attr_reader :totalSize
    
    # Current position [Integer]
    attr_accessor :position

    # Text font [FXFont]
    attr_accessor :font

    # Text color [FXColor]
    attr_accessor :textColor
  
    # Header style options [Integer]
    attr_accessor :headerStyle

    # Status line help text for this header
    attr_accessor :helpText
  
    #
    # Return an initialized FXHeader instance.
    #
    def initialize(p, target=nil, selector=0, opts=HEADER_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theHeader
    end
  
    #
    # Return the item (a FXHeaderItem instance) at the given index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItem(index); end
  
    #
    # Return the item-index given its coordinate offset.
    # Returns -1 if the specified coordinate is before the first item in the
    # header, or _numItems_ if the coordinate is after the last item in the
    # header.
    #
    def getItemAt(coord); end

    #
    # Replace the item at _index_ with a (possibly subclassed) item and return the index
    # of the replaced item.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the header's
    # message target before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItem(index, item, notify=false); end
  
    #
    # Replace the item at _index_ with a new item with the specified
    # text, icon, size and user data object, and return the index of the replaced
    # item. The new item is created by calling the FXHeader#createItem method.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the header's
    # message target before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItem(index, text, icon=nil, size=0, data=nil, notify=false); end
  
    #
    # Fill the header by appending items from an array of strings.
    # Returns the number of items appended.
    # 
    def fillItems(strings, icon=nil, size=0, data=nil, notify=false); end

    #
    # Insert a new (possibly subclassed) item at the specified _index_ and return the
    # index of the inserted item.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    #
    def insertItem(index, item, notify=false); end
  
    #
    # Insert a new item at the specified _index_ with the specified text, icon, size
    # and user data object, and return the index of the inserted item.
    # The new item is created by calling the FXHeader#createItem method.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    #
    def insertItem(index, text, icon=nil, size=0, data=nil, notify=false); end
  
    #
    # Append a (possibly subclassed) item to the list and return the index
    # of the appended item.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is appended.
    #
    def appendItem(item, notify=false); end
  
    #
    # Append a new item with the specified text, icon, size and user data object,
    # and return the index of the appended item.
    # The new item is created by calling the FXHeader#createItem method.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is appended.
    #
    def appendItem(text, icon=nil, size=0, data=nil, notify=false); end
  
    #
    # Prepend a (possibly subclassed) item to the list and return the index
    # of the prepended item.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is appended.
    #
    def prependItem(item, notify=false); end
  
    #
    # Prepend a new item with the specified text, icon, size and user data object,
    # and return the index of the appended item.
    # The new item is created by calling the FXHeader#createItem method.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the header's
    # message target after the item is appended.
    #
    def prependItem(text, icon=nil, size=0, data=nil, notify=false); end
  
    #
    # Extract item from list and return a reference to the item.
    # If _notify_ is  +true+, a +SEL_DELETED+ message is sent to the header's
    # message target before the item is extracted from the list.
    # Raises IndexError if _index_ is out of bounds.
    #
    def extractItem(index, notify=false); end
    
    #
    # Remove the item at the specified index from this header.
    # If _notify_ is  +true+, a +SEL_DELETED+ message is sent to the header's message target
    # before the item is removed.
    # Raises IndexError if _index_ is out of bounds.
    #
    def removeItem(index, notify=false); end
  
    #
    # Remove all items from this header.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the header's message target
    # before each item is removed.
    #
    def clearItems(notify=false); end
  
    #
    # Change text label for item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemText(index, text); end
  
    #
    # Get text of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemText(index); end
  
    #
    # Change icon of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemIcon(index, icon); end
  
    #
    # Return icon of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemIcon(index); end
  
    #
    # Change size of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemSize(index, size); end
  
    #
    # Return size of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemSize(index); end
  
    #
    # Return the offset (in pixels) of the left side of the item at index.
    # (If it's a vertical header, return the offset of the top side of the item).
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemOffset(index); end
  
    #
    # Change user data object of item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemData(index, ptr); end
  
    #
    # Return user data for item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemData(index); end
  
    #
    # Change arrow (sort) direction for item at index, where _dir_ is either
    # +FALSE+, +TRUE+ or +MAYBE+.
    # If _dir_ is +TRUE+, the arrow will point up; if _dir_ is +FALSE+, the
    # arrow points down; and if _dir_ is +MAYBE+, no arrow is drawn.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setArrowDir(index, dir=MAYBE); end
    
    #
    # Return sort direction for the item at index, one of +FALSE+, +TRUE+ or
    # +MAYBE+.
    # If _dir_ is +TRUE+, the arrow will point up; if _dir_ is +FALSE+, the
    # arrow points down; and if _dir_ is +MAYBE+, no arrow is drawn.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getArrowDir(index); end

    #
    # Change item justification. Horizontal justification is controlled by passing
    # FXHeaderItem::RIGHT, FXHeaderItem::LEFT, or FXHeaderItem::CENTER_X.
    # Vertical justification is controlled by FXHeaderItem::TOP, FXHeaderItem::BOTTOM,
    # or FXHeaderItem::CENTER_Y.
    # The default is a combination of FXHeaderItem::LEFT and FXHeaderItem::CENTER_Y.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemJustify(index, justify); end

    #
    # Return item justification for the item at _index_, one of
    # +LEFT+, +RIGHT+, +CENTER_X+, +TOP+, +BOTTOM+ or +CENTER_Y+.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemJustify(index); end
  
    #
    # Change relative position of icon and text of item.
    # Passing FXHeaderItem::BEFORE or FXHeaderItem::AFTER places the icon
    # before or after the text, and passing FXHeaderItem::ABOVE or
    # FXHeaderItem::BELOW places it above or below the text, respectively.
    # The default of FXHeaderItem::BEFORE places the icon in front of the text.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemIconPosition(index, mode); end
  
    #
    # Return relative icon and text position of the item at _index_,
    # one of +ABOVE+, +BELOW+, +BEFORE+ or +AFTER+.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemIconPosition(index); end

    #
    # Changed button item's pressed state.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemPressed(index, pressed=true); end
  
    #
    # Return +true+ if button item at specified index is pressed in.
    # Raises IndexError if _index_ is out of bounds.
    #
    def isItemPressed(index); end
    
    # Scroll to make the specified item visible.
    def makeItemVisible(index); end
  
    #
    # Repaint header at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def updateItem(index); end
  end
end

