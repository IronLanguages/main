# Copyright (c) 2004, 2005 Christoph Heindl and Martin Ankerl
class MutexDummy
  def synchronize
    yield
  end
end

# Lock used to synchronize item removes.
# for now, a global lock is good enough.
$fx_icon_item_remove_mutex = Mutex.new


# An item for Packet_List. This is a convenient wrapper for the FXIconItem.
class Packet_Item

  attr_accessor :searchable

  # Fox_Item is a very thin wrapper that calles Packet_Item functionality.
  class Fox_Item < FXIconItem
    # optimization: directly sort by sort_key and reversed
    attr_accessor :sort_key
    attr_accessor :reversed

    # Create a new item with packet_item as the parent. *args is passed to the FXIconItem.
    def initialize(packet_item, *args)
      @packet_item = packet_item
      @sort_key = nil
      @reversed = 1
      @is_deleted = false
      super(*args)
    end

    # Sometimes draw is called AFTER the item has been removed.
    # In this case: return immediately. Otherwise a Runtime Error would occur!
    def draw(*args)
      $fx_icon_item_remove_mutex.synchronize do
        #puts "draw"
        return if @is_deleted
        super
      end
    end

    # Mark item as deleted, to prevent execution of draw afterwards
    # this is called from Packet_List which uses the same mutex
    # as draw.
    def deleted
      @is_deleted = true
    end

    # Called from sortItems, uses packet_item's comparison.
    def <=>(other)
      (@sort_key <=> other.sort_key) * @reversed
    end

    # Get the packet item
    def packet_item
      @packet_item
    end

    # Pass-through method
    def data
      @packet_item.data
    end
  end

  # Creates a Packet_Item with newParent as the parent list, which is allowed to be nil.
  # You can also use Packet_List#add_item instead.
  def initialize(newParent, icon, *content)
    @content = content
    @sortable = Array.new(@content.size)
    @icon = icon
    @data = nil
    @parent = nil
    @sort_key = nil
    @searchable = nil
    # call parent=, don't forget the self!!
    self.parent = newParent
    show if newParent
    # update sortable
    @content.each_index do |pos|
      update_sortable(pos, @content[pos]) if parent
    end
  end

  # Get the content of the given column number.
  def [](column_nr)
    @content[column_nr]
  end

  # Set new text for the given column number.
  def []=(column_nr, newVal)
    @content[column_nr] = newVal
    return unless @parent
    @item.text = @content.join("\t")
    update_sortable(column_nr, newVal)
    @parent.recalc
  end

  # Get the sortable representation of this column's content.
  def sortable(pos)
    @sortable[pos]
  end

  # update FXIconItem's sort key and reversed status
  def update_sort_key
    @item.sort_key = @sortable[@parent.sort_index]
    @item.reversed = @parent.reversed? ? -1 : 1
  end

  # Get the parent list for this item.
  def parent
    @parent
  end

  # Set a new parent. This removes this item from the current list and adds itself to
  # the new one.
  def parent=(newParent)
    return if newParent == @parent
    remove_item if @parent
    @parent = newParent
    @content.each_index do |pos|
      update_sortable(pos, @content[pos]) if @parent
    end
  end

  # Shows item on parent without updating the search index. This can be used
  # if an item is removed and added to the same list.
  def show
    create_item
    @parent.add_item(self)
  end

  # Get the wrapper item.
  def fox_item
    @item
  end

  # Allows to set user data.
  def data=(data)
    @data = data
  end

  # Get user data.
  def data
    @data
  end

  # Removes the item from its parent.
  def clear
    @parent = nil
  end

  # The icon of this item.
  def icon
    @icon
  end

  private

  # Calles the sort function to update the sortable representation of column pos.
  def update_sortable(pos, newVal)
    sort_function = @parent.sort_function(pos)
    if sort_function
      @sortable[pos] = sort_function.call(newVal)
    else
      @sortable[pos] = newVal
    end
  end

  # Remove item from parent
  def remove_item
    @parent.remove_item(self)
  end

  # Creates wrapper item, and sets it's data
  def create_item
    @item = Fox_Item.new(self, @content.join("\t"), @icon, @icon)
  end
end