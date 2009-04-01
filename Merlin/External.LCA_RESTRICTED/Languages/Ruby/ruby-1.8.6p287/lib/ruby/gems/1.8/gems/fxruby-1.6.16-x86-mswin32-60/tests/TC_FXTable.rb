require 'test/unit'
require 'testcase'
require 'fox16'

include Fox

class OverrideError < Exception
end

class CustomTable < FXTable
  def setColumnWidth(col, cwidth)
    raise OverrideError
  end

  def setRowHeight(row, rheight)
    raise OverrideError
  end

  def setColumnX(colEdge, x)
    raise OverrideError
  end

  def setRowY(rowEdge, y)
    raise OverrideError
  end
end

class TC_FXTable < TestCase

private

  def populateTable
    @table.each_row do |row|
      row.each { |item| item.text = "foo" }
    end
  end

  def clearEdges(extending=false)
    item = nil
    nr = @table.numRows
    nc = @table.numColumns
    
    (1..nc-2).each { |c|
      item = @table.getItem(nr-1, c)
        item.button = false
        item.justify = FXTableItem::RIGHT
    }
    
    unless extending
      (1..nr-2).each { |r|
        item = @table.getItem(r, nc-1)
          item.button = false
      }
      if nr > 0 and nc > 0
        item = @table.getItem(nr-1, nc-1)
          item.button = false
          item.text = ""
      end
    end
  end

  def loadLogChunk(num_rows)
    if @logsource
      extending = true
    else
      extending = false
      @logsource = nil
      @header_row = ["foo"]*10
      @data_lines = []
    end

    100.times { @data_lines << ["1"]*10 }

    item = nil

    clearEdges(extending)

    old_nr = @table.numRows
    old_nc = @table.numColumns

    # resize the table
    nr = @data_lines.size + 2
    nc = @header_row.size + 2
    @table.setTableSize(nr, nc)

    @table.leadingRows = 1
    @table.leadingColumns = 1
    @table.trailingRows = 1
    @table.trailingColumns = 1

    # Initialize first & last fixed rows
    (1..nc-2).each { |c|
      unless extending
        @table.setItemText(0, c, @header_row[c-1])
        item = @table.getItem(0, c)
          item.button = true
      end
      @table.setItemText(nr-1, c, @header_row[c-1])
      item = @table.getItem(nr-1, c)
        item.button = true
    }

    # Initialize first & last fixed columns
    start_r = extending ? old_nr-1 : 1
    (start_r..nr-2).each { |r|
      @table.setItemText(r,  0, "#{r}")
      @table.setItemText(r, nc-1, "#{r}")
      @table.getItem(r, 0).setButton(true)
      @table.getItem(r, nc-1).setButton(true)
    }

    # The corners are just buttons
    @table.setItemText(0, 0, "")
    @table.getItem(0, 0).setButton(true)
    @table.setItemText(0, nc-1, "")
    @table.getItem(0, nc-1).setButton(true)
    @table.setItemText(nr-1, 0, "")
    @table.getItem(nr-1, 0).setButton(true)
    @table.setItemText(nr-1, nc-1, "")
    item = @table.getItem(nr-1, nc-1)
      item.button = true
      if false
        # disable the button
        @more_pos = nil
      else
        remaining = 100
        if remaining > 1024
          item.text = "%5.1fM MORE" % (remaining/1024)
        else
          item.text = "%5.1fK MORE" % remaining
        end
        @more_pos = FXTablePos.new
        @more_pos.row = nr-1; @more_pos.col = nc-1
      end
    @table.setItemText(0, nc-1, "")

    # Initialize scrollable part of table
    entry = nil
    (start_r..nr-2).each { |r|
      (1..nc-2).each { |c|
        entry = @data_lines[r-1][c-1]
        case entry
        when /\A(.*\.)(.*)\z/
          entry = sprintf("%10.5f", entry.to_f).
            sub(/\.(\d*?)(0*)$/) { ".#{$1}#{$2.gsub("0", " ")}" }
        when /\d+/
          entry << " " * 6
        end
        @table.setItemText(r, c, entry)
      }
    }
  end

  # Load the named log file
  def loadLog filename = @file_names
    @file_names = filename
    @logsource = nil
    clearEdges
    @table.setTableSize(0, 0)
    loadLogChunk(100)
  end

public
  
  def setup
    super(self.class.name)
    @table = FXTable.new(mainWindow)
    @customTable = CustomTable.new(mainWindow)
    populateTable
  end

=begin
  def test_setTableSize
    100.times { loadLog } # this should be enough to do it
  end
