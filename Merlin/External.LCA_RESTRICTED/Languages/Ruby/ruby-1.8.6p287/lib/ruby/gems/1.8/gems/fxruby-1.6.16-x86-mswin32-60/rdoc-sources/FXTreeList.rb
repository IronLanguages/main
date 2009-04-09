module Fox
  #
  # Each item in an FXTreeList is an instance of FXTreeItem.
  #
  # A tree item can contain zero or more child items, and those items are arranged
  # as a linked list. The FXTreeItem#first method returns the a reference to the
  # first child item, if any, and the FXTreeItem#last method returns a reference to
  # the last child item.
  #
  class FXTreeItem < FXObject
  
    #
    # Return a new FXTreeItem instance, initialized with the specified text,
    # open-state icon, closed-state icon and user data.
    #
    def initialize(text, openIcon=nil, closedIcon=nil, data=nil) # :yields: theItem
    end
    
    # Return the number of child items for this tree item.
    def numChildren; end

    # Return the item text (a string) for this tree item.
    def text; end
    
    # Set the item text for this tree item.
    def text=(txt); end

    # Return a reference to the opened-state icon (an FXIcon instance) for
    # this tree item, or +nil+ if none was specified.
    def openIcon; end
    
    # Set the opened-state icon (an FXIcon instance) for this tree item,
    # or +nil+ if no icon should be used.
    def setOpenIcon(oi, owned=false); end

    # Return a reference to the closed-state icon (an FXIcon instance) for
    # this tree item, or +nil+ if none was specified.
    def closedIcon; end
    
    # Set the closed-state icon (an FXIcon instance) for this tree item,
    # or +nil+ if no icon should be used.
    def setClosedIcon(ci, owned=false); end

    # Return a reference to the user data for this tree item, or +nil+
    # if no user data has been associated with this tree item.
    def data; end

    # Set the user data (a reference to any kind of object) for this tree item,
    # or +nil+ if no user data needs to be associated with this item.
    def data=(dt); end

    # Set the focus on this tree item (_focus_ is either +true+ or +false+)
    def setFocus(focus) ; end

    # Returns +true+ if this item has the focus
    def hasFocus? ; end
    
    # Set this item's selected state to +true+ or +false+.
    def selected=(sel); end

    # Returns +true+ if this item is selected
    def selected? ; end
    
    # Set this item's "opened" state to +true+ or +false+.
    def opened=(op); end

    # Returns +true+ if this item is opened
    def opened? ; end
    
    # Set this item's expanded state to +true+ or +false+.
    def expanded=(ex); end
    
    # Returns +true+ if this item is expanded
    def expanded? ; end

    # Set this item's enabled state to +true+ or +false+.
    def enabled=(en); end

    # Returns +true+ if this item is enabled
    def enabled? ; end

    # Set this item's "draggable" state to +true+ or +false+.
    def draggable=(dr); end
    
    # Returns +true+ if this item is draggable
    def draggable? ; end
    
    # Return +true+ if this items has subitems, real or imagined.
    def hasItems?; end
    
    # Change has items flag to +true+ or +false+.
    def hasItems=(flag); end
        
    # Return a reference to the parent item for this tree item, or +nil+
    # if this is a root-level item.
    def parent; end

    # Return a reference to the first child item for this tree item,
    # or +nil+ if this tree item has no child items.
    def first; end

    # Return a reference to the last child item for this tree item,
    # or +nil+ if this tree item has no child items.
    def last; end

    # Return a reference to the next sibling item for this tree item,
    # or +nil+ if this is the last item in the parent item's list of
    # child items.
    def next; end

    # Return a reference to the previous sibling item for this tree item,
    # or +nil+ if this is the first item in the parent item's list of
    # child items.
    def prev; end

    # Return a reference to the item that is "logically" below this item.
    def below; end

    # Return a reference to the item that is "logically" above this item.
    def above; end

    #
    # Return +true+ if this item is a descendant of _item_.
    #
    def childOf?(item); end

    #
    # Return +true+ if this item is an ancestor of _item_.
    #
    def parentOf?(item); end

    # Returns the item's text
    def to_s
      text
    end
    
    # Get the width of this item
    def getWidth(treeList) ; end
    
    # Get the height of this item
    def getHeight(treeList) ; end
    
    # Create this tree item
    def create; end

    # Detach this tree item
    def detach; end

    # Destroy this tree item
    def destroy; end
  end

  # 
  # A Tree List Widget organizes items in a hierarchical, tree-like fashion.
  # Subtrees can be collapsed or expanded by double-clicking on an item
  # or by clicking on the optional plus button in front of the item.
  # Each item may have a text and optional open-icon as well as a closed-icon.
  # The items may be connected by optional lines to show the hierarchical
  # relationship.
  # When an item's selected state changes, the treelist emits a SEL_SELECTED
  # or SEL_DESELECTED message.  If an item is opened or closed, a message
  # of type SEL_OPENED or SEL_CLOSED is sent.  When the subtree under an
  # item is expanded, a SEL_EXPANDED or SEL_COLLAPSED message is issued.
  # A change of the current item is signified by the SEL_CHANGED message.
  # In addition, the tree list sends SEL_COMMAND messages when the user
  # clicks on an item, and SEL_CLICKED, SEL_DOUBLECLICKED, and SEL_TRIPLECLICKED
  # when the user clicks once, twice, or thrice, respectively.
  # When items are added or removed, the tree list sends messages of the
  # type SEL_INSERTED or SEL_DELETED.
  # In each of these cases, a pointer to the item, if any, is passed in the
  # 3rd argument of the message.
  #
  # === Events
  #
  # The following messages are sent by FXTreeList to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::		sent when a list item is clicked on; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_CLICKED+::		sent when the left mouse button is single-clicked in the list; the message data is a reference to the item clicked (an FXTreeItem instance) or +nil+ if no item was clicked.
  # +SEL_DOUBLECLICKED+::	sent when the left mouse button is double-clicked in the list; the message data is a reference to the item clicked (an FXTreeItem instance) or +nil+ if no item was clicked.
  # +SEL_TRIPLECLICKED+::	sent when the left mouse button is triple-clicked in the list; the message data is a reference to the item clicked (an FXTreeItem instance) or +nil+ if no item was clicked.
  # +SEL_OPENED+::		sent when an item is opened; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_CLOSED+::		sent when an item is closed; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_EXPANDED+::		sent when a sub-tree is expanded; the message data is a reference to the root item for the sub-tree (an FXTreeItem instance).
  # +SEL_COLLAPSED+::		sent when a sub-tree is collapsed; the message data is a reference to the root item for the sub-tree (an FXTreeItem instance).
  # +SEL_SELECTED+::		sent when an item is selected; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_DESELECTED+::		sent when an item is deselected; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_CHANGED+::		sent when the current item changes; the message data is a reference to the current item (an FXTreeItem instance).
  # +SEL_INSERTED+::		sent after an item is added to the list; the message data is a reference to the item (an FXTreeItem instance).
  # +SEL_DELETED+::		sent before an item is removed from the list; the message data is a reference to the item (an FXTreeItem instance).
  #
  # === Tree list styles
  #
  # +TREELIST_EXTENDEDSELECT+::		Extended selection mode allows for drag-selection of ranges of items
  # +TREELIST_SINGLESELECT+::		Single selection mode allows up to one item to be selected
  # +TREELIST_BROWSESELECT+::		Browse selection mode enforces one single item to be selected at all times
  # +TREELIST_MULTIPLESELECT+::		Multiple selection mode is used for selection of individual items
  # +TREELIST_AUTOSELECT+::		Automatically select under cursor
  # +TREELIST_SHOWS_LINES+::		Lines shown
  # +TREELIST_SHOWS_BOXES+::		Boxes to expand shown
  # +TREELIST_ROOT_BOXES+::		Display root boxes also
  # +TREELIST_NORMAL+::			same as +TREELIST_EXTENDEDLIST+

  class FXTreeList < FXScrollArea

    # Number of items [Integer]
    attr_reader		:numItems

    # Number of visible items [Integer]
    attr_accessor	:numVisible

    # First root-level item [FXTreeItem]
    attr_reader		:firstItem

    # Last root-level item [FXTreeItem]
    attr_reader		:lastItem

    # Current item, if any [FXTreeItem]
    attr_accessor	:currentItem

    # Anchor item, if any [FXTreeItem]
    attr_accessor	:anchorItem

    # Item under the cursor, if any [FXTreeItem]
    attr_reader		:cursorItem

    # Text font [FXFont]
    attr_accessor	:font

    # Parent-child indent amount, in pixels [Integer]
    attr_accessor	:indent

    # Normal text color [FXColor]
    attr_accessor	:textColor

    # Selected text background color [FXColor]
    attr_accessor	:selBackColor

    # Selected text color [FXColor]
    attr_accessor	:selTextColor

    # Line color [FXColor]
    attr_accessor	:lineColor

    # List style [Integer]
    attr_accessor	:listStyle

    # Status line help text for this list [String]
    attr_accessor	:helpText

    #
    # Construct a new, initially empty tree list.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tree list [FXComposite]
    # +target+::	the message target, if any, for this tree list [FXObject]
    # +selector+::	the message identifier for this tree list [Integer]
    # +opts+::	tree list options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=TREELIST_NORMAL, x=0, y=0, width=0, height=0) # :yields: theTreeList
    end

    #
    # Fill tree list by appending items from array of strings and return the
    # number of items added.
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's
    # message target after each item is added.
    #
    def fillItems(father, strings, oi=nil, ci=nil, ptr=nil, notify=false); end

    # Insert a new (possibly subclassed) _item_ under _father_ before _other_ item
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def insertItem(other, father, item, notify=false); end
  
    # Insert item with given text and optional icons, and user-data pointer under _father_ before _other_ item. 
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def insertItem(other, father, text, oi=nil, ci=nil, ptr=nil, notify=false); end

    # Append a new (possibly subclassed) _item_ as last child of _father_. 
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def appendItem(father, item, notify=false); end
  
    # Append item with given text and optional icons, and user-data pointer as last child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def appendItem(father, text, oi=nil, ci=nil, ptr=nil, notify=false); end
  
    # Prepend a new (possibly subclassed) _item_ as first child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def prependItem(father, item, notify=false); end
  
    # Prepend item with given text and optional icons, and user-data pointer as first child of _father_.
    # Returns a reference to the newly added item (an FXTreeItem instance).
    # If _notify_ is +true+, a +SEL_INSERTED+ message is sent to the list's message
    # target after the item is added.
    def prependItem(father, text, oi=nil, ci=nil, ptr=nil, notify=false); end
  
    #
    # Move _item_ under _father_ before _other_ item and return a reference to
    # _item_.
    #
    def moveItem(other, father, item); end

    #
    # Extract item from list and return a reference to the item.
    # If _notify_ is  +true+ and the extraction causes the list's current item
    # to change, a +SEL_CHANGED+ message is sent to the list's message target
    # before the item is extracted from the list.
    #
    def extractItem(item, notify=false); end

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
  
    # Return item width
    def getItemWidth(item); end
  
    # Return item height
    def getItemHeight(item); end
    
    # Return a reference to the tree item at (_x_, _y_), if any.
    def getItemAt(x, y); end

    #
    # Search items by _text_, beginning from item _start_ (an FXTreeItem instance).
    # If the start item is +nil+, the search will start at the first, top-most
    # item in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the search
    # direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # The option +SEARCH_IGNORECASE+ causes a case-insensitive match. Finally,
    # passing +SEARCH_PREFIX+ causes searching for a prefix of the item name.
    # Return +nil+ if no matching item is found.
    #
    def findItem(text, start=nil, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Search items by associated user _data_, beginning from item _start_
    # (an FXTreeItem instance).
    # If the start item is +nil+ the search will start at the first, top-most item
    # in the list.
    # Flags may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+ to control the search
    # direction; this can be combined with +SEARCH_NOWRAP+ or +SEARCH_WRAP+
    # to control whether the search wraps at the start or end of the list.
    # Return +nil+ if no matching item is found.
    #
    def findItemByData(data, start=nil, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    # Scroll the list to make _item_ visible
    def makeItemVisible(item); end
  
    # Change item's text
    def setItemText(item, text); end
    
    # Return item's text
    def getItemText(item); end
  
    # Change item's open icon, deleting the old icon if it's owned
    def setItemOpenIcon(item, openIcon, owned=false); end
    
    # Return item's open icon
    def getItemOpenIcon(item); end
    
    # Change item's closed icon, deleting the old icon if it's owned
    def setItemClosedIcon(item, closedIcon, owned=false); end
    
    # Return item's closed icon
    def getItemClosedIcon(item); end
  
    # Change item's user data
    def setItemData(item, data); end
  
    # Return item's user data
    def getItemData(item); end
  
    # Return +true+ if item is selected
    def itemSelected?(item); end
    
    # Return +true+ if item is current
    def itemCurrent?(item); end

    # Return +true+ if item is visible
    def itemVisible?(item); end

    # Return +true+ if item opened
    def itemOpened?(item); end

    # Return +true+ if item expanded
    def itemExpanded?(item); end
  
    # Return +true+ if item is a leaf-item, i.e. has no children
    def itemLeaf?(item); end
  
    # Return +true+ if item is enabled
    def itemEnabled?(item); end

    # Return item hit code: 0 outside, 1 icon, 2 text, 3 box
    def hitItem(item, x, y); end

    # Repaint item
    def updateItem(item); end
  
    # Enable item
    def enableItem(item); end
  
    # Disable item
    def disableItem(item); end
  
    # Select item.
    # If _notify_ is +true+, a +SEL_SELECTED+ message is sent to the list's
    # message target after the item is selected.
    def selectItem(item, notify=false); end

    # Deselect item.
    # If _notify_ is +true+, a +SEL_DESELECTED+ message is sent to the list's
    # message target after the item is deselected.
    def deselectItem(item, notify=false); end
  
    # Toggle item selection.
    # If _notify_ is +true+, a +SEL_SELECTED+ or +SEL_DESELECTED+ message is
    # sent to the list's message target to indicate the change.
    def toggleItem(item, notify=false); end
  
    #
    # Set this item's state to opened. The primary result of this change is
    # that the item's icon will change to its "open" icon.
    # This is different from the #expandTree method, which actually
    # collapses part of the tree list, making some items invisible.
    # If _notify_ is +true+, a +SEL_OPENED+ message is sent to the list's
    # message target after the item is opened.
    #
    def openItem(item, notify=false); end
  
    #
    # Set this item's state to closed. The primary result of this change is
    # that the item's icon will change to its "closed" icon.
    # This is different from the #collapseTree method, which actually
    # collapses part of the tree list, making some items invisible.
    # If _notify_ is +true+, a +SEL_CLOSED+ message is sent to the list's
    # message target after the item is closed.
    #
    def closeItem(item, notify=false); end
  
    # Collapse sub-tree rooted at _tree_.
    # If _notify_ is +true+, a +SEL_COLLAPSED+ message is sent to the list's
    # message target after the sub-tree is collapsed.
    def collapseTree(tree, notify=false); end

    # Expand sub-tree rooted at _tree_.
    # If _notify_ is +true+, a +SEL_EXPANDED+ message is sent to the list's
    # message target after the sub-tree is expanded.
    def expandTree(tree, notify=false); end
  
    #
    # Change current item. If there is already a current item, that item
    # is first closed. After _item_ is set as the tree list's current item,
    # it is opened and selected.
    # If _notify_ is +true+, a +SEL_CHANGED+ message is sent to the list's
    # message target after the current item changes.
    #
    def setCurrentItem(item, notify=false); end
  
    # Extend selection from anchor item to _item_.
    # If _notify_ is +true+, a series of +SEL_SELECTED+ and +SEL_DESELECTED+
    # messages may be sent to the list's message target, indicating the changes.
    def extendSelection(item, notify=false); end
    
    # Deselect all items.
    # If _notify_ is +true+, +SEL_DESELECTED+ messages will be sent to the list's
    # message target indicating the affected items.
    def killSelection(notify=false); end
    
    # Sort root items.
    def sortRootItems(); end

    # Sort all items recursively.
    def sortItems(); end

    # Sort children of _item_
    def sortChildItems(item); end
  end
end

