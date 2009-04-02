module Fox
  #
  # Icon list item
  #
  class FXIconItem < FXObject

    # Item text [String]
    attr_accessor :text

    # Big icon [FXIcon]
    attr_accessor :bigIcon

    # Mini icon [FXIcon]
    attr_accessor :miniIcon

    # Item user data [Object]
    attr_accessor :data

    # Indicates whether this item is selected or not [Boolean]
    attr_writer	:selected

    # Indicates whether this item is enabled or not [Boolean]
    attr_writer	:enabled

    # Indicates whether this item is draggable or not [Boolean]
    attr_writer	:draggable
    
    # Constructor
    def initialize(text, bigIcon=nil, miniIcon=nil, data=nil) # :yields: theIconItem
    end
    
    # Return the icon item's text
    def to_s; text; end

    # Set the focused state for this item (where _focus_ is either +true+ or +false+)
    def setFocus(focus); end

    # Returns +true+ if this item has the focus
    def hasFocus? ; end
    
    # Return +true+ if this item is selected
    def selected? ; end
    
    # Return +true+ if this item is enabled
    def enabled? ; end
    
    # Return +true+ if this item is draggable
    def draggable? ; end
    
    # Return the width of this item
    def getWidth(iconList); end
    
    # Return the height of this item
    def getHeight(iconList); end
    
    # Create this item
    def create; end
    
    # Detach this item
    def detach; end
    
    # Destroy this item
    def destroy; end
  end

  #
  # A Icon List Widget displays a list of items, each with a text and
  # optional icon.  Icon List can display its items in essentially three
  # different ways; in big-icon mode, the bigger of the two icons is used
  # for each item, and the text is placed underneath the icon. In mini-
  # icon mode, the icons are listed in rows and columns, with the smaller
  # icon preceding the text.  Finally, in detail mode the icons are listed
  # in a single column, and all fields of the text are shown under a
  # header control with one button for each subfield.
  # When an item's selected state changes, the icon list sends
  # a +SEL_SELECTED+ or +SEL_DESELECTED+ message.  A change of the current
  # item is signified by the +SEL_CHANGED+ message.
  # The icon list sends +SEL_COMMAND+ messages when the user clicks on an item,
  # and +SEL_CLICKED+, +SEL_DOUBLECLICKED+, and +SEL_TRIPLECLICKED+ when the user
  # clicks once, twice, or thrice, respectively.
  # When items are added, replaced, or removed, the icon list sends messages
  # of the type +SEL_INSERTED+, +SEL_REPLACED+, or +SEL_DELETED+.
  # In each of these cases, the index to the item, if any, is passed in the
  # 3rd argument of the message.
  #
  # === Events
  #
  # The following messages are sent by FXIconList to its target:
  #
  # +SEL_CHANGED+::		sent when the current list item changes; the message data is an Integer indicating the index of the current item.
  # +SEL_COMMAND+::		sent when the current list item changes; the message data is an Integer indicating the index of the current item.
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  # +SEL_CLICKED+::		sent when a list item is single-clicked; the message data is an Integer indicating the index of the current item.
  # +SEL_DOUBLECLICKED+::	sent when a list item is double-clicked; the message data is an Integer indicating the index of the current item.
  # +SEL_TRIPLECLICKED+::	sent when a list item is triple-clicked; the message data is an Integer indicating the index of the current item.
  # +SEL_SELECTED+::		sent when a list item is selected; the message data is an Integer indicating the index of the selected item.
  # +SEL_DESELECTED+::		sent when a list item is deselected; the message data is an Integer indicating the index of the deselected item.
  # +SEL_REPLACED+::		sent when a list item is about to be replaced; the message data is an Integer indicating the index of the item to be replaced.
  # +SEL_INSERTED+::		sent after a list item is inserted; the message data is an Integer indicating the index of the item that was inserted.
  # +SEL_DELETED+::		sent when a list item is about to be removed; the message data is an Integer indicating the index of the item to be removed.
  #
  # === Icon list styles
  #
  # +ICONLIST_EXTENDEDSELECT+::		Extended selection mode
  # +ICONLIST_SINGLESELECT+::		At most one selected item
  # +ICONLIST_BROWSESELECT+::		Always exactly one selected item
  # +ICONLIST_MULTIPLESELECT+::		Multiple selection mode
  # +ICONLIST_AUTOSIZE+::		Automatically size item spacing
  # +ICONLIST_DETAILED+::		List mode
  # +ICONLIST_MINI_ICONS+::		Mini Icon mode
  # +ICONLIST_BIG_ICONS+::		Big Icon mode
  # +ICONLIST_ROWS+::			Row-wise mode
  # +ICONLIST_COLUMNS+::		Column-wise mode
  # +ICONLIST_NORMAL+::			same as +ICONLIST_EXTENDEDSELECT+
  #
  # === Message identifiers
  #
  # +ID_SHOW_DETAILS+::		x
  # +ID_SHOW_MINI_ICONS+::	x
  # +ID_SHOW_BIG_ICONS+::	x
  # +ID_ARRANGE_BY_ROWS+::	x
  # +ID_ARRANGE_BY_COLUMNS+::	x
  # +ID_HEADER_CHANGE+::	x
  # +ID_TIPTIMER+::		x
  # +ID_LOOKUPTIMER+::		x
  # +ID_SELECT_ALL+::		x
  # +ID_DESELECT_ALL+::		x
  # +ID_SELECT_INVERSE+::	x
  
  class FXIconList < FXScrollArea

    # Number of items [Integer]
    attr_reader	:numItems
    
    # Number of rows [Integer]
    attr_reader	:numRows
    
    # Number of columns [Integer]
    attr_reader	:numCols
    
    # The header control [FXHeader]
    attr_reader	:header
    
    # The number of header items in the header control [Integer]
    attr_reader	:numHeaders
    
    # Item width [Integer]
    attr_reader	:itemWidth
    
    # Item height [Integer]
    attr_reader	:itemHeight
    
    # Index of current item, or -1 if none [Integer]
    attr_accessor :currentItem
    
    # Index of anchor item, or -1 if none [Integer]
    attr_accessor :anchorItem
    
    # Index of item under the cursor, or -1 if none [Integer]
    attr_reader	:cursorItem
    
    # Text font [FXFont]
    attr_accessor :font
    
    # Normal text color [FXColor]
    attr_accessor :textColor
    
    # Background color for selected item(s) [FXColor]
    attr_accessor :selBackColor
    
    # Text color for selected item(s) [FXColor]
    attr_accessor :selTextColor
    
    # Maximum item space (in pixels) for each item [Integer]
    attr_accessor :itemSpace
    
    # Icon list style [Integer]
    attr_accessor :listStyle
    
    # Status line help text [String]
    attr_accessor :helpText

    # Construct icon list with no items in it initially
    def initialize(p, target=nil, selector=0, opts=ICONLIST_NORMAL, x=0, y=0, width=0, height=0) # :yields: theIconList
    end

    # Set headers from an array of strings.
    def setHeaders(strings, size=1); end

    # Append header with given _text_ and optional _icon_.
    def appendHeader(text, icon=nil, size=1); end
    
    # Remove header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def removeHeader(headerIndex); end
    
    # Change text of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def setHeaderText(headerIndex, text); end
    
    # Return text of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def getHeaderText(headerIndex); end
  
    # Change icon of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def setHeaderIcon(headerIndex, icon); end
  
    # Return icon of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def getHeaderIcon(headerIndex); end
  
    # Change size of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def setHeaderSize(headerIndex, size); end
    
    # Return size of header at _headerIndex_.
    # Raises IndexError if _headerIndex_ is out of bounds.
    def getHeaderSize(headerIndex); end
  
    # Return the item at the given _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItem(itemIndex); end
  
    # Replace the item at _index_ with a (possibly subclassed) _item_.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the list's message target
    # before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    def setItem(index, item, notify=false); end

    # Replace item _text_, _bigIcon_, _miniIcon_ and user _data_ for the item at _index_.
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the list's message target
    # before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    def setItem(index, text, bigIcon=nil, miniIcon=nil, data=nil, notify=false); end

    #
    # Fill list by appending items from array of strings, and return the number
    # of items appended.
    #
    def fillItems(strings, big=nil, mini=nil, data=nil, notify=false); end
  
    # Insert a new (possibly subclassed) _item_ at the given _index_.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    def insertItem(index, item, notify=false); end
  
    # Insert item at _index_ with given _text_, _bigIcon_, _miniIcon_ and user _data_.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    def insertItem(index, text, bigIcon=nil, miniIcon=nil, data=nil, notify=false); end
  
    # Append a new (possibly subclassed) _item_ to the end of the list.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is appended.
    def appendItem(item, notify=false); end
  
    # Append a new item with given _text_ and optional _bigIcon_, _miniIcon_ and user _data_.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is appended.
    def appendItem(text, bigIcon=nil, miniIcon=nil, data=nil, notify=false); end

    # Prepend a new (possibly subclassed) _item_ to the beginning of the list.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is prepended.
    def prependItem(item, notify=false); end
  
    # Prepend a new item with given _text_ and optional _bigIcon_, _miniIcon_ and user _data_.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is prepended.
    def prependItem(text, bigIcon=nil, miniIcon=nil, data=nil, notify=false); end

    #
    # Move item from _oldIndex_ to _newIndex_ and return the new index of the
    # item..
    # If _notify_ is +true+ and this move causes the current item to change, a
    # +SEL_CHANGED+ message is sent to the list's message target to indicate this
    # change in the current item.
    # Raises IndexError if either _oldIndex_ or _newIndex_ is out of bounds.
    #
    def moveItem(newIndex, oldIndex, notify=false); end

    #
    # Extract item from list and return a reference to the item.
    # If _notify_ is  +true+, a +SEL_DELETED+ message is sent to the list's
    # message target before the item is extracted from the list.
    # Raises IndexError if _index_ is out of bounds.
    #
    def extractItem(index, notify=false); end

    # Remove item at _index_ from the list.
    # If _notify_ is  +true+, a +SEL_DELETED+ message is sent to the list's message target
    # before the item is removed.
    # Raises IndexError if _index_ is out of bounds.
    def removeItem(index, notify=false); end
  
    # Remove all items from list.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the list's message target
    # before each item is removed.
    def clearItems(notify=false); end
  
    # Return index of item at (_x_, _y_), or -1 if none
    def getItemAt(x, y); end
  
    #
    # Search items by _text_, beginning from item _start_.  If the start
    # item is -1 the search will start at the first item in the list.
    # Flags may be SEARCH_FORWARD or SEARCH_BACKWARD to control the
    # search direction; this can be combined with SEARCH_NOWRAP or SEARCH_WRAP
    # to control whether the search wraps at the start or end of the list.
    # The option SEARCH_IGNORECASE causes a case-insensitive match.  Finally,
    # passing SEARCH_PREFIX causes searching for a prefix of the item name.
    # Return -1 if no matching item is found.
    #
    def findItem(text, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end
  
    #
    # Search items by associated user _data_, beginning from item _start_. If the
    # start item is -1 the search will start at the first item in the list.
    # Flags may be SEARCH_FORWARD or SEARCH_BACKWARD to control the
    # search direction; this can be combined with SEARCH_NOWRAP or SEARCH_WRAP
    # to control whether the search wraps at the start or end of the list.
    #
    def findItemByData(data, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Scroll to bring item into view. The argument is either a reference to
    # an FXIconItem instance, or the integer index of an item in the list.
    # For the latter case, #makeItemVisible raises IndexError if the index
    # is out of bounds.
    #
    def makeItemVisible(itemOrIndex); end
    
    # Change text for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemText(index, text); end
    
    # Return text for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemText(index); end
    
    # Change big icon for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemBigIcon(index, bigIcon, owned=false); end
    
    # Return big icon for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemBigIcon(index); end
    
    # Change mini icon for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemMiniIcon(index, miniIcon, owned=false); end
    
    # Return mini icon for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemMiniIcon(index); end
  
    # Change user _data_ for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemData(index, data);
  
    # Return user data for item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemData(index); end
  
    # Return +true+ if item at _index_ is selected.
    # Raises IndexError if _index_ is out of bounds.
    def itemSelected?(index); end
    
    # Return +true+ if item at _index_ is the current item.
    # Raises IndexError if _index_ is out of bounds.
    def itemCurrent?(index); end
    
    # Return +true+ if item at _index_ is visible.
    # Raises IndexError if _index_ is out of bounds.
    def itemVisible?(index); end
    
    # Return +true+ if item at _index_ is enabled.
    # Raises IndexError if _index_ is out of bounds.
    def itemEnabled?(index); end
    
    # Return item hit code: 0 outside, 1 icon, 2 text.
    # Raises IndexError if _index_ is out of bounds.
    def hitItem(index, x, y, ww=1, hh=1); end
  
    # Repaint item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def updateItem(index); end
    
    # Select items in rectangle.
    # If _notify_ is +true+, a +SEL_SELECTED+ message is sent to the list's
    # message target after each previously unselected item is selected.
    def selectInRectangle(x, y, w, h, notify=false); end
  
    # Enable item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def enableItem(index);
    
    # Disable item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def disableItem(index);
    
    # Select item at _index_.
    # If _notify_ is +true+, a +SEL_SELECTED+ message is sent to the list's
    # message target after the item is selected.
    # Raises IndexError if _index_ is out of bounds.
    def selectItem(index, notify=false); end
    
    # Deselect item at _index_.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the list's
    # message target after the item is deselected.
    # Raises IndexError if _index_ is out of bounds.
    def deselectItem(index, notify=false); end
  
    # Toggle item at _index_.
    # If _notify_ is  +true+, either a +SEL_SELECTED+ or +SEL_DESELECTED+
    # message is sent to the list's message target to indicate the item's
    # new state.
    # Raises IndexError if _index_ is out of bounds.
    def toggleItem(index, notify=false); end
    
    # Change current item index.
    # If _notify_ is  +true+, a +SEL_CHANGED+ message is sent to the list's message target
    # after the current item changes.
    # Raises IndexError if _index_ is out of bounds.
    def setCurrentItem(index, notify=false); end
    
    # Extend selection from anchor index to _index_.
    # If _notify_ is  +true+, a series of +SEL_SELECTED+ and +SEL_DESELECTED+ messages
    # are sent to the list's message target as the selected-state of different items changes.
    # Raises IndexError if _index_ is out of bounds.
    def extendSelection(index, notify=false); end
    
    # Deselect all items.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the list's message
    # target for all the items that were selected before killSelection was called.
    def killSelection(notify=false); end
  
    # Sort items
    def sortItems(); end
  end
end

