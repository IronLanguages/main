module Fox
  # Color item
  class FXColorItem < FXListItem
    # Item color [FXColor]
    attr_accessor :color
    
    # Return a new color item, initialized with the given text, color and
    # user data.
    def initialize(text, clr, data=nil) # :yields: theColorItem
    end
  end
  
  # Displays a list of colors
  class FXColorList < FXList

    #
    # Return an initially empty list of colors.
    #
    def initialize(p, target=nil, selector=0, opts=LIST_BROWSESELECT, x=0, y=0, width=0, height=0) # :yields: theColorList
    end

    #
    # Fill list by appending color items from array of strings and array of colors.
    #
    def fillItems(strings, colors=nil, ptr=nil, notify=false); end

    #
    # Insert item at index with given text, color, and user-data pointer
    #
    def insertItem(index, text, color=0, ptr=nil, notify=false); end

    #
    # Append new item with given text, color, and user-data pointer
    #
    def appendItem(text, color=0, ptr=nil, notify=false); end

    #
    # Prepend new item with given text, color, and user-data pointer
    #
    def prependItem(text, color=0, ptr=nil, notify=false); end

    #
    # Change item color for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def setItemColor(index, color); end

    #
    # Return item color for the item at _index_.
    # Raises IndexError if _index_ is out of bounds.
    #
    def getItemColor(index); end
  end
end

