#!/usr/bin/env ruby

require 'fox16'
require 'date'

include Fox

class TableWindow < FXMainWindow

  def initialize(app)
    # Call the base class initializer first
    super(app, "Table Widget Test", :opts => DECOR_ALL)

    # Tooltip
    tooltip = FXToolTip.new(getApp())
    
    # Icon used in some cells
    penguinicon = nil
    File.open(File.join('icons', 'penguin.png'), 'rb') do |f|
      penguinicon = FXPNGIcon.new(getApp(), f.read, 0, IMAGE_ALPHAGUESS)
    end
    
    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    
    # Separator
    FXHorizontalSeparator.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X|SEPARATOR_GROOVE)
  
    # Contents
    contents = FXVerticalFrame.new(self, LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    frame = FXVerticalFrame.new(contents,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y, :padding => 0)
  
    # Table
    @table = FXTable.new(frame,
      :opts => TABLE_COL_SIZABLE|TABLE_ROW_SIZABLE|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padding => 2)
      
    @table.visibleRows = 20
    @table.visibleColumns = 8

    @table.setTableSize(50, 14)

    @table.setBackColor(FXRGB(255, 255, 255))
    @table.setCellColor(0, 0, FXRGB(255, 255, 255))
    @table.setCellColor(0, 1, FXRGB(255, 240, 240))
    @table.setCellColor(1, 0, FXRGB(240, 255, 240))
    @table.setCellColor(1, 1, FXRGB(240, 240, 255))
  
    # Initialize the scrollable part of the table
    (0..49).each do |r|
      (0..13).each do |c|
        @table.setItemText(r, c, "r:#{r} c:#{c}")
      end
    end

    # Initialize column headers
    (0...12).each  { |c| @table.setColumnText(c, Date::MONTHNAMES[c+1]) }
    
    # Initialize row headers
    (0...50).each { |r| @table.setRowText(r, "Row#{r}") }
    
    @table.setItemText(10, 10, "This is multi-\nline text")
    @table.setItemJustify(10, 10, FXTableItem::CENTER_X|FXTableItem::CENTER_Y)

    @table.setItem(3, 3, nil)
    @table.setItem(5, 6, @table.getItem(5, 5))
    @table.setItem(5, 7, @table.getItem(5, 5))
    @table.setItemText(5, 5, "Spanning Item")
    @table.setItemJustify(5, 5, FXTableItem::CENTER_X|FXTableItem::CENTER_Y)
    
    @table.getItem( 9,  9).borders = FXTableItem::TBORDER|FXTableItem::LBORDER|FXTableItem::BBORDER
    @table.getItem( 9, 10).borders = FXTableItem::TBORDER|FXTableItem::RBORDER|FXTableItem::BBORDER
    
    @table.getItem(40, 13).borders = FXTableItem::LBORDER|FXTableItem::TBORDER|FXTableItem::RBORDER|FXTableItem::BBORDER
    @table.getItem(49, 13).borders = FXTableItem::LBORDER|FXTableItem::TBORDER|FXTableItem::RBORDER|FXTableItem::BBORDER
    @table.getItem( 5,  0).borders = FXTableItem::LBORDER|FXTableItem::TBORDER|FXTableItem::RBORDER|FXTableItem::BBORDER
    
    @table.getItem(6, 6).icon = penguinicon
    @table.getItem(6, 6).iconPosition = FXTableItem::ABOVE  # icon above the text
    @table.getItem(6, 6).justify = FXTableItem::CENTER_X|FXTableItem::CENTER_Y
    
    @table.getItem(3, 4).stipple = STIPPLE_CROSSDIAG
    
    # File Menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Options Menu
    tablemenu = FXMenuPane.new(self)
    FXMenuCheck.new(tablemenu, "Horizontal grid", @table, FXTable::ID_HORZ_GRID)
    FXMenuCheck.new(tablemenu, "Vertical grid", @table, FXTable::ID_VERT_GRID)
    FXMenuTitle.new(menubar, "&Options", nil, tablemenu)
    
    # Manipulations Menu
    manipmenu = FXMenuPane.new(self)
    FXMenuCommand.new(manipmenu, "Delete Column\tCtl-C", nil,
      @table, FXTable::ID_DELETE_COLUMN)
    FXMenuCommand.new(manipmenu, "Delete Row\tCtl-R", nil,
      @table, FXTable::ID_DELETE_ROW)
    FXMenuCommand.new(manipmenu, "Insert Column\tCtl-Shift-C", nil,
      @table, FXTable::ID_INSERT_COLUMN)
    FXMenuCommand.new(manipmenu, "Insert Row\tCtl-Shift-R", nil,
      @table, FXTable::ID_INSERT_ROW)
    FXMenuCommand.new(manipmenu, "Resize table...").connect(SEL_COMMAND, method(:onCmdResizeTable))
    FXMenuTitle.new(menubar, "&Manipulations", nil, manipmenu)
    
    # Selection Menu
    selectmenu = FXMenuPane.new(self)
    FXMenuCommand.new(selectmenu, "Select All", nil, @table, FXTable::ID_SELECT_ALL)
    FXMenuCommand.new(selectmenu, "Select Cell", nil, @table, FXTable::ID_SELECT_CELL)
    FXMenuCommand.new(selectmenu, "Select Row", nil, @table, FXTable::ID_SELECT_ROW)
    FXMenuCommand.new(selectmenu, "Select Column", nil, @table, FXTable::ID_SELECT_COLUMN)
    FXMenuCommand.new(selectmenu, "Deselect All", nil, @table, FXTable::ID_DESELECT_ALL)
    FXMenuCommand.new(selectmenu, "Cut to Clipboard", nil, @table, FXTable::ID_CUT_SEL)
    FXMenuCommand.new(selectmenu, "Copy to Clipboard", nil, @table, FXTable::ID_COPY_SEL)
    FXMenuCommand.new(selectmenu, "Paste from Clipboard", nil, @table, FXTable::ID_PASTE_SEL)
    FXMenuCommand.new(selectmenu, "Delete", nil, @table, FXTable::ID_DELETE_SEL)
    FXMenuTitle.new(menubar, "&Selection", nil, selectmenu)
  end

  # Resize the table
  def onCmdResizeTable(sender, sel, ptr)
    # Create an empty dialog box
    dlg = FXDialogBox.new(self, "Resize Table")

    # Set up its contents
    frame = FXHorizontalFrame.new(dlg, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(frame, "Rows:", nil, LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)
    rows = FXTextField.new(frame, 5,
      :opts => JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)
    FXLabel.new(frame, "Columns:", nil, LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)
    cols = FXTextField.new(frame, 5,
      :opts => JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)
    FXButton.new(frame, "Cancel", nil, dlg, FXDialogBox::ID_CANCEL,
      FRAME_RAISED|FRAME_THICK|LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)
    FXButton.new(frame, "  OK  ", nil, dlg, FXDialogBox::ID_ACCEPT,
      FRAME_RAISED|FRAME_THICK|LAYOUT_SIDE_LEFT|LAYOUT_CENTER_Y)

    # Initialize the text fields' contents
    oldnr, oldnc = @table.numRows, @table.numColumns
    rows.text = oldnr.to_s
    cols.text = oldnc.to_s

    # FXDialogBox#execute will return non-zero if the user clicks OK
    if dlg.execute != 0
      nr, nc = rows.text.to_i, cols.text.to_i
      nr = 0  if nr < 0
      nc = 0  if nc < 0
      @table.setTableSize(nr, nc)
      (0...nr).each { |r|
        (0...nc).each { |c|
#         @table.setItemText(r, c, "r:#{r+1} c:#{c+1}")
        }
      }
    end
    return 1
  end

  # Create and show this window
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

# Start the whole thing
if __FILE__ == $0
  # Make application
  application = FXApp.new("TableApp", "FoxTest")
  
  # Make window
  TableWindow.new(application)
  
  # Create app
  application.create
  
  # Run
  application.run
end
