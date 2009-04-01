module Fox
  #
  # The List Box is a control to select one of a list of options.  It looks
  # similar to a Combo Box except that List Box yields integer numbers only.
  # When an option is selected, List Box will send an SEL_COMMAND with the
  # index of the opton.  While manipulating the list, it may send SEL_CHANGED
  # messages to indicate which option the cursor is hovering over.
  # The List Box is able to receive ID_GETINTVALUE and ID_SETINTVALUE which
  # will retrieve the current option or change the selected option.
  #
  # === Events
  #
  # The following messages are sent by FXListBox to its target:
  #
  # +SEL_COMMAND+::
  #   sent when a new list item is clicked; the message data is the index of the selected item.
  # +SEL_CHANGED+::
  #   sent when a new list item is clicked.
  #
  # === List Box styles
  #
  # +LISTBOX_NORMAL+::		Normal style
  #
  # === Message identifiers
  #
  # +ID_LIST+
  # +ID_FIELD+
  #
  class FXListBox < FXPacker

    # Number of items in the list [Integer]
    attr_reader :numItems

    # Number of visible items [Integer]
    attr_accessor :numVisible

    # Current item's index, or -1 if no current item [Integer]
    attr_accessor :currentItem
    
    # Text font [FXFont]
    attr_accessor :font
  
    # Background color [FXColor]
    attr_reader :backColor
  
    # Text color [FXColor]
    attr_accessor :textColor
    
    # Background color for selected items [FXColor]
    attr_accessor :selBackColor
    
    # Text color for selected items [FXColor]
    attr_accessor :selTextColor
  
    # Status line help text [String]
    attr_accessor :helpText
  
    # Tool tip message [String]
    attr_accessor :tipText

    #
    # Returns an initialized FXListBox instance.
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_SUNKEN|FRAME_THICK|LISTBOX_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theListBox
    end

    #
    # Return +true+ if _index_ is the index of the current item.
    # Raises IndexError if _index_ is out of bounds.
    #
    def itemCurrent?(index); end

    #
    # Return the text of the item at the given index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def retrieveItem(index); end

    #
    # Replace the item at _index_ with a new item with the specified _text_,
    # _icon_ and _data_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItem(index, text, icon=nil, ptr=nil); end
  
    #
    # Fill list by appending items from array of strings, and return the number
    # items added.
    #
    def fillItems(strings, icon=nil, ptr=nil); end

    #
    # Insert a new item at index.
    # Raises IndexError if _index_ is out of bounds.
    #
    def insertItem(index, text, icon=nil, ptr=nil); end
  
    # Add an item to the end of the list.
    def appendItem(text, icon=nil, ptr=nil);
  
    # Prepend an item to the list
    def prependItem(text, icon=nil, ptr=nil); end
  
    #
    # Move item from _oldIndex_ to _newIndex_ and return the new
    # index of the item.
    # Raises IndexError if either _oldIndex_ or _newIndex_ is out of bounds.
    #
    def moveItem(newIndex, oldIndex); end

    #
    # Extract item from list and return a reference to the item.
    # Raises IndexError if _index_ is out of bounds.
    #
    def extractItem(index); end

    #
    # Remove this item from the list.
    # Raises IndexError if _index_ is out of bounds.
    #
    def removeItem(index); end
  
    # Remove all items from the list
    def clearItems(); end
  
    #
    # Search items by _text_, beginning from item _start_. If the start
    # item is -1 the search will start at the first item in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the
    # search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # The option +SEARCH_IGNORECASE+ causes a case-insensitive match.  Finally,
    # passing +SEARCH_PREFIX+ causes searching for a prefix of the item text.
    # Return -1 if no matching item is found.
    #
    def findItem(text, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end
  
    #
    # Search items by associated user _data_, beginning from item _start_. If the
    # start item is -1 the search will start at the first item in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the
    # search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    #
    def findItemByData(data, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Set text for specified item to _text_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemText(index, text); end
  
    #
    # Return text for specified item.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemText(index); end
  
    #
    # Set icon for specified item to _icon_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemIcon(index, icon, owned=false); end
    
    #
    # Return icon for specified item.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemIcon(index); end
    
    #
    # Set user data object for specified item to _ptr_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemData(index, ptr); end

    #
    # Return user data object for specified item.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemData(index); end
  
    # Return +true+ if the pane is shown.
    def paneShown?; end
  
    # Sort items using current sort function
    def sortItems; end
  
    alias appendItem <<
  end
end
