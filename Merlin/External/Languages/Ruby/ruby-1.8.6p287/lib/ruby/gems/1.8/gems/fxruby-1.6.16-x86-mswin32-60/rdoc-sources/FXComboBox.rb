module Fox
  #
  # An FXComboBox provides a way to select a string from a list of strings.
  # Unless +COMBOBOX_STATIC+ is passed, it also allows the user to enter a new
  # string into the text field, for example if the desired entry is not in the
  # list of strings.  Passing +COMBOBOX_REPLACE+, +COMBOBOX_INSERT_BEFORE+, +COMBOBOX_INSERT_AFTER+,
  # +COMBOBOX_INSERT_FIRST+, or +COMBOBOX_INSERT_LAST+ causes a newly entered text to replace the
  # current one in the list, or be added before or after the current entry, or to be added at
  # the beginning or end of the list.
  # FXComboBox is intended to enter text; if you need to enter a choice from a list of
  # options, it is recommended that the FXListBox widget is used instead.
  # When the text in the field is changed, a +SEL_COMMAND+ will be send to the target.
  # The FXComboBox can also receive +ID_GETSTRINGVALUE+ and +ID_SETSTRINGVALUE+ and so
  # on, which will behave similar to FXTextField in that they will retrieve or update
  # the value of the field.
  #
  # === Events
  #
  # The following messages are sent by FXComboBox to its target:
  #
  # +SEL_CHANGED+::		sent when the text in the text field changes; the message data is a String containing the new text.
  # +SEL_COMMAND+::		sent when a new item is selected from the list, or when a command message is sent from the text field; the message data is a String containing the new text.
  #
  # === ComboBox styles
  #
  # +COMBOBOX_NO_REPLACE+::     Leave the list the same
  # +COMBOBOX_REPLACE+::        Replace current item with typed text
  # +COMBOBOX_INSERT_BEFORE+::  Typed text inserted before current
  # +COMBOBOX_INSERT_AFTER+::   Typed text inserted after current
  # +COMBOBOX_INSERT_FIRST+::   Typed text inserted at begin of list
  # +COMBOBOX_INSERT_LAST+::    Typed text inserted at end of list
  # +COMBOBOX_STATIC+::         Unchangable text box
  # +COMBOBOX_NORMAL+::         Default options for comboboxes
  #
  # === Message identifiers
  #
  # +ID_LIST+::			identifier associated with the embedded FXList instance
  # +ID_TEXT+::			identifier associated with the embedded FXTextField instance
  #
  class FXComboBox < FXPacker

    # Editable state [Boolean]
    attr_writer	:editable
    
    # Text [String]
    attr_accessor :text
    
    # Number of columns [Integer]
    attr_accessor :numColumns
    
    # Text justification mode; default is +JUSTIFY_LEFT+ [Integer]
    attr_accessor :justify
    
    # Number of items in the list [Integer]
    attr_reader	:numItems
    
    # Number of visible items in the drop-down list [Integer]
    attr_accessor :numVisible
    
    # Index of current item, or -1 if no current item [Integer]
    attr_accessor :currentItem
    
    # Text font [FXFont]
    attr_accessor :font
    
    # Combo box style [Integer]
    attr_accessor :comboStyle
    
    # Window background color [FXColor]
    attr_accessor :backColor
    
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
    # Return an initialized FXComboBox instance, with room to display _cols_ columns of text.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this combo-box [FXComposite]
    # +cols+::	number of columns [Integer]
    # +target+::	message target [FXObject]
    # +selector+::	message identifier [Integer]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	left-side padding, in pixels [Integer]
    # +padRight+::	right-side padding, in pixels [Integer]
    # +padTop+::	top-side padding, in pixels [Integer]
    # +padBottom+::	bottom-side padding, in pixels [Integer]
    #
    def initialize(p, cols, target=nil, selector=0, opts=COMBOBOX_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theComboBox
    end

    # Return the combo box text
    def to_s; end
    
    # Return +true+ if combobox is editable
    def editable?() ; end

    # Return +true+ if the item at _index_ is the current item.
    # Raises IndexError if _index_ is out of bounds.
    def itemCurrent?(index) ; end

    # Return the text of the item at the given _index_.
    # Raises IndexError if _index_ is out of bounds.
    def retrieveItem(index) ; end

    # Replace the item at _index_ with a new item with the specified _text_ and user _data_.
    # Raises IndexError if _index_ is out of bounds.
    def setItem(index, text, data=nil) ; end

    #
    # Fill combo box by appending items from _strings_, where _strings_ is
    # an array of strings. Return the number of items added.
    #
    def fillItems(strings); end

    # Insert a new item at _index_, with the specified _text_ and user _data_.
    # Raises IndexError if _index_ is out of bounds.
    def insertItem(index, text, data=nil) ; end

    # Append a new item to the list with the specified _text_ and user _data_.
    def appendItem(text, data=nil) ; end

    # Prepend an item to the list with the specified _text_ and user _data_
    def prependItem(text, data=nil) ; end
    
    #
    # Move item from _oldIndex_ to _newIndex_ and return the new index of the item.
    # Raises IndexError if either _oldIndex_ or _newIndex_ is out of bounds.
    #
    def moveItem(newIndex, oldIndex); end

    # Remove the item at _index_ from the list.
    # Raises IndexError if _index_ is out of bounds.
    def removeItem(index) ; end

    # Remove all items from the list
    def clearItems() ; end

    #
    # Search for items by name, beginning from the item with index _start_.
    # If the start item is -1, the search will start at the first item in the
    # list. The search _flags_ may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+, to
    # control the search direction; this can be combined with +SEARCH_NOWRAP+ or
    # +SEARCH_WRAP+ to control whether the search wraps at the start or end of
    # the list.
    # The option +SEARCH_IGNORECASE+ causes a case-insensitive match.
    # Finally, passing +SEARCH_PREFIX+ causes searching for a prefix of the
    # item name.
    # Returns the index of the first matching item, or -1 if no matching item
    # is found.
    #
    def findItem(text, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    #
    # Search for items by associated user data, beginning from the item with
    # index _start_.
    # If the start item is -1, the search will start at the first item in the
    # list.
    # The search _flags_ may be +SEARCH_FORWARD+ or +SEARCH_BACKWARD+, to
    # control the search direction; this can be combined with +SEARCH_NOWRAP+ or
    # +SEARCH_WRAP+ to control whether the search wraps at the start or end of
    # the list.
    # Returns the index of the first matching item, or -1 if no matching item
    # is found.
    #
    def findItemByData(data, start=-1, flags=SEARCH_FORWARD|SEARCH_WRAP); end

    # Set text for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemText(index, text) ; end

    # Get text for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemText(index) ; end

    # Set user _data_ for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def setItemData(index, data) ; end

    # Get data pointer for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    def getItemData(index) ; end

    # Return +true+ if the pane is shown.
    def paneShown?() ; end

    # Sort items using current sort function
    def sortItems() ; end
    
    #
    # Set current item to _index_, where _index_ is the zero-based index of
    # the item. If _notify_ is +true+, a +SEL_COMMAND+ message is sent
    # to the combo box's message target.
    #
    def setCurrentItem(index, notify=false); end
  end
end
