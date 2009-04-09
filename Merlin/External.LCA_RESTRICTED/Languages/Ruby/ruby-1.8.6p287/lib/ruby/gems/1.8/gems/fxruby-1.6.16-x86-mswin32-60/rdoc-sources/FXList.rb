module Fox
  #
  # List item
  #
  class FXListItem < FXObject

    # Text for this item [String]
    attr_accessor :text
    
    # Icon for this item [FXIcon]
    attr_accessor :icon
    
    # User data for this item [Object]
    attr_accessor :data
    
    # Indicates whether this item has the focus [Boolean]
    attr_writer :focus
    
    # Indicates whether this item is selected [Boolean]
    attr_writer :selected
    
    # Indicates whether this item is enabled [Boolean]
    attr_writer :enabled

    # Indicates whether this item is draggable [Boolean]
    attr_writer :draggable
    
    # Initialize
    def initialize(text, icon=nil, data=nil) # :yields: theListItem
    end
    
    # Return the list item's text
    def to_s; text; end
    
    # Returns +true+ if this item has the focus
    def hasFocus?() ; end
    
    # Return +true+ if this item is selected
    def selected?() ; end
    
    # Return +true+ if this item is enabled
    def enabled?() ; end
    
    # Return +true+ if this item is draggable
    def draggable?() ; end
    
    # Return the width of this item for a specified list
    def getWidth(list) ; end
    
    # Return the height of this item for a specified list
    def getHeight(list) ; end
    
    # Create the item
    def create() ; end
    
    # Detach the item
    def detach() ; end
    
    # Destroy the item
    def destroy( ); end
  end
  
  #
  # A List Widget displays a list of items, each with a text and
  # optional icon.  When an item's selected state changes, the list sends
  # a +SEL_SELECTED+ or +SEL_DESELECTED+ message.  A change of the current
  # item is signified by the +SEL_CHANGED+ message.
  # The list sends +SEL_COMMAND+ messages when the user clicks on an item,
  # and +SEL_CLICKED+, +SEL_DOUBLECLICKED+, and +SEL_TRIPLECLICKED+ when the user
  # clicks once, twice, or thrice, respectively.
  # When items are added, replaced, or removed, the list sends messages of
  # the type +SEL_INSERTED+, +SEL_REPLACED+, or +SEL_DELETED+.
  # In each of these cases, the index to the item, if any, is passed in the
  # 3rd argument of the message.
  #
  # === Events
  #
  # The following messages are sent by FXList to its target:
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
  # === List styles
  #
  # +LIST_EXTENDEDSELECT+::	Extended selection mode allows for drag-selection of ranges of items
  # +LIST_SINGLESELECT+::	Single selection mode allows up to one item to be selected
  # +LIST_BROWSESELECT+::	Browse selection mode enforces one single item to be selected at all times
  # +LIST_MULTIPLESELECT+::	Multiple selection mode is used for selection of individual items
  # +LIST_AUTOSELECT+::		Automatically select under cursor
  # +LIST_NORMAL+::		same as +LIST_EXTENDEDSELECT+
  #
  # === Message identifiers
  #
  # +ID_TIPTIMER+::
  # +ID_LOOKUPTIMER+::
  #
  class FXList < FXScrollArea

    # Number of items in the list [Integer]
    attr_reader	:numItems
    
    # Number of visible items [Integer]
    attr_accessor :numVisible
    
    # Index of current item, or -1 if no current item [Integer]
    attr_accessor :currentItem
    
    # Index of anchor item, or -1 if no anchor item [Integer]
    attr_reader	:anchorItem
    
    # Index of item under the cursor, or -1 if none [Integer]
    attr_reader	:cursorItem
    
    # Text font [FXFont]
    attr_accessor :font
    
    # Normal text color [FXColor]
    attr_accessor :textColor
    
    # Selected text background color [FXColor]
    attr_accessor :selBackColor
    
    # Selected text color [FXColor]
    attr_accessor :selTextColor
    
    # List style [Integer]
    attr_accessor :listStyle
    
    # Status line help text [String]
    attr_accessor :helpText

    # Construct a list with initially no items in it.
    def initialize(p, target=nil, selector=0, opts=LIST_NORMAL, x=0, y=0, width=0, height=0) # :yields: theList
    end

    # Return the item at the given _index_; returns a reference to an FXListItem instance.
    # Raises IndexError if _index_ is out of bounds.
    def getItem(index) ; end

    # Replace the item at _index_ with a (possibly subclassed) _item_, e.g.
    #
    #   list.setItem(0, FXListItem.new("inky"))
    #
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the list's message target
    # before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    # Returns the integer index of the replaced item.
    def setItem(index, item, notify=false) ; end

    # Replace the _text_, _icon_, and user _data_ for the item at _index_, e.g.
    #
    #   list.setItem(0, "inky")
    #
    # If _notify_ is +true+, a +SEL_REPLACED+ message is sent to the list's message target
    # before the item is replaced.
    # Raises IndexError if _index_ is out of bounds.
    # Returns the integer index of the replaced item.
    def setItem(index, text, icon=nil, data=nil, notify=false) ; end

    #
    # Fill list by appending items from array of strings, and return the number
    # items added.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's
    # message target after the item is added.
    #
    def fillItems(strings, icon=nil, ptr=nil, notify=false); end

    # Insert a new (possibly subclassed) _item_ at the given _index_, e.g.
    #
    #   list.insertItem(1, FXListItem.new("blinky"))
    #
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    # Returns the integer index of the inserted item.
    def insertItem(index, item, notify=false) ; end

    # Insert item at _index_ with given _text_, _icon_, and user _data_, e.g.
    #
    #   list.insertItem(1, "blinky")
    #
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is inserted.
    # Raises IndexError if _index_ is out of bounds.
    # Returns the integer index of the inserted item.
    def insertItem(index, text, icon=nil, data=nil, notify=false) ; end

    # Append a (possibly subclassed) _item_ to the list, e.g.
    #
    #   list.appendItem(FXListItem.new("pinky"))
    #
    # If _notify_ is  +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is appended.
    # Returns the integer index of the newly appended item.
    def appendItem(item, notify=false) ; end

    # Append a new item with given _text_ and optional _icon_ and user _data_, e.g.
    #
    #   list.appendItem("pinky")
    #
    # If _notify_ is  +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is appended.
    # Returns the integer index of the newly appended item.
    def appendItem(text, icon=nil, data=nil, notify=false) ; end

    # Prepend a (possibly subclassed) _item_ to the list, e.g.
    #
    #   list.prependItem(FXListItem.new("clyde"))
    #
    # If _notify_ is  +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is prepended.
    # Returns the integer index of the newly prepended item (which should
    # always be zero, by definition).
    def prependItem(item, notify=false) ; end

    # Prepend a new item with given _text_ and optional _icon_ and user _data_, e.g.
    #
    #   list.prependItem("clyde")
    #
    # If _notify_ is  +true+, a +SEL_INSERTED+ message is sent to the list's message target
    # after the item is prepended.
    # Returns the integer index of the newly prepended item (which should
    # always be zero, by definition).
    def prependItem(text, icon=nil, data=nil, notify=false) ; end

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

    # Remove item at _index_ from list.
    # If _notify_ is  +true+, a +SEL_DELETED+ message is sent to the list's message target
    # before the item is removed.
    # Raises IndexError if _index_ is out of bounds.
    def removeItem(index, notify=false) ; end

    # Remove all items from the list
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the list's message target
    # before each item is removed.
    def clearItems(notify=false) ; end

    # Return width of item at _index_. Raises IndexError if _index_ is out of bounds.
    def getItemWidth(index) ; end

    # Return height of item at _index_. Raises IndexError if _index_ is out of bounds.
    def getItemHeight(index) ; end

    # Return index of item at (_x_, _y_), if any
    def getItemAt(x, y) ; end

    # Return item hit code: 0 no hit; 1 hit the icon; 2 hit the text
    def hitItem(index, x, y) ; end

    #
    # Search items by _text_, beginning from item _start_.  If the start
    # item is -1 the search will start at the first item in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the
    # search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # The option +SEARCH_IGNORECASE+ causes a case-insensitive match.  Finally,
    # passing +SEARCH_PREFIX+ causes searching for a prefix of the item name.
    # Return -1 if no matching item is found.
    #
    def findItem(text, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP) ; end

    #
    # Search items by associated user _data_, beginning from item _start_.
    # Returns the integer index of the matching item, or -1 if no match is
    # found. If the start item is -1 the search will start at the first item in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the
    # search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    #
    def findItemByData(data, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Scroll to bring item into view. The argument is either a reference to
    # an FXListItem instance, or the integer index of an item in the list.
    # For the latter case, #makeItemVisible raises IndexError if the index
    # is out of bounds.
    #
    def makeItemVisible(itemOrIndex) ; end

    #
    # Change item text and mark the list's layout as dirty; this is
    # equivalent to:
    #
    #   getItem(index).text = text
    #   recalc
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemText(index, text) ; end

    #
    # Return item text; this is equivalent to:
    #
    #   getItem(index).text
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemText(index) ; end

    #
    # Change item icon and mark the list's layout as dirty; this is equivalent to:
    #
    #   getItem(index).icon = icon
    #   recalc
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemIcon(index, icon, owned=false) ; end

    #
    # Return item icon, if any. This is equivalent to:
    #
    #   getItem(index).icon
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemIcon(index) ; end

    #
    # Change item user data; this is equivalent to:
    #
    #   getItem(index).data = data
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemData(index, data) ; end

    #
    # Return item user data; this is equivalent to:
    #
    #   getItem(index).data
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemData(index) ; end

    #
    # Return +true+ if item is selected; this is equivalent to:
    #
    #   getItem(index).selected?
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemSelected?(index) ; end

    # Return +true+ if item is current. Raises IndexError if _index_ is out of bounds.
    def itemCurrent?(index) ; end

    # Return +true+ if item is visible. Raises IndexError if _index_ is out of bounds.
    def itemVisible?(index) ; end

    #
    # Return +true+ if item is enabled; this is equivalent to:
    #
    #   getItem(index).enabled?
    #
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemEnabled?(index) ; end

    # Repaint item. Raises IndexError if _index_ is out of bounds.
    def updateItem(index) ; end

    #
    # Enable item. Raises IndexError if _index_ is out of bounds.
    #
    def enableItem(index) ; end

    #
    # Disable item. Raises IndexError if _index_ is out of bounds.
    #
    def disableItem(index) ; end

    # Select item.
    # If _notify_ is  +true+, a +SEL_SELECTED+ message is sent to the list's message target
    # after the item is selected.
    # Raises IndexError if _index_ is out of bounds.
    def selectItem(index, notify=false) ; end

    # Deselect item.
    # If _notify_ is  +true+, a +SEL_DESELECTED+ message is sent to the list's message target
    # after the item is deselected.
    # Raises IndexError if _index_ is out of bounds.
    def deselectItem(index, notify=false) ; end

    # Toggle item selection state.
    # If _notify_ is  +true+, either a +SEL_SELECTED+ or +SEL_DESELECTED+ message is sent to the list's message target
    # to indicate the item's new state.
    # Raises IndexError if _index_ is out of bounds.
    def toggleItem(index, notify=false) ; end

    # Change current item.
    # If _notify_ is  +true+, a +SEL_CHANGED+ message is sent to the list's message target
    # after the current item changes.
    # Raises IndexError if _index_ is out of bounds.
    def setCurrentItem(index, notify=false) ; end

    # Extend selection from anchor item to _index_.
    # If _notify_ is  +true+, a series of +SEL_SELECTED+ and +SEL_DESELECTED+ messages
    # are sent to the list's message target as the selected-state of different items changes.
    # Raises IndexError if _index_ is out of bounds.
    def extendSelection(index, notify=false) ; end

    # Deselect all items.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the list's message
    # target for all the items that were selected before killSelection was called.
    def killSelection(notify=false) ; end

    # Sort items using current sort function
    def sortItems() ; end
  end
end
