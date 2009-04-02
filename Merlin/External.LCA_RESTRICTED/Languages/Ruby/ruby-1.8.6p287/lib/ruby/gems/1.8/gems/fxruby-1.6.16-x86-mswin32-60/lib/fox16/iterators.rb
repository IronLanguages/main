module Fox

  class FXComboBox

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end
    
    #
    # Calls block once for each item in the list, passing the item's text and
    # user data as parameters.
    #
    def each # :yields: itemText, itemData
      0.upto(numItems - 1) do |i|
        yield getItemText(i), getItemData(i)
      end
      self
    end
  end

  class FXFoldingList

    include Enumerable

    #
    # Calls block once for each root-level folding list item, passing a
    # reference to that item as a parameter.
    #
    def each # :yields: aFoldingItem
      current = firstItem
      while current != nil
        next_current = current.next
        yield current
        current = next_current
      end
      self
    end
  end

  class FXFoldingItem

    include Enumerable

    #
    # Calls block once for each child of this folding list item, passing a
    # reference to that child item as a parameter.
    #
    def each # :yields: aFoldingItem
      current = first
      while current != nil
        next_current = current.next
        yield current
        current = next_current
      end
      self
    end
  end

  class FXHeader

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each item in the list, passing a reference to that
    # item as a parameter.
    #
    def each # :yields: aHeaderItem
      0.upto(numItems - 1) do |i|
        yield getItem(i)
      end
      self
    end
  end

  class FXIconList

    include Enumerable

    #
    # Calls block once for each item in the list, passing a reference to that
    # item as a parameter.
    #
    def each # :yields: anIconItem
      0.upto(numItems - 1) do |i|
        yield getItem(i)
      end
      self
    end
  end

  class FXList

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each item in the list, passing a reference to that
    # item as a parameter.
    #
    def each # :yields: aListItem
      0.upto(numItems - 1) do |i|
        yield getItem(i)
      end
      self
    end
  end

  class FXListBox

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each item in the list, passing the item's text,
    # icon and user data as parameters.
    #
    def each
      0.upto(numItems - 1) do |i|
        yield getItemText(i), getItemIcon(i), getItemData(i)
      end
      self
    end
  end

  class FXTable

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each row in the table, passing an array of
    # references (one element per column) as a parameter.
    #
    def each_row # :yields: itemArray
      0.upto(numRows - 1) do |i|
        tableRow = []
        0.upto(numColumns - 1) do |j|
          tableRow << getItem(i, j)
        end
        yield tableRow
      end
      self
    end

    #
    # Calls block once for each column in the table, passing an array of
    # references (one element per row) as a parameter.
    #
    def each_column # :yields: itemArray
      0.upto(numColumns - 1) do |j|
        tableCol = []
        0.upto(numRows - 1) do |i|
          tableCol << getItem(i, j)
        end
        yield tableCol
      end
      self
    end

    alias each each_row
  end

  class FXTreeItem

    include Enumerable

    #
    # Calls block once for each child of this tree item, passing a
    # reference to that child item as a parameter.
    #
    def each # :yields: aTreeItem
      current = first
      while current != nil
        next_current = current.next
        yield current
        current = next_current
      end
      self
    end
  end

  class FXTreeList

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each root-level tree item, passing a
    # reference to that item as a parameter.
    #
    def each # :yields: aTreeItem
      current = firstItem
      while current != nil
        next_current = current.next
        yield current
        current = next_current
      end
      self
    end
  end

  class FXTreeListBox

    include Enumerable

    #
    # Override Enumerable#first with FXWindow#first for backwards compatibility.
    #
    def first
      getFirst
    end

    #
    # Calls block once for each root-level tree item, passing a
    # reference to that item as a parameter.
    #
    def each # :yields: aTreeItem
      current = firstItem
      while current != nil
        next_current = current.next
        yield current
        current = next_current
      end
      self
    end
  end

  class FXStream
  end

  class FXFileStream
    #
    # Construct a new FXFileStream object with the specified data flow
    # direction (<em>save_or_load</em>) and _container_ object.
    # If an optional code block is given, it will be passed this file
    # stream as an argument, and the file stream will automatically be
    # closed when the block terminates.
    # If no code block is provided, this method just returns the
    # new file stream in an opened state.
    #
    # Raises FXStreamNoWriteError if <em>save_or_load</em> is +FXStreamSave+
    # but the file cannot be opened for writing. Raises FXStreamNoReadError
    # if <em>save_or_load</em> is +FXStreamLoad+ but the file cannot be opened
    # for reading.
    #

    def FXFileStream.open(filename, save_or_load, size=8192, container=nil) # :yields: theFileStream
      fstream = FXFileStream.new(container)
      if fstream.open(filename, save_or_load, size)
      	if block_given?
      	  begin
      	    yield fstream
      	  ensure
      	    fstream.close
      	  end
      	else
      	  fstream
      	end
      else
        # FXFileStream#open returned false, so report error
      	raise FXStreamError.makeStreamError(fstream.status)
      end
    end
  end

  class FXMemoryStream
    #
    # Construct a new FXMemoryStream object with the specified data flow
    # direction, data and container object.
    # If an optional code block is given, it will be passed this memory
    # stream as an argument, and the memory stream will automatically be
    # closed when the block terminates.
    # If no code block is provided, this method just returns the
    # new memory stream in an opened state.
    #
    # Raises FXStreamAllocError if some kind of memory allocation failed
    # while initializing the stream.
    #
    # ==== Parameters:
    #
    # +save_or_load+::	access mode, either +FXStreamSave+ or +FXStreamLoad+ [Integer]
    # +data+::		memory buffer used for the stream, or +nil+ if the buffer is to be initially empty [String].
    # +cont+::		the container object, or +nil+ if there is none [FXObject]
    #
    def FXMemoryStream.open(save_or_load, data, cont=nil) # :yields: theMemoryStream
      stream = FXMemoryStream.new(cont)
      if stream.open(save_or_load, data)
      	if block_given?
      	  begin
      	    yield stream
      	  ensure
      	    stream.close
      	  end
      	else
      	  stream
      	end
      else
        # FXFileStream#open returned false, so report error
      	raise FXStreamError.makeStreamError(stream.status)
      end
    end
  end

  class FXApp

    alias beginWaitCursor0 beginWaitCursor # :nodoc:

    #
    # Changes the default application cursor to an hourglass shape,
    # to provide a visual cue to the user that it's time to wait.
    # To revert the default application cursor to its normal shape,
    # call the #endWaitCursor method. For example,
    #
    #    getApp().beginWaitCursor()
    #      ... time-consuming operation ...
    #    getApp().endWaitCursor()
    #
    # Invocations of #beginWaitCursor may be nested, and if so, the
    # call to #endWaitCursor is matched with the most recent call to
    # #beginWaitCursor.
    #
    # If an optional code block is provided, #endWaitCursor is
    # automatically called after the block terminates, e.g.
    #
    #    getApp().beginWaitCursor() {
    #              ... time-consuming operation ...
    #      ... endWaitCursor() is called automatically ...
    #    }
    #
    def beginWaitCursor
      beginWaitCursor0
      if block_given?
        begin
      	  yield
      	ensure
      	  endWaitCursor
      	end
      end
    end
  end
  
  class FXDCPrint

    alias beginPrint0 beginPrint # :nodoc:

    #
    # Begin print session described by _job_ (an FXPrinter instance).
    # If an optional code block is provided, #endPrint is automatically
    # called after the block terminates.
    #
    def beginPrint(job) # :yields: theDC
      result = beginPrint0(job)
      if block_given?
        begin
      	  yield self
      	ensure
      	  endPrint
      	end
      else
        result
      end
    end

    alias beginPage0 beginPage # :nodoc:

    #
    # Generate beginning of _page_ (the page number).
    # If an optional code block is provided, #endPage is automatically
    # called after the block terminates.
    #
    def beginPage(page=1) # :yields: theDC
      result = beginPage0(page)
      if block_given?
        begin
      	  yield self
      	ensure
      	  endPage
      	end
      else
        result
      end
    end
  end
end
