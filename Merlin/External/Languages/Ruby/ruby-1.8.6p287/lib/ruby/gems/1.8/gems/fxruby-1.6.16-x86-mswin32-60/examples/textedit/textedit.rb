#!/usr/bin/env ruby

require 'fox16'
require 'fox16/responder'
require 'fox16/undolist'
require 'prefdialog'
require 'helpwindow'
require 'commands'

include Fox

class TextWindow < FXMainWindow

  include Responder

  MAXUNDOSIZE, KEEPUNDOSIZE = 1000000, 500000

  # Define message identifiers recognized by this class
  ID_ABOUT,
  ID_FILEFILTER,
  ID_OPEN,
  ID_OPEN_SELECTED,
  ID_REOPEN,
  ID_SAVE,
  ID_SAVEAS,
  ID_NEW,
  ID_TITLE,
  ID_FONT,
  ID_QUIT,
  ID_PRINT,
  ID_TREELIST,
  ID_TEXT_BACK,
  ID_TEXT_FORE,
  ID_TEXT_SELBACK,
  ID_TEXT_SELFORE,
  ID_TEXT_CURSOR,
  ID_DIR_BACK,
  ID_DIR_FORE,
  ID_DIR_SELBACK,
  ID_DIR_SELFORE,
  ID_DIR_LINES,
  ID_RECENTFILE,
  ID_TOGGLE_WRAP,
  ID_FIXED_WRAP,
  ID_SAVE_SETTINGS,
  ID_TEXT,
  ID_STRIP_CR,
  ID_STRIP_SP,
  ID_INCLUDE_PATH,
  ID_SHOW_HELP,
  ID_OVERSTRIKE,
  ID_READONLY,
  ID_FILETIME,
  ID_PREFERENCES,
  ID_TABCOLUMNS,
  ID_WRAPCOLUMNS,
  ID_DELIMITERS,
  ID_INSERTTABS,
  ID_AUTOINDENT,
  ID_BRACEMATCH,
  ID_NUMCHARS,
  ID_INSERT_FILE,
  ID_EXTRACT_FILE,
  ID_WHEELADJUST,
  ID_LAST = enum(FXMainWindow::ID_LAST, 47)

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

  def initialize(app)
    # Call base class initialize first
    super(app, "FOX Text Editor: - untitled", nil, nil, DECOR_ALL,
      0, 0, 850, 600, 0, 0)

    # Set up the message map for this class
    FXMAPFUNC(SEL_TIMEOUT,            ID_FILETIME,      :onCheckFile)
    FXMAPFUNC(SEL_COMMAND,            ID_ABOUT,         :onCmdAbout)
    FXMAPFUNC(SEL_COMMAND,            ID_REOPEN,        :onCmdReopen)
    FXMAPFUNC(SEL_UPDATE,             ID_REOPEN,        :onUpdReopen)
    FXMAPFUNC(SEL_COMMAND,            ID_OPEN,          :onCmdOpen)
    FXMAPFUNC(SEL_COMMAND,            ID_OPEN_SELECTED, :onCmdOpenSelected)
    FXMAPFUNC(SEL_COMMAND,            ID_SAVE,          :onCmdSave)
    FXMAPFUNC(SEL_UPDATE,             ID_SAVE,          :onUpdSave)
    FXMAPFUNC(SEL_COMMAND,            ID_SAVEAS,        :onCmdSaveAs)
    FXMAPFUNC(SEL_COMMAND,            ID_NEW,           :onCmdNew)
    FXMAPFUNC(SEL_UPDATE,             ID_TITLE,         :onUpdTitle)
    FXMAPFUNC(SEL_COMMAND,            ID_FONT,          :onCmdFont)
    FXMAPFUNC(SEL_COMMAND,            ID_QUIT,          :onCmdQuit)
    FXMAPFUNC(SEL_SIGNAL,             ID_QUIT,          :onCmdQuit)
    FXMAPFUNC(SEL_CLOSE,              ID_TITLE,         :onCmdQuit)
    FXMAPFUNC(SEL_COMMAND,            ID_PRINT,         :onCmdPrint)
    FXMAPFUNC(SEL_COMMAND,            ID_TREELIST,      :onCmdTreeList)
  
    FXMAPFUNC(SEL_COMMAND,            ID_TEXT_BACK,     :onCmdTextBackColor)
    FXMAPFUNC(SEL_CHANGED,            ID_TEXT_BACK,     :onCmdTextBackColor)
    FXMAPFUNC(SEL_UPDATE,             ID_TEXT_BACK,     :onUpdTextBackColor)
    FXMAPFUNC(SEL_COMMAND,            ID_TEXT_SELBACK,  :onCmdTextSelBackColor)
    FXMAPFUNC(SEL_CHANGED,            ID_TEXT_SELBACK,  :onCmdTextSelBackColor)
    FXMAPFUNC(SEL_UPDATE,             ID_TEXT_SELBACK,  :onUpdTextSelBackColor)
    FXMAPFUNC(SEL_COMMAND,            ID_TEXT_FORE,     :onCmdTextForeColor)
    FXMAPFUNC(SEL_CHANGED,            ID_TEXT_FORE,     :onCmdTextForeColor)
    FXMAPFUNC(SEL_UPDATE,             ID_TEXT_FORE,     :onUpdTextForeColor)
    FXMAPFUNC(SEL_COMMAND,            ID_TEXT_SELFORE,  :onCmdTextSelForeColor)
    FXMAPFUNC(SEL_CHANGED,            ID_TEXT_SELFORE,  :onCmdTextSelForeColor)
    FXMAPFUNC(SEL_UPDATE,             ID_TEXT_SELFORE,  :onUpdTextSelForeColor)
    FXMAPFUNC(SEL_COMMAND,            ID_TEXT_CURSOR,   :onCmdTextCursorColor)
    FXMAPFUNC(SEL_CHANGED,            ID_TEXT_CURSOR,   :onCmdTextCursorColor)
    FXMAPFUNC(SEL_UPDATE,             ID_TEXT_CURSOR,   :onUpdTextCursorColor)
  
    FXMAPFUNC(SEL_COMMAND,            ID_DIR_BACK,      :onCmdDirBackColor)
    FXMAPFUNC(SEL_CHANGED,            ID_DIR_BACK,      :onCmdDirBackColor)
    FXMAPFUNC(SEL_UPDATE,             ID_DIR_BACK,      :onUpdDirBackColor)
    FXMAPFUNC(SEL_COMMAND,            ID_DIR_FORE,      :onCmdDirForeColor)
    FXMAPFUNC(SEL_CHANGED,            ID_DIR_FORE,      :onCmdDirForeColor)
    FXMAPFUNC(SEL_UPDATE,             ID_DIR_FORE,      :onUpdDirForeColor)
    FXMAPFUNC(SEL_COMMAND,            ID_DIR_SELBACK,   :onCmdDirSelBackColor)
    FXMAPFUNC(SEL_CHANGED,            ID_DIR_SELBACK,   :onCmdDirSelBackColor)
    FXMAPFUNC(SEL_UPDATE,             ID_DIR_SELBACK,   :onUpdDirSelBackColor)
    FXMAPFUNC(SEL_COMMAND,            ID_DIR_SELFORE,   :onCmdDirSelForeColor)
    FXMAPFUNC(SEL_CHANGED,            ID_DIR_SELFORE,   :onCmdDirSelForeColor)
    FXMAPFUNC(SEL_UPDATE,             ID_DIR_SELFORE,   :onUpdDirSelForeColor)
    FXMAPFUNC(SEL_COMMAND,            ID_DIR_LINES,     :onCmdDirLineColor)
    FXMAPFUNC(SEL_CHANGED,            ID_DIR_LINES,     :onCmdDirLineColor)
    FXMAPFUNC(SEL_UPDATE,             ID_DIR_LINES,     :onUpdDirLineColor)
  
    FXMAPFUNC(SEL_COMMAND,            ID_RECENTFILE,    :onCmdRecentFile)
    FXMAPFUNC(SEL_UPDATE,             ID_TOGGLE_WRAP,   :onUpdWrap)
    FXMAPFUNC(SEL_COMMAND,            ID_TOGGLE_WRAP,   :onCmdWrap)
    FXMAPFUNC(SEL_COMMAND,            ID_SAVE_SETTINGS, :onCmdSaveSettings)
    FXMAPFUNC(SEL_INSERTED,           ID_TEXT,          :onTextInserted)
    FXMAPFUNC(SEL_REPLACED,           ID_TEXT,          :onTextReplaced)
    FXMAPFUNC(SEL_DELETED,            ID_TEXT,          :onTextDeleted)
    FXMAPFUNC(SEL_RIGHTBUTTONRELEASE, ID_TEXT,          :onTextRightMouse)
    FXMAPFUNC(SEL_UPDATE,             ID_FIXED_WRAP,    :onUpdWrapFixed)
    FXMAPFUNC(SEL_COMMAND,            ID_FIXED_WRAP,    :onCmdWrapFixed)
    FXMAPFUNC(SEL_DND_MOTION,         ID_TEXT,          :onEditDNDMotion)
    FXMAPFUNC(SEL_DND_DROP,           ID_TEXT,          :onEditDNDDrop)
    FXMAPFUNC(SEL_UPDATE,             ID_STRIP_CR,      :onUpdStripReturns)
    FXMAPFUNC(SEL_COMMAND,            ID_STRIP_CR,      :onCmdStripReturns)
    FXMAPFUNC(SEL_UPDATE,             ID_STRIP_SP,      :onUpdStripSpaces)
    FXMAPFUNC(SEL_COMMAND,            ID_STRIP_SP,      :onCmdStripSpaces)
    FXMAPFUNC(SEL_COMMAND,            ID_INCLUDE_PATH,  :onCmdIncludePaths)
    FXMAPFUNC(SEL_COMMAND,            ID_SHOW_HELP,     :onCmdShowHelp)
    FXMAPFUNC(SEL_COMMAND,            ID_FILEFILTER,    :onCmdFilter)
    FXMAPFUNC(SEL_UPDATE,             ID_OVERSTRIKE,    :onUpdOverstrike)
    FXMAPFUNC(SEL_UPDATE,             ID_READONLY,      :onUpdReadOnly)
    FXMAPFUNC(SEL_UPDATE,             ID_NUMCHARS,      :onUpdNumChars)
    FXMAPFUNC(SEL_COMMAND,            ID_PREFERENCES,   :onCmdPreferences)
    FXMAPFUNC(SEL_COMMAND,            ID_TABCOLUMNS,    :onCmdTabColumns)
    FXMAPFUNC(SEL_UPDATE,             ID_TABCOLUMNS,    :onUpdTabColumns)
    FXMAPFUNC(SEL_COMMAND,            ID_DELIMITERS,    :onCmdDelimiters)
    FXMAPFUNC(SEL_UPDATE,             ID_DELIMITERS,    :onUpdDelimiters)
    FXMAPFUNC(SEL_COMMAND,            ID_WRAPCOLUMNS,   :onCmdWrapColumns)
    FXMAPFUNC(SEL_UPDATE,             ID_WRAPCOLUMNS,   :onUpdWrapColumns)
    FXMAPFUNC(SEL_COMMAND,            ID_AUTOINDENT,    :onCmdAutoIndent)
    FXMAPFUNC(SEL_UPDATE,             ID_AUTOINDENT,    :onUpdAutoIndent)
    FXMAPFUNC(SEL_COMMAND,            ID_INSERTTABS,    :onCmdInsertTabs)
    FXMAPFUNC(SEL_UPDATE,             ID_INSERTTABS,    :onUpdInsertTabs)
    FXMAPFUNC(SEL_COMMAND,            ID_BRACEMATCH,    :onCmdBraceMatch)
    FXMAPFUNC(SEL_UPDATE,             ID_BRACEMATCH,    :onUpdBraceMatch)
    FXMAPFUNC(SEL_UPDATE,             ID_INSERT_FILE,   :onUpdInsertFile)
    FXMAPFUNC(SEL_COMMAND,            ID_INSERT_FILE,   :onCmdInsertFile)
    FXMAPFUNC(SEL_UPDATE,             ID_EXTRACT_FILE,  :onUpdExtractFile)
    FXMAPFUNC(SEL_COMMAND,            ID_EXTRACT_FILE,  :onCmdExtractFile)
    FXMAPFUNC(SEL_UPDATE,             ID_WHEELADJUST,   :onUpdWheelAdjust)
    FXMAPFUNC(SEL_COMMAND,            ID_WHEELADJUST,   :onCmdWheelAdjust)

    # Undoable commands
    @undolist = FXUndoList.new

    # Default font
    @font = nil
  
    # Make some icons
    @bigicon = loadIcon("big.png")
    @smallicon = loadIcon("small.png")
    @newicon = loadIcon("filenew.png")
    @openicon = loadIcon("fileopen.png")
    @saveicon = loadIcon("filesave.png")
    @saveasicon = FXPNGIcon.new(getApp(), File.open(File.join("..", "icons", "saveas.png"), "rb").read(),
      0, IMAGE_ALPHAGUESS)
    @printicon = loadIcon("printicon.png")
    @cuticon = loadIcon("cut.png")
    @copyicon = loadIcon("copy.png")
    @pasteicon = loadIcon("paste.png")
    @deleteicon = loadIcon("kill.png")
    @undoicon = loadIcon("undo.png")
    @redoicon = loadIcon("redo.png")
    @fontsicon = loadIcon("fonts.png")
    @helpicon = loadIcon("help.png")

    # Application icons
    setIcon(@bigicon)
    setMiniIcon(@smallicon)
  
    # Make main window; set myself as the target
    setTarget(self)
    setSelector(ID_TITLE)
  
    # Help window
    @helpwindow = HelpWindow.new(self)
  
    # Make menu bar
    dragshell1 = FXToolBarShell.new(self, FRAME_RAISED|FRAME_THICK)
    menubar = FXMenuBar.new(self, dragshell1, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    FXToolBarGrip.new(menubar, menubar, FXMenuBar::ID_TOOLBARGRIP,
      TOOLBARGRIP_DOUBLE)
  
    # Tool bar
    dragshell2 = FXToolBarShell.new(self, FRAME_RAISED|FRAME_THICK)
    toolbar = FXToolBar.new(self, dragshell2,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH|PACK_UNIFORM_HEIGHT)
    FXToolBarGrip.new(toolbar, toolbar, FXToolBar::ID_TOOLBARGRIP,
      TOOLBARGRIP_DOUBLE)
  
    # Status bar
    statusbar = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
  
    # Info about the editor
    FXButton.new(statusbar, "\tThe FOX Text Editor\tAbout the FOX Text Editor.",
      @smallicon, self, ID_ABOUT, LAYOUT_FILL_Y|LAYOUT_RIGHT)
  
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
  
    # Edit Menu
    editmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Edit", nil, editmenu)
  
    # Goto Menu
    gotomenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Goto", nil, gotomenu)
  
    # Search Menu
    searchmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Search", nil, searchmenu)
  
    # Options Menu
    optionmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Options", nil, optionmenu)
  
    # View menu
    viewmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&View", nil, viewmenu)
  
    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)
  
    # Splitter
    splitter = FXSplitter.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y|SPLITTER_TRACKING)
  
    # Sunken border for tree
    @treebox = FXVerticalFrame.new(splitter, LAYOUT_FILL_X|LAYOUT_FILL_Y,
      0, 0, 0, 0, 0, 0, 0, 0)
  
    # Make tree
    treeframe = FXHorizontalFrame.new(@treebox,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      0, 0, 0, 0, 0, 0, 0, 0)
    @dirlist = FXDirList.new(treeframe, self, ID_TREELIST,
      (DIRLIST_SHOWFILES|TREELIST_BROWSESELECT|TREELIST_SHOWS_LINES|
       TREELIST_SHOWS_BOXES|LAYOUT_FILL_X|LAYOUT_FILL_Y))
    filterframe = FXHorizontalFrame.new(@treebox, LAYOUT_FILL_X)
    FXLabel.new(filterframe, "Filter:")
    @filter = FXComboBox.new(filterframe, 25, self, ID_FILEFILTER,
      COMBOBOX_STATIC|LAYOUT_FILL_X|FRAME_SUNKEN|FRAME_THICK)
    @filter.numVisible = 4
  
    # Sunken border for text widget
    textbox = FXHorizontalFrame.new(splitter,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y, 0,0,0,0, 0,0,0,0)
  
    # Make editor window
    @editor = FXText.new(textbox, self, ID_TEXT, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @editor.hiliteMatchTime = 300000
  
    # Show readonly state in status bar
    readonly = FXLabel.new(statusbar, nil, nil, FRAME_SUNKEN|LAYOUT_RIGHT|LAYOUT_CENTER_Y)
    readonly.padLeft = 2
    readonly.padRight = 2
    readonly.padTop = 1
    readonly.padBottom = 1
    readonly.setTarget(self)
    readonly.setSelector(ID_READONLY)
  
    # Show insert mode in status bar
    overstrike = FXLabel.new(statusbar, nil, nil, FRAME_SUNKEN|LAYOUT_RIGHT|LAYOUT_CENTER_Y)
    overstrike.padLeft = 2
    overstrike.padRight = 2
    overstrike.padTop = 1
    overstrike.padBottom = 1
    overstrike.setTarget(self)
    overstrike.setSelector(ID_OVERSTRIKE)
  
    # Show size of text in status bar
    numchars = FXTextField.new(statusbar, 6, self, ID_NUMCHARS,
      FRAME_SUNKEN|JUSTIFY_RIGHT|LAYOUT_RIGHT|LAYOUT_CENTER_Y,
      0, 0, 0, 0, 2, 2, 1, 1)
    numchars.backColor = statusbar.backColor
  
    # Caption before number
    FXLabel.new(statusbar, "  Size:", nil, LAYOUT_RIGHT|LAYOUT_CENTER_Y)
  
    # Show column number in status bar
    columnno = FXTextField.new(statusbar, 4, @editor, FXText::ID_CURSOR_COLUMN,
      FRAME_SUNKEN|JUSTIFY_RIGHT|LAYOUT_RIGHT|LAYOUT_CENTER_Y,
      0, 0, 0, 0, 2, 2, 1, 1)
    columnno.backColor = statusbar.backColor
  
    # Caption before number
    FXLabel.new(statusbar, "  Col:", nil, LAYOUT_RIGHT|LAYOUT_CENTER_Y)
  
    # Show line number in status bar
    rowno = FXTextField.new(statusbar, 4, @editor, FXText::ID_CURSOR_ROW,
      FRAME_SUNKEN|JUSTIFY_RIGHT|LAYOUT_RIGHT|LAYOUT_CENTER_Y,
      0, 0, 0, 0, 2, 2, 1, 1)
    rowno.backColor = statusbar.backColor
  
    # Caption before number
    FXLabel.new(statusbar, "  Line:", nil, LAYOUT_RIGHT|LAYOUT_CENTER_Y)
  
    # Toobar buttons: File manipulation
    FXButton.new(toolbar, "New\tNew\tCreate new document.", @newicon,
      self, ID_NEW, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Open\tOpen\tOpen document file.", @openicon,
      self, ID_OPEN, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Save\tSave\tSave document.", @saveicon,
      self, ID_SAVE, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Save as\tSave As\tSave document to another file.",
      @saveasicon, self, ID_SAVEAS, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
  
    # Toobar buttons: Print
    FXFrame.new(toolbar,
      LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, 0, 0, 5, 5)
    FXButton.new(toolbar, "Print\tPrint\tPrint document.", @printicon,
      self, ID_PRINT, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
  
    # Toobar buttons: Editing
    FXFrame.new(toolbar,
      LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, 0, 0, 5, 5)
    FXButton.new(toolbar, "Cut\tCut\tCut selection to clipboard.", @cuticon,
      @editor, FXText::ID_CUT_SEL, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Copy\tCopy\tCopy selection to clipboard.", @copyicon,
      @editor, FXText::ID_COPY_SEL, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Paste\tPaste\tPaste clipboard.", @pasteicon,
      @editor, FXText::ID_PASTE_SEL, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Undo\tUndo\tUndo last change.", @undoicon,
      @undolist, FXUndoList::ID_UNDO, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
    FXButton.new(toolbar, "Redo\tRedo\tRedo last undo.", @redoicon,
      @undolist, FXUndoList::ID_REDO, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_LEFT))
  
    # Color
    FXFrame.new(toolbar,
      LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, 0, 0, 5, 5)
    FXButton.new(toolbar, "Fonts\tFonts\tDisplay font dialog.", @fontsicon,
      self, ID_FONT, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|
      LAYOUT_TOP|LAYOUT_LEFT))
 
    FXButton.new(toolbar, "Help\tHelp on editor\tDisplay help information.",
      @helpicon, self, ID_SHOW_HELP, (ICON_ABOVE_TEXT|BUTTON_TOOLBAR|
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_RIGHT))
  
    # File Menu entries
    FXMenuCommand.new(filemenu, "&Open...        \tCtl-O\tOpen document file.",
      @openicon, self, ID_OPEN)
    FXMenuCommand.new(filemenu,
      "Open Selected...   \tCtl-E\tOpen highlighted document file.", nil,
      self, ID_OPEN_SELECTED)
    FXMenuCommand.new(filemenu, "&Reopen...\t\tReopen file.", nil,
      self, ID_REOPEN)
    FXMenuCommand.new(filemenu, "&New...\tCtl-N\tCreate new document.",
      @newicon, self, ID_NEW)
    FXMenuCommand.new(filemenu, "&Save\tCtl-S\tSave changes to file.",
      @saveicon, self, ID_SAVE)
    FXMenuCommand.new(filemenu, "Save &As...\t\tSave document to another file.",
      @saveasicon, self, ID_SAVEAS)
    FXMenuSeparator.new(filemenu)
    FXMenuCommand.new(filemenu, "Insert from file...\t\tInsert text from file.",
      nil, self, ID_INSERT_FILE)
    FXMenuCommand.new(filemenu, "Extract to file...\t\tExtract text to file.",
      nil, self, ID_EXTRACT_FILE)
    FXMenuCommand.new(filemenu, "&Print...\tCtl-P\tPrint document.", @printicon,
      self, ID_PRINT)
    FXMenuCommand.new(filemenu, "&Editable\t\tDocument editable.", nil,
      @editor, FXText::ID_TOGGLE_EDITABLE)
    iconifyCmd = FXMenuCommand.new(filemenu, "&Iconify...\t\tIconify editor.")
    iconifyCmd.connect(SEL_COMMAND) { self.minimize }
  
    # Recent file menu; this automatically hides if there are no files
    sep1 = FXMenuSeparator.new(filemenu)
    sep1.setTarget(@mrufiles)
    sep1.setSelector(FXRecentFiles::ID_ANYFILES)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_1)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_2)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_3)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_4)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_5)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_6)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_7)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_8)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_9)
    FXMenuCommand.new(filemenu, nil, nil, @mrufiles, FXRecentFiles::ID_FILE_10)
    FXMenuCommand.new(filemenu, "&Clear Recent Files", nil,
      @mrufiles, FXRecentFiles::ID_CLEAR)
    sep2 = FXMenuSeparator.new(filemenu)
    sep2.setTarget(@mrufiles)
    sep2.setSelector(FXRecentFiles::ID_ANYFILES)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil, self, ID_QUIT)
  
    # Edit Menu entries
    FXMenuCommand.new(editmenu, "&Undo\tCtl-Z\tUndo last change.", @undoicon,
      @undolist, FXUndoList::ID_UNDO)
    FXMenuCommand.new(editmenu, "&Redo\tCtl-Y\tRedo last undo.", @redoicon,
      @undolist, FXUndoList::ID_REDO)
    FXMenuCommand.new(editmenu, "&Undo all\t\tUndo all.", nil,
      @undolist, FXUndoList::ID_UNDO_ALL)
    FXMenuCommand.new(editmenu, "&Redo all\t\tRedo all.", nil,
      @undolist, FXUndoList::ID_REDO_ALL)
    FXMenuCommand.new(editmenu, "&Revert to saved\t\tRevert to saved.", nil,
      @undolist, FXUndoList::ID_REVERT)
    FXMenuSeparator.new(editmenu)
    FXMenuCommand.new(editmenu, "&Copy\tCtl-C\tCopy selection to clipboard.",
      @copyicon, @editor, FXText::ID_COPY_SEL)
    FXMenuCommand.new(editmenu, "Cu&t\tCtl-X\tCut selection to clipboard.",
      @cuticon, @editor, FXText::ID_CUT_SEL)
    FXMenuCommand.new(editmenu, "&Paste\tCtl-V\tPaste from clipboard.",
      @pasteicon, @editor, FXText::ID_PASTE_SEL)
    FXMenuCommand.new(editmenu, "&Delete\t\tDelete selection.", @deleteicon,
      @editor, FXText::ID_DELETE_SEL)
    FXMenuSeparator.new(editmenu)
    FXMenuCommand.new(editmenu, "Lo&wer-case\tCtl-6\tChange to lower case.",
      nil, @editor, FXText::ID_LOWER_CASE)
    FXMenuCommand.new(editmenu,
      "Upp&er-case\tShift-Ctl-^\tChange to upper case.", nil,
      @editor, FXText::ID_UPPER_CASE)
    FXMenuCommand.new(editmenu,
      "Clean indent\t\tClean indentation to either all tabs or all spaces.",
      nil, @editor, FXText::ID_CLEAN_INDENT)
    FXMenuCommand.new(editmenu, "Shift left\tCtl-9\tShift text left.", nil,
      @editor, FXText::ID_SHIFT_LEFT)
    FXMenuCommand.new(editmenu, "Shift right\tCtl-0\tShift text right.", nil,
      @editor, FXText::ID_SHIFT_RIGHT)
    FXMenuCommand.new(editmenu,
      "Shift tab left\t\tShift text left one tab position.", nil,
      @editor, FXText::ID_SHIFT_TABLEFT)
    FXMenuCommand.new(editmenu,
      "Shift tab right\t\tShift text right one tab position.", nil,
      @editor, FXText::ID_SHIFT_TABRIGHT)
 
    # Goto Menu entries
    FXMenuCommand.new(gotomenu,
      "&Goto...\tCtl-G\tGoto line number.", nil,
      @editor, FXText::ID_GOTO_LINE)
    FXMenuCommand.new(gotomenu,
      "Goto selected...\tCtl-L\tGoto selected line number.", nil,
      @editor, FXText::ID_GOTO_SELECTED)
    FXMenuSeparator.new(gotomenu)
    FXMenuCommand.new(gotomenu,
      "Goto {..\tShift-Ctl-{\tGoto start of enclosing block.", nil,
      @editor, FXText::ID_LEFT_BRACE)
    FXMenuCommand.new(gotomenu,
      "Goto ..}\tShift-Ctl-}\tGoto end of enclosing block.", nil,
      @editor, FXText::ID_RIGHT_BRACE)
    FXMenuCommand.new(gotomenu,
      "Goto (..\tShift-Ctl-(\tGoto start of enclosing expression.", nil,
      @editor, FXText::ID_LEFT_PAREN)
    FXMenuCommand.new(gotomenu,
      "Goto ..)\tShift-Ctl-)\tGoto end of enclosing expression.", nil,
      @editor, FXText::ID_RIGHT_PAREN)
    FXMenuSeparator.new(gotomenu)
    FXMenuCommand.new(searchmenu,
      "Goto matching (..)\tCtl-M\tGoto matching brace or parenthesis.", nil,
      @editor, FXText::ID_GOTO_MATCHING)
  
    # Search Menu entries
    FXMenuCommand.new(searchmenu,
      "Select matching (..)\tShift-Ctl-M\tSelect matching brace or parenthesis.", nil,
      @editor, FXText::ID_SELECT_MATCHING)
    FXMenuCommand.new(searchmenu,
      "Select block {..}\tShift-Alt-{\tSelect enclosing block.", nil,
      @editor, FXText::ID_SELECT_BRACE)
    FXMenuCommand.new(searchmenu,
      "Select block {..}\tShift-Alt-}\tSelect enclosing block.", nil,
      @editor, FXText::ID_SELECT_BRACE)
    FXMenuCommand.new(searchmenu,
      "Select expression (..)\tShift-Alt-(\tSelect enclosing parentheses.", nil,
      @editor, FXText::ID_SELECT_PAREN)
    FXMenuCommand.new(searchmenu,
      "Select expression (..)\tShift-Alt-)\tSelect enclosing parentheses.", nil,
      @editor, FXText::ID_SELECT_PAREN)
    FXMenuSeparator.new(searchmenu)
    FXMenuCommand.new(searchmenu,
      "&Search sel. fwd\tCtl-H\tSearch for selection.", nil,
      @editor, FXText::ID_SEARCH_FORW_SEL)
    FXMenuCommand.new(searchmenu,
      "&Search sel. bck\tShift-Ctl-H\tSearch for selection.", nil,
      @editor, FXText::ID_SEARCH_BACK_SEL)
    FXMenuCommand.new(searchmenu,
      "&Search...\tCtl-F\tSearch for a string.", nil,
      @editor, FXText::ID_SEARCH)
    FXMenuCommand.new(searchmenu,
      "R&eplace...\tCtl-R\tSearch for a string.", nil,
      @editor, FXText::ID_REPLACE)
  
    # Options menu
    FXMenuCommand.new(optionmenu,
      "Preferences...\t\tChange preferences.", nil,
      self, TextWindow::ID_PREFERENCES)
    FXMenuCommand.new(optionmenu,
      "Font...\t\tChange text font.", @fontsicon, self, ID_FONT)
    FXMenuCommand.new(optionmenu,
      "Overstrike\t\tToggle overstrike mode.", nil,
      @editor, FXText::ID_TOGGLE_OVERSTRIKE)
    FXMenuCommand.new(optionmenu,
      "Include path...\t\tDirectories to search for include files.", nil,
      self, TextWindow::ID_INCLUDE_PATH)
    FXMenuCommand.new(optionmenu,
      "Save Settings...\t\tSave settings now.", nil,
      self, TextWindow::ID_SAVE_SETTINGS)
  
    # View Menu entries
    FXMenuCommand.new(viewmenu,
      "Hidden files\t\tShow hidden files and directories.", nil,
      @dirlist, FXDirList::ID_TOGGLE_HIDDEN)
    FXMenuCommand.new(viewmenu,
      "File Browser\t\tDisplay file list.", nil,
      @treebox,FXWindow::ID_TOGGLESHOWN)
    FXMenuCommand.new(viewmenu, "Toolbar\t\tDisplay toolbar.", nil,
      toolbar, FXWindow::ID_TOGGLESHOWN)
    FXMenuCommand.new(viewmenu, "Status line\t\tDisplay status line.", nil,
      statusbar, FXWindow::ID_TOGGLESHOWN)
  
    # Help Menu entries
    FXMenuCommand.new(helpmenu, "&Help...\t\tDisplay help information.",
      @helpicon, self, ID_SHOW_HELP, 0)
    FXMenuSeparator.new(helpmenu)
    FXMenuCommand.new(helpmenu, "&About TextEdit...\t\tDisplay about panel.",
      @smallicon, self, ID_ABOUT, 0)
  
    # Make a tool tip
    FXToolTip.new(getApp(), 0)
  
    # Recent files
    @mrufiles = FXRecentFiles.new
    @mrufiles.setTarget(self)
    @mrufiles.setSelector(ID_RECENTFILE)
  
    # Add some alternative accelerators
    if getAccelTable()
      getAccelTable().addAccel(MKUINT(KEY_Z, CONTROLMASK|SHIFTMASK),
                               @undolist,
                               MKUINT(FXUndoList::ID_REDO, SEL_COMMAND))
    end
  
    # Initialize file name
    @filename = "untitled"
    @filetime = nil
    @filenameset = false
  
    # Initialize other stuff
    @searchpath = "/usr/include"
    setPatterns(["All Files (*)"])
    setCurrentPattern(0)
    @timer = nil
    @stripcr = true
    @stripsp = false
    @undolist.mark
  end


  # Load file
  def loadFile(file)
    begin
      getApp().beginWaitCursor()
      text = File.open(file, "r").read
      text.gsub!('\r', '') if @stripcr
      @editor.text = text
    ensure
      getApp().endWaitCursor()
    end

    # Set stuff
    @editor.modified = false
    @editor.editable = File.stat(file).writable?
    @dirlist.currentFile = file
    @mrufiles.appendFile(file)
    @filetime = File.mtime(file)
    @filename = file
    @filenameset = true
    @undolist.clear
    @undolist.mark
  end


  # Insert file
  def insertFile(file)
    begin
      getApp().beginWaitCursor()
      text = File.open(file, "r").read
      text.gsub!('\r', '') if @stripcr
      @editor.insertText(@editor.cursorPos, text, n, true)
      @editor.modified = true
    ensure
      getApp().endWaitCursor()
    end
  end


  # Save file
  def saveFile(file)
    # Set wait cursor
    getApp().beginWaitCursor()

    # Get text from editor
    text = @editor.text

    # Strip trailing spaces
    if @stripsp
      lines = text.split('\n')
      lines.each { |line|
        line.sub!(/ *$/, "")
      }
      text = lines.join('\n')
    end

    # Write the file
    File.open(file, "w").write(text)

    # Kill wait cursor
    getApp().endWaitCursor()

    # Set stuff
    @editor.modified = false
    @editor.editable = true
    @dirlist.currentFile = file
    @mrufiles.appendFile(file)
    @filetime = File.mtime(file)
    @filename = file
    @filenameset = true
    @undolist.mark
  end


  # Extract file
  def extractFile(file)
    # Set wait cursor
    getApp().beginWaitCursor()

    # Get text from editor
    size = @editor.selEndPos - @editor.selStartPos
    text = @editor.extractText(@editor.selStartPos, size)

    # Strip trailing spaces
    if @stripsp
      lines = text.split('\n')
      lines.each { |line|
        line.sub!(/ *$/, "")
      }
      text = lines.join('\n')
    end

    # Write the file
    File.open(file, "w").write(text)

    # Kill wait cursor
    getApp().endWaitCursor()
  end


  # Read settings from registry
  def readRegistry
    # Text colors
    textback    = getApp().reg().readColorEntry("SETTINGS", "textbackground", @editor.backColor)
    textfore    = getApp().reg().readColorEntry("SETTINGS", "textforeground", @editor.textColor)
    textselback = getApp().reg().readColorEntry("SETTINGS", "textselbackground", @editor.selBackColor)
    textselfore = getApp().reg().readColorEntry("SETTINGS", "textselforeground", @editor.selTextColor)
    textcursor  = getApp().reg().readColorEntry("SETTINGS", "textcursor", @editor.cursorColor)

    # Directory colors
    dirback    = getApp().reg().readColorEntry("SETTINGS", "browserbackground", @dirlist.backColor)
    dirfore    = getApp().reg().readColorEntry("SETTINGS", "browserforeground", @dirlist.textColor)
    dirselback = getApp().reg().readColorEntry("SETTINGS", "browserselbackground", @dirlist.selBackColor)
    dirselfore = getApp().reg().readColorEntry("SETTINGS", "browserselforeground", @dirlist.selTextColor)
    dirlines   = getApp().reg().readColorEntry("SETTINGS", "browserlines", @dirlist.lineColor)

    # Delimiters
    delimiters = getApp().reg().readStringEntry("SETTINGS", "delimiters", '~.,/\\`\'!@#$%^&*()-=+{}|[]":;<>?')

    # Font
    fontspec = getApp().reg().readStringEntry("SETTINGS", "font", "")
    if fontspec != ""
      font = FXFont.new(getApp(), fontspec)
      @editor.font = font
    end

    # Get size
    xx = getApp().reg().readIntEntry("SETTINGS", "x", 5)
    yy = getApp().reg().readIntEntry("SETTINGS", "y", 5)
    ww = getApp().reg().readIntEntry("SETTINGS", "width", 600)
    hh = getApp().reg().readIntEntry("SETTINGS", "height", 400)

    # Hidden files shown
    hiddenfiles = getApp().reg().readIntEntry("SETTINGS", "showhiddenfiles", 0)
    @dirlist.hiddenFilesShown = (hiddenfiles != 0) ? true : false

    # Showing the tree?
    hidetree = getApp().reg().readIntEntry("SETTINGS", "hidetree", 1)

    # Width of tree
    treewidth = getApp().reg().readIntEntry("SETTINGS", "treewidth", 100)

    # Word wrapping
    wrapping = getApp().reg().readIntEntry("SETTINGS", "wordwrap", 0)
    wrapcols = getApp().reg().readIntEntry("SETTINGS", "wrapcols", 80)
    fixedwrap = getApp().reg().readIntEntry("SETTINGS", "fixedwrap", 1)

    # Tab settings, autoindent
    autoindent = getApp().reg().readIntEntry("SETTINGS", "autoindent", 0)
    hardtabs = getApp().reg().readIntEntry("SETTINGS", "hardtabs", 1)
    tabcols = getApp().reg().readIntEntry("SETTINGS", "tabcols", 8)

    # Strip returns
    @stripcr = getApp().reg().readIntEntry("SETTINGS", "stripreturn", 0)
    @stripcr = (@stripcr != 0) ? true : false
    @stripsp = getApp().reg().readIntEntry("SETTINGS", "stripspaces", 0)
    @stripsp = (@stripsp != 0) ? true : false

    # File patterns
    patterns = getApp().reg().readStringEntry("SETTINGS", "filepatterns", "All Files (*)")
    setPatterns(patterns.split("\n")) 
    setCurrentPattern(getApp().reg().readIntEntry("SETTINGS", "filepatternno", 0))

    # Search path
    searchpath = getApp().reg().readStringEntry("SETTINGS", "searchpath", "/usr/include")

    # Change the colors
    @editor.textColor    = textfore
    @editor.backColor    = textback
    @editor.selBackColor = textselback
    @editor.selTextColor = textselfore
    @editor.cursorColor  = textcursor
  
    @dirlist.textColor    = dirfore
    @dirlist.backColor    = dirback
    @dirlist.selBackColor = dirselback
    @dirlist.selTextColor = dirselfore
    @dirlist.lineColor    = dirlines
  
    # Change delimiters
    @editor.delimiters = delimiters
  
    # Hide tree if asked for
    @treebox.hide if hidetree
  
    # Set tree width
    @treebox.width = treewidth
  
    # Open toward file
    @dirlist.currentFile = @filename
  
    # Wrap mode
    if wrapping
      @editor.textStyle |=  TEXT_WORDWRAP
    else
      @editor.textStyle &= ~TEXT_WORDWRAP
    end
  
    # Wrap fixed mode
    if fixedwrap
      @editor.textStyle |=  TEXT_FIXEDWRAP
    else
      @editor.textStyle &= ~TEXT_FIXEDWRAP
    end
  
    # Autoindent
    if autoindent
      @editor.textStyle |=  TEXT_AUTOINDENT
    else
      @editor.textStyle &= ~TEXT_AUTOINDENT
    end
  
    # Hard tabs
    if hardtabs
      @editor.textStyle &= ~TEXT_NO_TABS
    else
      @editor.textStyle |=  TEXT_NO_TABS
    end
  
    # Wrap and tab columns
    @editor.wrapColumns = wrapcols
    @editor.tabColumns = tabcols
  
    # Reposition window
    position(xx, yy, ww, hh)
  end


  # Save settings to registry
  def writeRegistry
    # Colors of text
    getApp().reg().writeColorEntry("SETTINGS", "textbackground", @editor.backColor)
    getApp().reg().writeColorEntry("SETTINGS", "textforeground", @editor.textColor)
    getApp().reg().writeColorEntry("SETTINGS", "textselbackground", @editor.selBackColor)
    getApp().reg().writeColorEntry("SETTINGS", "textselforeground", @editor.selTextColor)
    getApp().reg().writeColorEntry("SETTINGS", "textcursor", @editor.cursorColor)
  
    # Colors of directory
    getApp().reg().writeColorEntry("SETTINGS", "browserbackground", @dirlist.backColor)
    getApp().reg().writeColorEntry("SETTINGS", "browserforeground", @dirlist.textColor)
    getApp().reg().writeColorEntry("SETTINGS", "browserselbackground", @dirlist.selBackColor)
    getApp().reg().writeColorEntry("SETTINGS", "browserselforeground", @dirlist.selTextColor)
    getApp().reg().writeColorEntry("SETTINGS", "browserlines", @dirlist.lineColor)
  
    # Delimiters
    getApp().reg().writeStringEntry("SETTINGS", "delimiters", @editor.delimiters)
  
    # Write new window size back to registry
    getApp().reg().writeIntEntry("SETTINGS", "x", getX())
    getApp().reg().writeIntEntry("SETTINGS", "y", getY())
    getApp().reg().writeIntEntry("SETTINGS", "width", getWidth())
    getApp().reg().writeIntEntry("SETTINGS", "height", getHeight())
  
    # Were showing hidden files
    getApp().reg().writeIntEntry("SETTINGS", "showhiddenfiles", @dirlist.hiddenFilesShown? ? 1 : 0)
  
    # Was tree shown?
    getApp().reg().writeIntEntry("SETTINGS", "hidetree", @treebox.shown() ? 0 : 1)
  
    # Width of tree
    getApp().reg().writeIntEntry("SETTINGS", "treewidth", @treebox.getWidth())
  
    # Wrap mode
    getApp().reg().writeIntEntry("SETTINGS", "wordwrap", (@editor.textStyle & TEXT_WORDWRAP) != 0 ? 1 : 0)
    getApp().reg().writeIntEntry("SETTINGS", "fixedwrap", (@editor.textStyle & TEXT_FIXEDWRAP) != 0 ? 1 : 0)
    getApp().reg().writeIntEntry("SETTINGS", "wrapcols", @editor.getWrapColumns())
  
    # Tab settings, autoindent
    getApp().reg().writeIntEntry("SETTINGS", "autoindent", (@editor.textStyle & TEXT_AUTOINDENT) != 0 ? 1 : 0)
    getApp().reg().writeIntEntry("SETTINGS", "hardtabs", (@editor.textStyle & TEXT_NO_TABS) == 0 ? 1 : 0)
    getApp().reg().writeIntEntry("SETTINGS", "tabcols", @editor.getTabColumns())
  
    # Strip returns
    getApp().reg().writeIntEntry("SETTINGS", "stripreturn", @stripcr ? 1 : 0)
    getApp().reg().writeIntEntry("SETTINGS", "stripspaces", @stripsp ? 1 : 0)
  
    # File patterns
    getApp().reg().writeIntEntry("SETTINGS", "filepatternno", getCurrentPattern())
    patterns = getPatterns().join("\n")
    getApp().reg().writeStringEntry("SETTINGS", "filepatterns", patterns)
  
    # Search path
    getApp().reg().writeStringEntry("SETTINGS", "searchpath", @searchpath)
  
    # Font
    getApp().reg().writeStringEntry("SETTINGS", "font", @editor.font.font)
  end

  # About box
  def onCmdAbout(sender, sel, ptr)
    about = FXMessageBox.new(self, "FOX Text Editor",
      "The FOX Text Editor\n\nUsing FOX Library Version #{fxversion[0]}.#{fxversion[1]}.#{fxversion[2]}\n\nCopyright (C) 2000,2001 Jeroen van der Zijp (jvz@cfdrc.com)", @bigicon, MBOX_OK|DECOR_TITLE|DECOR_BORDER)
    about.execute
    return 1
  end


  # Change font
  def onCmdFont(sender, sel, ptr)
    fontdlg = FXFontDialog.new(self, "Change Font", DECOR_BORDER|DECOR_TITLE)
    fontdesc = @editor.font.fontDesc
    fontdlg.fontSelection = fontdesc
    if fontdlg.execute() != 0
      fontdesc = fontdlg.fontSelection
      font = FXFont.new(getApp(), fontdesc)
      font.create
      @editor.font = font
      @editor.update
    end
    return 1
  end


  # Save settings
  def onCmdSaveSettings(sender, sel, ptr)
    writeRegistry();
    getApp().reg().write
    return 1
  end


  # Toggle wrap mode
  def onCmdWrap(sender, sel, ptr)
    @editor.textStyle ^= TEXT_WORDWRAP
    return 1
  end


  # Update toggle wrap mode
  def onUpdWrap(sender, sel, ptr)
    if (@editor.textStyle & TEXT_WORDWRAP) != 0
      sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
    end
    return 1
  end


  # Toggle fixed wrap mode
  def onCmdWrapFixed(sender, sel, ptr)
    @editor.textStyle ^= TEXT_FIXEDWRAP
    return 1
  end


  # Update toggle fixed wrap mode
  def onUpdWrapFixed(sender, sel, ptr)
    if (@editor.textStyle & TEXT_FIXEDWRAP) != 0
      sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
    end
    return 1
  end

  # Toggle strip returns mode
  def onCmdStripReturns(sender, sel, ptr)
    @stripcr = !@stripcr
    return 1
  end


  # Update toggle strip returns mode
  def onUpdStripReturns(sender, sel, ptr)
    if @stripcr
      sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
    end
    return 1
  end


  # Toggle strip spaces mode
  def onCmdStripSpaces(sender, sel, ptr)
    @stripsp = !@stripsp
    return 1
  end


  # Update toggle strip spaces mode
  def onUpdStripSpaces(sender, sel, ptr)
    if @stripsp
      sender.handle(self, MKUINT(ID_CHECK, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
    end
    return 1
  end


  # Reopen file
  def onCmdReopen(sender, sel, ptr)
    if !@undolist.marked?
      if FXMessageBox.question(self, MBOX_YES_NO, "Document was changed",
        "Discard changes to this document?") == MBOX_CLICKED_NO
           return 1
      end
    end
    loadFile(@filename)
    return 1
  end


  # Update reopen file
  def onUpdReopen(sender, sel, ptr)
    if @filenameset
      sender.handle(self, MKUINT(ID_ENABLE, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_DISABLE, SEL_COMMAND), nil)
    end
    return 1
  end


  # Save changes, prompt for new filename
  def saveChanges
    if !@undolist.marked?
      answer = FXMessageBox.question(self, MBOX_YES_NO_CANCEL,
        "Unsaved Document", "Save current document to file?")
      return false if (answer == MBOX_CLICKED_CANCEL)
      if answer == MBOX_CLICKED_YES
        file = @filename
        if !@filenameset
          savedialog = FXFileDialog.new(self, "Save Document")
          savedialog.selectMode = SELECTFILE_ANY
          savedialog.patternList = getPatterns()
          savedialog.currentPattern = getCurrentPattern()
          savedialog.filename = file
          return false if (savedialog.execute == 0)
          setCurrentPattern(savedialog.currentPattern)
          file = savedialog.filename
          if File.exists?(file)
            if MBOX_CLICKED_NO == FXMessageBox.question(self, MBOX_YES_NO,
              "Overwrite Document", "Overwrite existing document: #{file}?")
              return false
            end
          end
        end
        file = savedialog.filename
        saveFile(file)
      end
    end
    true
  end


  # Open
  def onCmdOpen(sender, sel, ptr)
    return 1 if !saveChanges()
    opendialog = FXFileDialog.new(self, "Open Document")
    opendialog.selectMode = SELECTFILE_EXISTING
    opendialog.patternList = getPatterns()
    opendialog.currentPattern = getCurrentPattern()
    opendialog.filename = @filename
    if opendialog.execute != 0
      setCurrentPattern(opendialog.currentPattern)
      loadFile(opendialog.filename)
    end
    return 1
  end


  # Insert file into buffer
  def onCmdInsertFile(sender, sel, ptr)
    opendialog = FXFileDialog.new(self, "Open Document")
    opendialog.selectMode = SELECTFILE_EXISTING
    opendialog.patternList = getPatterns()
    opendialog.currentPattern = getCurrentPattern()
    if opendialog.execute != 0
      setCurrentPattern(opendialog.currentPattern)
      insertFile(opendialog.filename)
    end
    return 1
  end


  # Update insert file
  def onUpdInsertFile(sender, sel, ptr)
    if @editor.editable?
      sender.handle(self, MKUINT(ID_ENABLE, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_DISABLE, SEL_COMMAND), nil)
    end
    return 1
  end


  # Extract selection to file
  def onCmdExtractFile(sender, sel, ptr)
    savedialog = FXFileDialog.new(self, "Save Document")
    file = "untitled"
    savedialog.selectMode = SELECTFILE_ANY
    savedialog.patternList = getPatterns()
    savedialog.currentPattern = getCurrentPattern()
    savedialog.filename = file
    if savedialog.execute != 0
      setCurrentPattern(savedialog.currentPattern)
      file = savedialog.filename
      if File.exists?(file)
        if MBOX_CLICKED_NO == FXMessageBox.question(self, MBOX_YES_NO,
          "Overwrite Document", "Overwrite existing document: #{file}?")
          return 1
        end
      end
      extractFile(file)
    end
    return 1
  end


  # Update extract file
  def onUpdExtractFile(sender, sel, ptr)
    if @editor.hasSelection()
      sender.handle(self, MKUINT(ID_ENABLE, SEL_COMMAND), nil)
    else
      sender.handle(self, MKUINT(ID_DISABLE, SEL_COMMAND), nil)
    end
    return 1
  end


  # Open Selected
  def onCmdOpenSelected(sender, sel, ptr)
    string = getDNDData(FROM_SELECTION, stringType)
    return 1 if !string

    if string.length < 1024
      # Where to look for this file?
      if @filename.empty?
        dir = Dir.getwd
      else
        dir = File.dirname(@filename) unless @filename.empty?
      end

      # Strip leading and trailing spaces
      string.strip!

      # Attempt to extract the file name from various forms
      if    string =~ /#include \".*\"/
        file = File.expand_path(name, dir)
        if !File.exists?(file)
          Find.find(@searchpath) { |f| file = f if (f == name) }
        end
      elsif string =~ /#include <.*>/
        file = File.expand_path(name, dir)
        if !File.exists?(file)
          Find.find(@searchpath) { |f| file = f if (f == name) }
        end
      elsif string =~ /.*:.*:.*/
        file = File.expand_path(name, dir)
        if !File.exists?(file)
          Find.find(@searchpath) { |f| file = f if (f == name) }
        end
      else
        file = File.expand_path(string, dir)
      end

      if File.exists?(file)
        # Different from current file?
        if file != @filename
          # Save old file first
          return 1 if !saveChanges()

          # Open the new file
          loadFile(file)
        end

        # Switch line number only
        if lineno != 0
          pos = @editor.nextLine(0, lineno - 1)
          @editor.cursorPos = pos
          @editor.centerLine = pos
        end
        return 1
      end
    else
      getApp().beep # string is too long to be a file name
    end
    return 1
  end


  # Open recent file
  def onCmdRecentFile(sender, sel, filename)
    return 1 if !saveChanges()
    loadFile(filename)
    return 1
  end


  # Save
  def onCmdSave(sender, sel, ptr)
    if !@filenameset
      return onCmdSaveAs(sender, sel, ptr)
    end
    saveFile(@filename)
    return 1
  end


  # Save Update
  def onUpdSave(sender, sel, ptr)
    msg = (!@undolist.marked?) ? FXWindow::ID_ENABLE : FXWindow::ID_DISABLE
    sender.handle(self, MKUINT(msg, SEL_COMMAND), nil)
    return 1
  end


  # Save As
  def onCmdSaveAs(sender, sel, ptr)
    savedialog = FXFileDialog.new(self, "Save Document")
    file = @filename
    savedialog.selectMode = SELECTFILE_ANY
    savedialog.patternList = getPatterns()
    savedialog.currentPattern = getCurrentPattern()
    savedialog.filename = file
    if savedialog.execute != 0
      setCurrentPattern(savedialog.currentPattern)
      file = savedialog.filename
      if File.exists?(file)
        if MBOX_CLICKED_NO == FXMessageBox.question(self, MBOX_YES_NO,
          "Overwrite Document", "Overwrite existing document: #{file}?")
          return 1
        end
      end
      saveFile(file)
    end
    return 1
  end


  # New
  def onCmdNew(sender, sel, ptr)
    return 1 if !saveChanges()
    @filename = "untitled"
    @filetime = nil
    @filenameset = false
    @editor.text = nil
    @editor.modified = false
    @editor.editable = true
    @undolist.clear
    @undolist.mark
    return 1
  end


  # Quit
  def onCmdQuit(sender, sel, ptr)
    return 1 if !saveChanges()
    writeRegistry()
    getApp().exit(0)
    return 1
  end


  # Update title
  def onUpdTitle(sender, sel, ptr)
    title = "FOX Text Editor:- " + @filename
    title += "*" if !@undolist.marked?
    sender.handle(self, MKUINT(FXWindow::ID_SETSTRINGVALUE, SEL_COMMAND), title)
    return 1
  end


  # Print the text
  def onCmdPrint(sender, sel, ptr)
    dlg = FXPrintDialog.new(self, "Print File")
    if dlg.execute != 0
      printer = dlg.printer
    end
    return 1
  end


  # Command from the tree list
  def onCmdTreeList(sender, sel, item)
    if !item || !@dirlist.isItemFile(item)
      return 1
    end
    if !saveChanges()
      return 1
    end
    file = @dirlist.getItemPathname(item)
    loadFile(file)
    return 1
  end


  # See if we can get it as a filename
  def onEditDNDDrop(sender, sel, ptr)
    urilist = getDNDData(FROM_DRAGNDROP, FXWindow.urilistType)
    if urilist
      file = FXURL.fileFromURL(urilist.before('\r'))
      return 1 if (file == "")
      return 1 if !saveChanges()
      loadFile(file)
      return 1
    end
    return 0
  end


  # See if a filename is being dragged over the window
  def onEditDNDMotion(sender, sel, ptr)
    if offeredDNDType(FROM_DRAGNDROP, FXWindow.urilistType)
      acceptDrop(DRAG_COPY)
      return 1
    end
    return 0
  end


  # Change both text background color
  def onCmdTextBackColor(sender, sel, color)
    @editor.backColor = color
    return 1
  end


  # Update background color
  def onUpdTextBackColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.backColor)
    return 1
  end


  # Change both text selected background color
  def onCmdTextSelBackColor(sender, sel, color)
    @editor.selBackColor = color
    return 1
  end


  # Update selected background color
  def onUpdTextSelBackColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.selBackColor)
    return 1
  end


  # Change both text and tree text color
  def onCmdTextForeColor(sender, sel, color)
    @editor.textColor = color
    return 1
  end


  # Forward GUI update to text widget
  def onUpdTextForeColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.textColor)
    return 1
  end


  # Change both text and tree text color
  def onCmdTextSelForeColor(sender, sel, color)
    @editor.selTextColor = color
    return 1
  end


  # Forward GUI update to text widget
  def onUpdTextSelForeColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.selTextColor)
    return 1
  end


  # Change cursor color
  def onCmdTextCursorColor(sender, sel, color)
    @editor.cursorColor = color
    return 1
  end


  # Update cursor color
  def onUpdTextCursorColor(sender, sel, ptr)
    sender.handle(self, MKUINT(FXWindow::ID_SETINTVALUE, SEL_COMMAND), @editor.cursorColor)
    return 1
  end


  # Change both tree background color
  def onCmdDirBackColor(sender, sel, color)
    @dirlist.backColor = color
    return 1
  end


  # Update background color
  def onUpdDirBackColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @dirlist.backColor)
    return 1
  end


  # Change both text and tree selected background color
  def onCmdDirSelBackColor(sender, sel, color)
    @dirlist.selBackColor = color
    return 1
  end


  # Update selected background color
  def onUpdDirSelBackColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @dirlist.selBackColor)
    return 1
  end


  # Change both text and tree text color
  def onCmdDirForeColor(sender, sel, color)
    @dirlist.textColor = color
    return 1
  end


  # Forward GUI update to text widget
  def onUpdDirForeColor(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @dirlist.textColor)
    return 1
  end


  # Change both text and tree
  def onCmdDirSelForeColor(sender, sel, color)
    @dirlist.selTextColor = color
    return 1
  end


  # Forward GUI update to text widget
  def onUpdDirSelForeColor(sender, sel, ptr)
    sender.handle(sender, MKUINT(FXWindow::ID_SETINTVALUE, SEL_COMMAND), @dirlist.selTextColor)
    return 1
  end


  # Change both text and tree
  def onCmdDirLineColor(sender, sel, color)
    @dirlist.lineColor = color
    return 1
  end


  # Forward GUI update to text widget
  def onUpdDirLineColor(sender, sel, ptr)
    sender.handle(sender, MKUINT(FXWindow::ID_SETINTVALUE, SEL_COMMAND), @dirlist.lineColor)
    return 1
  end


  # Text inserted
  def onTextInserted(sender, sel, change)
    @undolist.add(FXTextInsert.new(@editor, change.pos))

    # Keep the undo list in check by trimming it down to KEEPUNDOSIZE
    # whenever the amount of undo buffering exceeds MAXUNDOSIZE.
    @undolist.trimSize(KEEPUNDOSIZE) if (@undolist.undoSize() > MAXUNDOSIZE)
    return 1
  end


  # Text deleted
  def onTextDeleted(sender, sel, change)
    @undolist.add(FXTextDelete.new(@editor, change))

    # Keep the undo list in check by trimming it down to KEEPUNDOSIZE
    # whenever the amount of undo buffering exceeds MAXUNDOSIZE.
    @undolist.trimSize(KEEPUNDOSIZE) if (@undolist.undoSize() > MAXUNDOSIZE)
    return 1
  end


  # Text replaced
  def onTextReplaced(sender, sel, change)
    @undolist.add(FXTextReplace.new(@editor, change))

    # Keep the undo list in check by trimming it down to KEEPUNDOSIZE
    # whenever the amount of undo buffering exceeds MAXUNDOSIZE.
    @undolist.trimSize(KEEPUNDOSIZE) if (@undolist.undoSize() > MAXUNDOSIZE)
    return 1
  end


  # Released right button
  def onTextRightMouse(sender, sel, event)
    if !event.moved
      pane = FXMenuPane.new(self)
      FXMenuCommand.new(pane, "Undo", @undoicon, @undolist, FXUndoList::ID_UNDO)
      FXMenuCommand.new(pane, "Redo", @redoicon, @undolist, FXUndoList::ID_REDO)
      FXMenuSeparator.new(pane)
      FXMenuCommand.new(pane, "Cut", @cuticon, @editor, FXText::ID_CUT_SEL)
      FXMenuCommand.new(pane, "Copy", @copyicon, @editor, FXText::ID_COPY_SEL)
      FXMenuCommand.new(pane, "Paste", @pasteicon, @editor, FXText::ID_PASTE_SEL)
      FXMenuCommand.new(pane, "Select All", nil, @editor, FXText::ID_SELECT_ALL)
      pane.create
      pane.popup(nil, event.root_x, event.root_y)
      getApp().runModalWhileShown(pane)
    end
    return 1
  end


  # Set TextWindow path
  def onCmdIncludePaths(sender, sel, ptr)
    searchpath = FXInputDialog::getString(@searchpath, self,
      "Change include file search path",
      "Specify a list of directories separated by a `#{File::PATH_SEPARATOR}'" +
      " where include files are to be found.\nFor example:\n\n" +
      "  /usr/include#{File::PATH_SEPARATOR}/usr/local/include\n\n" +
      "This list will be used to locate the selected file name.")
    @searchpath = searchpath if searchpath != nil
    return 1
  end


  # Change patterns
  def setPatterns(patterns)
    @filter.clearItems
    patterns.each { |pat| @filter.appendItem(pat) }
    if @filter.numItems == 0
      @filter.appendItem("All Files (*)")
    end
    setCurrentPattern(0)
  end


  # Return array of pattern strings
  def getPatterns()
    patterns = []
    @filter.each { |itemText, itemData| patterns.push(itemText) }
    patterns
  end


  # Strip pattern from text if present
  def patternFromText(pattern)
    if pattern =~ /\(.*\)/
      $&[1..2]
    else
      pattern
    end
  end


  # Set current pattern
  def setCurrentPattern(n)
    n = [[0, n].max, @filter.getNumItems() - 1].min
    @filter.currentItem = n
    @dirlist.pattern = patternFromText(@filter.getItemText(n))
  end


  # Return current pattern
  def getCurrentPattern()
    @filter.currentItem
  end


  # Change the pattern
  def onCmdFilter(sender, sel, ptr)
    @dirlist.pattern = patternFromText(ptr)
    return 1
  end


  # Show help window
  def onCmdShowHelp(sender, sel, ptr)
    @helpwindow.show(PLACEMENT_CURSOR)
    return 1
  end


  # Show preferences dialog
  def onCmdPreferences(sender, sel, ptr)
    preferences = PrefDialog.new(self)
    preferences.setPatterns(getPatterns())
    if preferences.execute != 0
      setPatterns(preferences.getPatterns())
    end
    return 1
  end


  # Change tab columns
  def onCmdTabColumns(sender, sel, ptr)
    @editor.tabColumns = sender.text.to_i # sender is an FXTextField
    return 1
  end


  # Update tab columns
  def onUpdTabColumns(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.tabColumns)
    return 1
  end


  # Change wrap columns
  def onCmdWrapColumns(sender, sel, ptr)
    @editor.wrapColumns sender.text.to_i # sender is an FXTextField
    return 1
  end


  # Update wrap columns
  def onUpdWrapColumns(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.wrapColumns)
    return 1
  end


  # Toggle insertion of tabs
  def onCmdInsertTabs(sender, sel, ptr)
    @editor.textStyle ^= TEXT_NO_TABS
    return 1
  end


  # Update insertion of tabs
  def onUpdInsertTabs(sender, sel, ptr)
    sender.handle(self, ((@editor.textStyle & TEXT_NO_TABS) != 0) ?
      MKUINT(ID_UNCHECK, SEL_COMMAND) : MKUINT(ID_CHECK, SEL_COMMAND), nil)
    return 1
  end


  # Toggle autoindent
  def onCmdAutoIndent(sender, sel, ptr)
    @editor.textStyle ^= TEXT_AUTOINDENT
    return 1
  end


  # Update autoindent
  def onUpdAutoIndent(sender, sel, ptr)
    sender.handle(self, ((@editor.textStyle & TEXT_AUTOINDENT) != 0) ?
      MKUINT(ID_CHECK, SEL_COMMAND) : MKUINT(ID_UNCHECK, SEL_COMMAND), nil)
    return 1
  end


  # Set brace match time
  def onCmdBraceMatch(sender, sel, ptr)
    @editor.hiliteMatchTime = sender.text.to_i # sender is an FXTextField
    return 1
  end


  # Update brace match time
  def onUpdBraceMatch(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.hiliteMatchTime)
    return 1
  end


  # Change word delimiters
  def onCmdDelimiters(sender, sel, ptr)
    @editor.delimiters = sender.text # sender is an FXTextField
    return 1
  end


  # Update word delimiters
  def onUpdDelimiters(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETSTRINGVALUE, SEL_COMMAND), @editor.delimiters)
    return 1
  end


  # Update box for overstrike mode display
  def onUpdOverstrike(sender, sel, ptr)
    mode = ((@editor.textStyle & TEXT_OVERSTRIKE) != 0) ? "OVR" : "INS"
    sender.handle(self, MKUINT(ID_SETSTRINGVALUE, SEL_COMMAND), mode)
    return 1
  end


  # Update box for readonly display
  def onUpdReadOnly(sender, sel, ptr)
    rw = ((@editor.textStyle & TEXT_READONLY) != 0) ? "RO" : "RW"
    sender.handle(self, MKUINT(ID_SETSTRINGVALUE, SEL_COMMAND), rw)
    return 1
  end


  # Update box for size display
  def onUpdNumChars(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), @editor.length)
    return 1
  end


  # Update box for readonly display
  def onCheckFile(sender, sel, ptr)
    mtime = File.exists?(@filename) ? File.mtime(@filename) : nil
    if @filetime && mtime && mtime != @filetime
      @filetime = mtime
      if MBOX_CLICKED_OK == FXMessageBox.warning(self, MBOX_OK_CANCEL,
        "File Was Changed",
        "The file was changed by another program\nReload this file from disk?")
        loadFile(@filename)
      end
    end
    @timer = getApp().addTimeout(1000, self, ID_FILETIME)
    return 1
  end


  # Set scroll wheel lines
  def onCmdWheelAdjust(sender, sel, ptr)
    getApp().wheelLines = sender.value # sender is an FXSpinner
    return 1;
  end


  # Update wheel adjustment time
  def onUpdWheelAdjust(sender, sel, ptr)
    sender.handle(self, MKUINT(ID_SETINTVALUE, SEL_COMMAND), getApp().wheelLines)
    return 1
  end


  # Start the ball rolling
  def start(args)
    if args.length > 0
      loadFile(File.expand_path(args[1]))
    end
  end


  # Create and show window
  def create
    @urilistType = getApp().registerDragType(FXWindow.urilistTypeName) unless @urilistType
    readRegistry
    super
    show
    @timer = getApp().addTimeout(1000, self, ID_FILETIME)
  end
end


# Start the whole thing
if __FILE__ == $0
  # Make application
  application = FXApp.new("TextEdit", "FoxTest")
  application.threadsEnabled = false

  # Open display
  application.init(ARGV)

  # Make window
  window = TextWindow.new(application)

  # Handle interrupt to save stuff nicely
  application.addSignal("SIGINT", window, TextWindow::ID_QUIT)

  # Create it
  application.create

  # Start
  window.start(ARGV)

  # Run
  application.run
end
