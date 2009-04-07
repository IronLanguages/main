# Copyright (c) 2004, 2005 Martin Ankerl
require 'set'

# Packet_List is a more convenient wrapper for FXIconList. This class has to be used
# in combination with Packet_Item.
class Packet_List < FXIconList
  include Responder

  # Create a new Packet_List. All parameters are passed to the parental constructor FXIconList.
  def initialize(data, *args)
    @data = data
    @sort_mutex = Mutex.new
    @sort_thread = nil
    if FOXVERSION=="1.0"
      def getItem(num)
        retrieveItem(num)
      end
    end

    @header_item_index = 0
    @reversed = true
    @conversions = Array.new
    @items = Set.new
    super(*args)

    header.connect(SEL_COMMAND) do |sender, sel, item_number|
      on_cmd_header(item_number)
    end


    # HACK: only works when one header is there.
    self.connect(SEL_CONFIGURE) do |sender, sel, data|
      update_header_width
    end
  end

  def create
    super
    recalc
  end

  # HACK: only works when one header is there.
  def update_header_width
    header.setItemSize(0, width-verticalScrollBar.width)
    recalc
  end


  # Called whenever a header is clicked
  def on_cmd_header(header_item_index)
    @data.gui_mutex.synchronize do
      header.setArrowDir(@header_item_index, MAYBE)
      if @header_item_index == header_item_index
        @reversed = !@reversed
      else
        @reversed = true
      end
      @header_item_index = header_item_index
      header.setArrowDir(@header_item_index, @reversed)
      sortItems
    end
    # sort array
    @data.items.sort! do |a, b|
      cmp = a.sortable(header_item_index) <=> b.sortable(header_item_index)
      if @reversed
        -cmp
      else
        cmp
      end
    end
    return 0
  end

  def sortItems
    @items.each do |item|
      item.update_sort_key
    end
    super
  end

  # Check if the search order is reversed (happens when clicking on the same header twice)
  def reversed?
    @reversed
  end

  # Get index of the header that should be used for sorting
  def sort_index
    @header_item_index
  end

  # Add a new header. The block that you need to specify is used to convert a string of this
  # column into something that can be used for sorting. The yielded parameter item_colum_text
  # is the text of one item of this column.
  def add_header(text, width, &conversion) # :yields: item_column_text
    appendHeader(text, nil, width)
    header.setArrowDir(0, false) if @conversions.empty?
    @conversions.push conversion
  end

  # Create a new Packet_Item. After the icon, you can specify the text for each of the columns.
  # Here is an example:
  #  list.create_item(my_icon, "Martin", "Ankerl", "2005")
  def create_item(icon, *args)
    Packet_Item.new(self, icon, *args)
  end

  # Add a new Packet_Item to this list.Called from within Packet_Item during Packet_Item#parent=
  def add_item(item)
    @items.add item
    appendItem(item.fox_item)
    item
  end

  # Remove the given Packet_Item from the list. This is slow, because
  # the item has to be searched with a linear search. To remove all items
  # use #clear.
  def remove_item(item)
    i = 0
    @items.delete item
    fox_item = item.fox_item
    i += 1 while i < numItems && fox_item != getItem(i)
    removeItem(i) if i < numItems
    item
  end

  def appendItem(*args)
    $fx_icon_item_remove_mutex.synchronize do
      #puts "appendItem"
      super
    end
  end

  # Before actually deleting, the item has to be marked as deleted to prevent
  # drawing afterwards.
  def removeItem(*args)
    $fx_icon_item_remove_mutex.synchronize do
      #puts "removeitem"
      # at first, mark item as deleted to prevent drawing afterwards
      getItem(args[0]).deleted
      # do the evil delete
      super
    end
  end

  # Remove all items, but do this within the mutex to be sure everything goes ok.
  def clearItems(*args)
    $fx_icon_item_remove_mutex.synchronize do
      #puts "clearitems"
      super
    end
  end

  # This may be called when the item is currently beeing deleted.
  # this method works only before or after the delete, therefore
  # the mutex is necessary.
  def getContentWidth(*args)
    $fx_icon_item_remove_mutex.synchronize do
      #puts "contentwidth"
      super
    end
  end

  # This may be called when the item is currently beeing deleted.
  # this method works only before or after the delete, therefore
  # the mutex is necessary.
  def getContentHeight(*args)
    $fx_icon_item_remove_mutex.synchronize do
      #puts "contentheight"
      super
    end
  end

  # Removes all items from the list.
  def clear
    @items.each { |item| item.clear }
    @items.clear
    # todo: mutex!?
    clearItems
  end

  # use in combination with Packet_Item#show
  def dirty_clear
    clearItems
    @items.clear
  end

  # Get the sort for a given header index. The sort function has been supplied
  # when calling #new. This is used from Packet_Item.
  def sort_function(header_item_index)
    @conversions[header_item_index]
  end
end