=end
  
  def test_getCellColor
    assert_nothing_raised {
      @table.getCellColor(0, 0)
    }
    assert_nothing_raised {
      @table.getCellColor(0, 1)
    }
    assert_nothing_raised {
      @table.getCellColor(1, 0)
    }
    assert_nothing_raised {
      @table.getCellColor(1, 1)
    }

    @table.setTableSize(5, 5)
    assert_raises(IndexError) {
      @table.getCellColor(-1, 0)
    }
    assert_raises(IndexError) {
      @table.getCellColor(2, 0)
    }
    assert_raises(IndexError) {
      @table.getCellColor(0, -1)
    }
    assert_raises(IndexError) {
      @table.getCellColor(0, 2)
    }
  end

  def test_setCellColor
    assert_nothing_raised {
      @table.setCellColor(0, 0, FXRGB(0, 0, 0))
    }
    assert_nothing_raised {
      @table.setCellColor(0, 1, FXRGB(0, 0, 0))
    }
    assert_nothing_raised {
      @table.setCellColor(1, 0, FXRGB(0, 0, 0))
    }
    assert_nothing_raised {
      @table.setCellColor(1, 1, FXRGB(0, 0, 0))
    }

    @table.setTableSize(5, 5)
    assert_raises(IndexError) {
      @table.setCellColor(-1, 0, FXRGB(0, 0, 0))
    }
    assert_raises(IndexError) {
      @table.setCellColor(2, 0, FXRGB(0, 0, 0))
    }
    assert_raises(IndexError) {
      @table.setCellColor(0, -1, FXRGB(0, 0, 0))
    }
    assert_raises(IndexError) {
      @table.setCellColor(0, 2, FXRGB(0, 0, 0))
    }
  end
  
  def test_updateRange
    @table.setTableSize(5, 5)
    assert_nothing_raised {
      @table.updateRange(0, 4, 0, 4)
    }
    assert_raises(IndexError) {
      @table.updateRange(-1, 0, 0, 0) # startRow < 0
    }
    assert_raises(IndexError) {
      @table.updateRange(0, 5, 0, 0) # endRow >= numRows
    }
    assert_raises(IndexError) {
      @table.updateRange(0, 0, -1, 0) # startCol < 0
    }
    assert_raises(IndexError) {
      @table.updateRange(0, 0, 0, 5) # endCol >= numColumns
    }
  end
  
  def test_insertRows
    @table.setTableSize(5, 5)
    assert_nothing_raised {
      @table.insertRows(0)
    }
    assert_nothing_raised {
      @table.insertRows(@table.numRows)
    }
    assert_raises(IndexError) {
      @table.insertRows(-1) # row < 0
    }
    assert_raises(IndexError) {
      @table.insertRows(@table.numRows+1) # row > numRows
    }
  end

  def test_insertColumns
    @table.setTableSize(5, 5)
    assert_nothing_raised {
      @table.insertColumns(0)
    }
    assert_nothing_raised {
      @table.insertColumns(@table.numColumns)
    }
    assert_raises(IndexError) {
      @table.insertColumns(-1) # column < 0
    }
    assert_raises(IndexError) {
      @table.insertColumns(@table.numColumns+1) # column > numColumns
    }
  end
  
  def test_removeRows
    @table.setTableSize(8, 5)
    assert_raises(IndexError) {
      @table.removeRows(-1)
    }
    assert_nothing_raised {
      @table.removeRows(0)
    }
    assert_nothing_raised {
      @table.removeRows(@table.numRows-1)
    }
    assert_raises(IndexError) {
      @table.removeRows(@table.numRows)
    }
  end
  
  def test_removeColumns
    @table.setTableSize(5, 8)
    assert_raises(IndexError) {
      @table.removeColumns(-1)
    }
    assert_nothing_raised {
      @table.removeColumns(0)
    }
    assert_nothing_raised {
      @table.removeColumns(@table.numColumns-1)
    }
    assert_raises(IndexError) {
      @table.removeColumns(@table.numColumns)
    }
  end

  def test_getColumnX
    @table.setTableSize(5, 5)
    assert_raises(IndexError) {
      @table.getColumnX(-1)
    }
    assert_nothing_raised {
      @table.getColumnX(0)
    }
    assert_nothing_raised {
      @table.getColumnX(4)
    }
    assert_raises(IndexError) {
      @table.getColumnX(5)
    }
  end
  
  def test_getRowY
    @table.setTableSize(5, 5)
    assert_raises(IndexError) {
      @table.getRowY(-1)
    }
    assert_nothing_raised {
      @table.getRowY(0)
    }
    assert_nothing_raised {
      @table.getRowY(4)
    }
    assert_raises(IndexError) {
      @table.getRowY(5)
    }
  end

  def test_extractText
    @table.setTableSize(2, 2)
    @table.setItemText(0, 0, "(0, 0)")
    @table.setItemText(0, 1, "(0, 1)")
    @table.setItemText(1, 0, "(1, 0)")
    @table.setItemText(1, 1, "(1, 1)")
    assert_equal("(0, 0)\t(0, 1)\n(1, 0)\t(1, 1)\n", @table.extractText(0, 1, 0, 1))
  end
  
  def test_overlayText
    @table.setTableSize(2, 2)
    @table.overlayText(0, 1, 0, 1, "(0, 0)\t(0, 1)\n(1, 0)\t(1, 1)\n")
    assert_equal("(0, 0)", @table.getItemText(0, 0))
    assert_equal("(0, 1)", @table.getItemText(0, 1))
    assert_equal("(1, 0)", @table.getItemText(1, 0))
    assert_equal("(1, 1)", @table.getItemText(1, 1))
  end
end
