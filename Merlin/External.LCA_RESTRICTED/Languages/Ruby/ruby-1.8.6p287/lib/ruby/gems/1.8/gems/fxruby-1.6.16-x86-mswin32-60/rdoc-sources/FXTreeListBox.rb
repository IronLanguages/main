module Fox
  #
  # The Tree List Box behaves very much like a List Box, except that
  # it supports a hierarchical, tree structured display of the items.
  # When an item is selected it issues a +SEL_COMMAND+ message with the
  # pointer to the item. While manipulating the tree list, it may send
  # +SEL_CHANGED+ messages to indicate which item the cursor is hovering over.
  #
  # === Events
  #
  # The following messages are sent by FXTreeListBox to its target:
  #
  # +SEL_CHANGED+::
  #   sent when the current list item changes; the message data is a reference to the new tree item.
  # +SEL_COMMAND+::
  #   sent when the current list item changes; the message data is a reference to the new tree item.
  #
  # === Tree list box styles
  #
  # +TREELISTBOX_NORMAL+::	Normal style
  #
  # === Message identifiers
  #
  # +ID_TREE+::		x
  # +ID_FIELD+::	x
  #
  class FXTreeListBox < FXPacker

    # Number of items [Integer]
    attr_reader :numItems

    # Number of visible items [Integer]
    attr_accessor :numVisible

    # First root-level item [FXTreeItem]
    attr_reader :firstItem

    # Last root-level item [FXTreeItem]
    attr_reader :lastItem

    # Current item, if any [FXTreeItem]
    attr_accessor :currentItem

    # Text font [FXFont]
    attr_accessor :font
    
    # Tree list box style
    attr_accessor :listStyle

    # Status line help text for this tree list box [String]
    attr_accessor :helpText

    # Tool tip text for this tree list box [String]
    attr_accessor :tipText

    #
    # Return an initially empty FXTreeListBox.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tree list box [FXComposite]
    # +target+::	the message target, if any, for this tree list box [FXObject]
    # +selector+::	the message identifier for this tree list box [Integer]
    # +opts+::	tree list options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_SUNKEN|FRAME_THICK|TREELISTBOX_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theTreeListBox
    end

    #
    # Fill tree list box by appending items from array of strings and return the
    # number of items added.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list box's
    # message target after each item is added.
    #
    def fillItems(father, strings, oi=nil, ci=nil, ptr=nil); end

    # Insert a new (possibly subclassed) _item_ under _father_ before _other_ item.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def insertItem(other, father, item, notify=false); end
  
    # Insert item with given text and optional icons, and user-data pointer under _father_ before _other_ item. 
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def insertItem(other, father, text, openIcon=nil, closedIcon=nil, data=nil, notify=false); end

    # Append a new (possibly subclassed) _item_ as last child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def appendItem(father, item, notify=false); end
  
    # Append item with given text and optional icons, and user-data pointer as last child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def appendItem(father, text, openIcon=nil, closedIcon=nil, data=nil, notify=false); end
  
    # Prepend a new (possibly subclassed) _item_ as first child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def prependItem(father, item, notify=false); end
  
    # Prepend item with given text and optional icons, and user-data pointer as first child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def prependItem(father, text, openIcon=nil, closedIcon=nil, data=nil, notify=false); end
  
    # Remove item.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the list's message
    # target before the item is removed.
    def removeItem(item, notify=false);

    # Remove items in range [_fromItem_, _toItem_] inclusively.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the list's message
    # target before each item is removed.
    def removeItems(fromItem, toItem, notify=false); end

    # Remove all items from the list.
    # If _notify_ is +true+, a +SEL_DELETED+ message is sent to the list's message
    # target before each item is removed.
    def clearItems(notify=false); end

    #
    # Move _item_ under _father_ before _other_ item and return a reference to
    # _item_.
    #
    def moveItem(other, father, item); end

    #
    # Extract item from list and return a reference to the item.
    #
    def extractItem(item); end

    #
    # Search items by _text_, beginning from item _start_.  If the
    # start item is +nil+ the search will start at the first, top-most item
    # in the list. Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control
    # the search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # The option +SEARCH_IGNORECASE+ causes a case-insensitive match.  Finally,
    # passing +SEARCH_PREFIX+ causes searching for a prefix of the item text.
    # Return +nil+ if no matching item is found.
    #
    def findItem(text, start=nil, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Search items by associated user _data_, beginning from item _start_.  If the
    # start item is +nil+ the search will start at the first, top-most item
    # in the list. Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control
    # the search direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # Return +nil+ if no matching item is found.
    #
    def findItemByData(data, start=nil, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    # Return +true+ if item is current
    def itemCurrent?(item); end

    # Return +true+ if item is a leaf-item, i.e. has no children
    def itemLeaf?(item); end

    # Sort root items
    def sortRootItems(); end

    # Sort all items recursively.
    def sortItems(); end

    # Sort children of _item_
    def sortChildItems(item); end

    #
    # Change current item.
    # If _notify_ is +true+, a SEL_CHANGED message is sent to the tree list box's
    # message target.
    #
    def setCurrentItem(item, notify=false); end
    
    # Change item's text
    def setItemText(item, text); end
    
    # Return item's text
    def getItemText(item); end
  
    # Change item's open icon
    def setItemOpenIcon(item, openIcon); end
    
    # Return item's open icon
    def getItemOpenIcon(item); end
    
    # Change item's closed icon
    def setItemClosedIcon(item, closedIcon); end
    
    # Return item's closed icon
    def getItemClosedIcon(item); end
  
    # Change item's user data
    def setItemData(item, data); end
  
    # Return item's user data
    def getItemData(item); end
  
    # Return +true+ if the pane is shown.
    def paneShown?; end
  end
end

