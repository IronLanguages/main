require 'fox16'

include Fox

class PrefDialog < FXDialogBox
  # Load the named icon from a file
  def loadIcon(filename)
    begin
      filename = File.join("..", "icons", filename)
      icon = nil
      File.open(filename, "rb") { |f|
        icon = FXPNGIcon.new(getApp(), f.read)
      }
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end

  def initialize(owner)
    super(owner, "TextEdit Preferences", DECOR_TITLE|DECOR_BORDER|DECOR_RESIZE,
      0, 0, 0, 0, 0, 0, 0, 0, 4, 4)

    vertical = FXVerticalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    horizontal = FXHorizontalFrame.new(vertical, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    buttons = FXVerticalFrame.new(horizontal, (LAYOUT_LEFT|LAYOUT_FILL_Y|
      FRAME_SUNKEN|PACK_UNIFORM_WIDTH|PACK_UNIFORM_HEIGHT))
    buttons.padLeft = 0
    buttons.padRight = 0
    buttons.padTop = 0
    buttons.padBottom = 0
    switcher = FXSwitcher.new(horizontal, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # Icons
    pal = loadIcon("palette.png")
    ind = loadIcon("indent.png")
    pat = loadIcon("pattern.png")
    del = loadIcon("delimit.png")

    # Pane 1
    pane1 = FXVerticalFrame.new(switcher, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(pane1, "Color settings", nil, LAYOUT_LEFT)
    FXHorizontalSeparator.new(pane1, SEPARATOR_LINE|LAYOUT_FILL_X)
    matrix1 = FXMatrix.new(pane1, 5,
      MATRIX_BY_ROWS|PACK_UNIFORM_HEIGHT|LAYOUT_FILL_X|LAYOUT_FILL_Y)

    FXLabel.new(matrix1, "Background:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Text:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Sel. text background:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Sel. text:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Cursor:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)

    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_TEXT_BACK,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_TEXT_FORE,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_TEXT_SELBACK,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_TEXT_SELFORE,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_TEXT_CURSOR,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)

    FXLabel.new(matrix1, "Files background:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Files:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Sel. files background:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Sel. files:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix1, "Lines:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)

    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_DIR_BACK,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_DIR_FORE,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_DIR_SELBACK,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_DIR_SELFORE,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)
    FXColorWell.new(matrix1, FXRGB(0, 0, 0), owner, TextWindow::ID_DIR_LINES,
      (COLORWELL_OPAQUEONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_LEFT|
       LAYOUT_CENTER_Y|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|LAYOUT_FILL_COLUMN|
       LAYOUT_FILL_ROW), 0, 0, 40, 24)

    # Button 1
    FXButton.new(buttons, "Colors\tChange Colors\tChange text colors.", pal,
      switcher, FXSwitcher::ID_OPEN_FIRST, (FRAME_RAISED|ICON_ABOVE_TEXT|
      LAYOUT_FILL_Y))

    # Pane 2
    pane2 = FXVerticalFrame.new(switcher, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(pane2, "Editor settings", nil, LAYOUT_LEFT)
    FXHorizontalSeparator.new(pane2, SEPARATOR_LINE|LAYOUT_FILL_X)
    matrix2 = FXMatrix.new(pane2, 5, (MATRIX_BY_ROWS|PACK_UNIFORM_HEIGHT|
      LAYOUT_FILL_X|LAYOUT_FILL_Y))

    FXLabel.new(matrix2, "Word wrapping:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Auto indent:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Fixed wrap margin:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Strip carriage returns:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Insert tab characters:", nil, (JUSTIFY_LEFT|
      LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))

    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_TOGGLE_WRAP,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)
    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_AUTOINDENT,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)
    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_FIXED_WRAP,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)
    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_STRIP_CR,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)
    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_INSERTTABS,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)

    FXLabel.new(matrix2, "Wrap margin:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Tab columns:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Brace match time (us):", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Strip trailing spaces:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)
    FXLabel.new(matrix2, "Mouse wheel lines:", nil,
      JUSTIFY_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW)

    FXTextField.new(matrix2, 4, owner, TextWindow::ID_WRAPCOLUMNS,
      (JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_LEFT|
       LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))
    FXTextField.new(matrix2, 4, owner, TextWindow::ID_TABCOLUMNS,
      (JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_LEFT|
       LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))
    FXTextField.new(matrix2, 4, owner, TextWindow::ID_BRACEMATCH,
      (JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_LEFT|
       LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))
    FXCheckButton.new(matrix2, nil, owner, TextWindow::ID_STRIP_SP,
      LAYOUT_LEFT|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW,
      0, 0, 0, 0, 0, 0, 0, 0)
    spinner = FXSpinner.new(matrix2, 2, owner, TextWindow::ID_WHEELADJUST,
      (JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_LEFT|
       LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))
    spinner.range = 1..100

    # Button 2
    FXButton.new(buttons,
      "Editor\tEditor settings\tChange editor settings and other things.", ind,
      switcher, FXSwitcher::ID_OPEN_SECOND,
      FRAME_RAISED|ICON_ABOVE_TEXT|LAYOUT_FILL_Y)

    # Pane 3
    pane3 = FXVerticalFrame.new(switcher, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(pane3, "File Patterns", nil, LAYOUT_LEFT)
    FXHorizontalSeparator.new(pane3, SEPARATOR_LINE|LAYOUT_FILL_X)
    sub3 = FXVerticalFrame.new(pane3, LAYOUT_FILL_Y|LAYOUT_FILL_X)
    FXLabel.new(sub3,
      'Filename patterns, for example "Source Files (*.c,*.h)", one per line:',
      nil, JUSTIFY_LEFT)
    textwell = FXVerticalFrame.new(sub3,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_SUNKEN|FRAME_THICK,
      0, 0, 0, 0, 0, 0, 0, 0)
    @text = FXText.new(textwell, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # Button 3
    FXButton.new(buttons,
      "Patterns\tFilename patterns\tChange wildcard patterns for filenames.",
      pat, switcher, FXSwitcher::ID_OPEN_THIRD,
      FRAME_RAISED|ICON_ABOVE_TEXT|LAYOUT_FILL_Y)

    # Pane 4
    pane4 = FXVerticalFrame.new(switcher, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(pane4, "Word Delimiters", nil, LAYOUT_LEFT)
    FXHorizontalSeparator.new(pane4, SEPARATOR_LINE|LAYOUT_FILL_X)
    sub4 = FXVerticalFrame.new(pane4, LAYOUT_FILL_Y|LAYOUT_FILL_X)
    FXLabel.new(sub4, "Characters delimiting words:", nil, JUSTIFY_LEFT)
    FXTextField.new(sub4, 20, owner, TextWindow::ID_DELIMITERS,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X)

    # Button 4
    FXButton.new(buttons,
      "Delimiters\tWord delimiters\tChange delimiters for word selections.",
      del, switcher, FXSwitcher::ID_OPEN_FOURTH,
      FRAME_RAISED|ICON_ABOVE_TEXT|LAYOUT_FILL_Y)

    # Bottom part
    FXHorizontalSeparator.new(vertical, SEPARATOR_RIDGE|LAYOUT_FILL_X)
    closebox = FXHorizontalFrame.new(vertical,
      LAYOUT_BOTTOM|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH)
    FXButton.new(closebox, "&Accept", nil, self, FXDialogBox::ID_ACCEPT,
      LAYOUT_RIGHT|FRAME_RAISED|FRAME_THICK, 0, 0, 0, 0, 20, 20)
    FXButton.new(closebox, "&Cancel", nil, self, FXDialogBox::ID_CANCEL,
      LAYOUT_RIGHT|FRAME_RAISED|FRAME_THICK, 0, 0, 0, 0, 20, 20)
  end


  # Change patterns, each pattern separated by newline
  def setPatterns(patterns)
    @text.text = patterns.join("\n")
  end

  # Return array of patterns
  def getPatterns
    @text.text.split("\n")
  end
end
