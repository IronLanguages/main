#!/usr/bin/env ruby

require 'fox16'

include Fox

class ImageWindow < FXMainWindow

  include Responder

  def initialize(app)
    # Invoke base class initialize first
    super(app, "FOX Image Viewer: - untitled", :opts => DECOR_ALL, :width => 850, :height => 600)

    # Recently used files list
    @mrufiles = FXRecentFiles.new

    # Make some icons
    fileopenicon = getIcon("fileopen.png")
    filesaveicon = getIcon("filesave.png")
    cuticon = getIcon("cut.png")
    copyicon = getIcon("copy.png")
    pasteicon = getIcon("paste.png")
    uplevelicon = getIcon("tbuplevel.png")
    paletteicon = getIcon("colorpal.png")

    # Make color dialog
    colordlg = FXColorDialog.new(self, "Color Dialog")
  
    # Make menu bar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X|FRAME_RAISED)
  
    # Status bar
    statusbar = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
  
    # Docking sites
    topDockSite = FXDockSite.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    FXDockSite.new(self, LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X)
    FXDockSite.new(self, LAYOUT_SIDE_LEFT|LAYOUT_FILL_Y)
    FXDockSite.new(self, LAYOUT_SIDE_RIGHT|LAYOUT_FILL_Y)

    # Splitter
    splitter = FXSplitter.new(self, (LAYOUT_SIDE_TOP|LAYOUT_FILL_X|
      LAYOUT_FILL_Y| SPLITTER_TRACKING|SPLITTER_VERTICAL|SPLITTER_REVERSED))
  
    # Tool bar is docked inside the top one for starters
    toolbarShell = FXToolBarShell.new(self)
    toolbar = FXToolBar.new(topDockSite, toolbarShell,
      PACK_UNIFORM_WIDTH|PACK_UNIFORM_HEIGHT|FRAME_RAISED|LAYOUT_FILL_X)
    FXToolBarGrip.new(toolbar, toolbar, FXToolBar::ID_TOOLBARGRIP, TOOLBARGRIP_DOUBLE)

    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Edit Menu
    editmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Edit", nil, editmenu)
  
    # Manipulation Menu
    manipmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar,"&Manipulation", nil, manipmenu)
  
    # View menu
    viewmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&View", nil, viewmenu)
    
    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)

    # Sunken border for image widget
    imagebox = FXHorizontalFrame.new(splitter,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
  
    # Make image widget
    @imageview = FXImageView.new(imagebox, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # Sunken border for file list
    @filebox = FXHorizontalFrame.new(splitter,
      LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
  
    # Make file list
    fileframe = FXHorizontalFrame.new(@filebox,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0, :hSpacing => 0, :vSpacing => 0)
    @filelist = FXFileList.new(fileframe,
      :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|ICONLIST_MINI_ICONS|ICONLIST_AUTOSIZE)
    @filelist.connect(SEL_DOUBLECLICKED, method(:onCmdFileList))
    FXButton.new(@filebox, "\tUp one level\tGo up to higher directory.",
      uplevelicon, @filelist, FXFileList::ID_DIRECTORY_UP,
      BUTTON_TOOLBAR|FRAME_RAISED|LAYOUT_FILL_Y)
  
    # Toobar buttons: File manipulation
    openBtn = FXButton.new(toolbar, "&Open\tOpen Image\tOpen image file.", fileopenicon,
      :opts => ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED)
    openBtn.connect(SEL_COMMAND, method(:onCmdOpen))
    saveBtn = FXButton.new(toolbar, "&Save\tSave Image\tSave image file.", filesaveicon,
      :opts => ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED)
    saveBtn.connect(SEL_COMMAND, method(:onCmdSave))
  
    # Toobar buttons: Editing
    FXButton.new(toolbar, "Cut\tCut", cuticon,
      :opts => ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED)
    FXButton.new(toolbar, "Copy\tCopy", copyicon,
      :opts => ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED)
    FXButton.new(toolbar, "Paste\tPaste", pasteicon,
      :opts => ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED)
  
    # Color
    FXButton.new(toolbar, "&Colors\tColors\tDisplay color dialog.", paletteicon,
      colordlg, FXWindow::ID_SHOW,
      ICON_ABOVE_TEXT|BUTTON_TOOLBAR|FRAME_RAISED|LAYOUT_RIGHT)
  
    # File Menu entries
    FXMenuCommand.new(filemenu, "&Open...\tCtl-O\tOpen image file.", fileopenicon).connect(SEL_COMMAND, method(:onCmdOpen))
    FXMenuCommand.new(filemenu, "&Save...\tCtl-S\tSave image file.", filesaveicon).connect(SEL_COMMAND, method(:onCmdSave))
    FXMenuCommand.new(filemenu, "Dump", nil, getApp(), FXApp::ID_DUMP)
  
    # Recent file menu; this automatically hides if there are no files
    sep1 = FXMenuSeparator.new(filemenu)
    sep1.target = @mrufiles
    sep1.selector = FXRecentFiles::ID_ANYFILES
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
    sep2.target = @mrufiles
    sep2.selector = FXRecentFiles::ID_ANYFILES
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q").connect(SEL_COMMAND, method(:onCmdQuit))
  
    # Edit Menu entries
    FXMenuCommand.new(editmenu, "&Undo\tCtl-Z\tUndo last change.")
    FXMenuCommand.new(editmenu, "&Redo\tCtl-R\tRedo last undo.")
    FXMenuCommand.new(editmenu, "&Copy\tCtl-C\tCopy selection to clipboard.", copyicon)
    FXMenuCommand.new(editmenu, "C&ut\tCtl-X\tCut selection to clipboard.", cuticon)
    FXMenuCommand.new(editmenu, "&Paste\tCtl-V\tPaste from clipboard.", pasteicon)
    FXMenuCommand.new(editmenu, "&Delete\t\tDelete selection.")
  
    # Manipulation Menu entries
    rotate90Cmd = FXMenuCommand.new(manipmenu, "Rotate 90\t\tRotate 90 degrees.")
    rotate90Cmd.connect(SEL_COMMAND) { @imageview.image.rotate(90) }
    rotate90Cmd.connect(SEL_UPDATE, method(:onUpdImage))
    
    rotate180Cmd = FXMenuCommand.new(manipmenu, "Rotate 180\t\tRotate 180 degrees.")
    rotate180Cmd.connect(SEL_COMMAND) { @imageview.image.rotate(180) }
    rotate180Cmd.connect(SEL_UPDATE, method(:onUpdImage))
    
    rotate270Cmd = FXMenuCommand.new(manipmenu, "Rotate -90\t\tRotate -90 degrees.")
    rotate270Cmd.connect(SEL_COMMAND) { @imageview.image.rotate(270) }
    rotate270Cmd.connect(SEL_UPDATE, method(:onUpdImage))

    mirrorHorCmd = FXMenuCommand.new(manipmenu, "Mirror Hor.\t\tMirror Horizontally.")
    mirrorHorCmd.connect(SEL_COMMAND) { @imageview.image.mirror(true, false) }
    mirrorHorCmd.connect(SEL_UPDATE, method(:onUpdImage))
    
    mirrorVerCmd = FXMenuCommand.new(manipmenu, "Mirror Ver.\t\tMirror Vertically.")
    mirrorVerCmd.connect(SEL_COMMAND) { @imageview.image.mirror(false, true) }
    mirrorVerCmd.connect(SEL_UPDATE, method(:onUpdImage))
    
    scaleCmd = FXMenuCommand.new(manipmenu, "Scale...\t\tScale image.")
    scaleCmd.connect(SEL_COMMAND, method(:onCmdScale))
    scaleCmd.connect(SEL_UPDATE, method(:onUpdImage))
    
    cropCmd = FXMenuCommand.new(manipmenu, "Crop...\t\tCrop image.")
    cropCmd.connect(SEL_COMMAND, method(:onCmdCrop))
    cropCmd.connect(SEL_UPDATE, method(:onUpdImage))
  
    # View Menu entries
    FXMenuCheck.new(viewmenu, "File list\t\tDisplay file list.",
      @filebox, FXWindow::ID_TOGGLESHOWN)
    FXMenuCheck.new(viewmenu,
      "Show hidden files\t\tShow hidden files and directories.",
      @filelist, FXFileList::ID_TOGGLE_HIDDEN)
    FXMenuSeparator.new(viewmenu)
    FXMenuRadio.new(viewmenu,
      "Show small icons\t\tDisplay directory with small icons.",
      @filelist, FXFileList::ID_SHOW_MINI_ICONS)
    FXMenuRadio.new(viewmenu,
      "Show big icons\t\tDisplay directory with big icons.",
      @filelist, FXFileList::ID_SHOW_BIG_ICONS)
    FXMenuRadio.new(viewmenu,
      "Show details view\t\tDisplay detailed directory listing.",
      @filelist, FXFileList::ID_SHOW_DETAILS)
    FXMenuSeparator.new(viewmenu)
    FXMenuRadio.new(viewmenu, "Rows of icons\t\tView row-wise.",
      @filelist, FXFileList::ID_ARRANGE_BY_ROWS)
    FXMenuRadio.new(viewmenu, "Columns of icons\t\tView column-wise.",
      @filelist,FXFileList::ID_ARRANGE_BY_COLUMNS)
    FXMenuSeparator.new(viewmenu)
    FXMenuCheck.new(viewmenu, "Toolbar\t\tDisplay toolbar.",
      toolbar, FXWindow::ID_TOGGLESHOWN)
    
    FXMenuCommand.new(viewmenu, "Float toolbar\t\tUndock the toolbar.", nil, toolbar, FXToolBar::ID_DOCK_FLOAT)
    FXMenuCommand.new(viewmenu, "Dock toolbar top\t\tDock the toolbar on the top.", nil, toolbar, FXToolBar::ID_DOCK_TOP)
    FXMenuCommand.new(viewmenu, "Dock toolbar left\t\tDock the toolbar on the left.", nil, toolbar, FXToolBar::ID_DOCK_LEFT)
    FXMenuCommand.new(viewmenu, "Dock toolbar right\t\tDock the toolbar on the right.", nil, toolbar, FXToolBar::ID_DOCK_RIGHT)
    FXMenuCommand.new(viewmenu, "Dock toolbar bottom\t\tDock the toolbar on the bottom.", nil, toolbar, FXToolBar::ID_DOCK_BOTTOM)
    
    FXMenuSeparator.new(viewmenu)
    
    FXMenuCheck.new(viewmenu, "Status line\t\tDisplay status line.",
      statusbar, FXWindow::ID_TOGGLESHOWN)
 
    # Help Menu entries
    FXMenuCommand.new(helpmenu, "&About FOX...").connect(SEL_COMMAND) {
      FXMessageBox.new(self, "About Image Viewer",
        "Image Viewer demonstrates the FOX ImageView widget.\n\n" +
        "Using the FOX C++ GUI Library (http://www.fox-toolkit.org)\n\n" +
        "Copyright (C) 2000 Jeroen van der Zijp (jeroen@fox-toolkit.org)", nil,
        MBOX_OK|DECOR_TITLE|DECOR_BORDER).execute
    }
  
    # Make a tool tip
    FXToolTip.new(getApp(), TOOLTIP_NORMAL)
  
    # Recent files
    @mrufiles.connect(SEL_COMMAND) do |sender, sel, filename|
      @filename = filename
      @filelist.currentFile = @filename
      loadImage(@filename)
    end

    # Initialize file name and pattern for file dialog
    @filename = "untitled"
    @preferredFileFilter = 0
  end

  # Convenience function to construct a PNG icon
  def getIcon(filename)
    begin
      filename = File.join("icons", filename)
      icon = nil
      File.open(filename, "rb") do |f|
        icon = FXPNGIcon.new(getApp(), f.read)
      end
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end

  def hasExtension?(filename, ext)
    File.basename(filename, ext) != File.basename(filename)
  end

  # Load the named image file
  def loadImage(file)
    img = nil
    if hasExtension?(file, ".gif")
      img = FXGIFImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".bmp")
      img = FXBMPImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".xpm")
      img = FXXPMImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".png")
      img = FXPNGImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".jpg")
      img = FXJPGImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".pcx")
      img = FXPCXImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".tif")
      img = FXTIFImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".tga")
      img = FXTGAImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    elsif hasExtension?(file, ".ico")
      img = FXICOImage.new(getApp(), nil, IMAGE_KEEP|IMAGE_SHMI|IMAGE_SHMP)
    end

    # Perhaps failed?
    if img.nil?
      FXMessageBox.error(self, MBOX_OK, "Error loading image",
        "Unsupported image type: #{file}")
      return
    end

    # Load it...
    getApp().beginWaitCursor do
      FXFileStream.open(file, FXStreamLoad) { |stream| img.loadPixels(stream) }
      img.create
      @imageview.image = img
    end
  end

  # Save image to named file
  def saveImage(file)
    getApp().beginWaitCursor do
      FXFileStream.open(file, FXStreamSave) { |stream| @imageview.image.savePixels(stream) }
    end
  end

  # Open a new file
  def onCmdOpen(sender, sel, ptr)
    openDialog = FXFileDialog.new(self, "Open Image")
    openDialog.filename = @filename
    patterns = ["All Files (*)",
                "GIF Image (*.gif)",
                "BMP Image (*.bmp)",
                "XPM Image (*.xpm)",
                "PCX Image (*.pcx)",
                "ICO Image (*.ico)",
                "PNG Image (*.png)",
                "JPEG Image (*.jpg)",
                "TIFF Image (*.tif)",
                "TARGA Image (*.tga)"
    ]
    openDialog.patternList = patterns
    openDialog.currentPattern = @preferredFileFilter
    if openDialog.execute != 0
      @preferredFileFilter = openDialog.currentPattern
      @filename = openDialog.filename
      @filelist.currentFile = @filename
      @mrufiles.appendFile(@filename)
      loadImage(@filename)
    end
    return 1
  end

  # Save this file
  def onCmdSave(sender, sel, ptr)
    saveDialog = FXFileDialog.new(self, "Save Image")
    if saveDialog.execute != 0
      if File.exists? saveDialog.filename
        if MBOX_CLICKED_NO == FXMessageBox.question(self, MBOX_YES_NO,
          "Overwrite Image", "Overwrite existing image?")
          return 1
        end
      end
      @filename = saveDialog.filename
      @filelist.currentFile = @filename
      @mrufiles.appendFile(@filename)
      saveImage(@filename)
    end
    return 1
  end

  # Quit the application
  def onCmdQuit(sender, sel, ptr)
    # Write new window size back to registry
    getApp().reg().writeIntEntry("SETTINGS", "x", x)
    getApp().reg().writeIntEntry("SETTINGS", "y", y)
    getApp().reg().writeIntEntry("SETTINGS", "width", width)
    getApp().reg().writeIntEntry("SETTINGS", "height", height)

    # Height of file list
    getApp().reg().writeIntEntry("SETTINGS", "fileheight", @filebox.height)

    # Was file box shown?
    getApp().reg().writeBoolEntry("SETTINGS", "filesshown", @filebox.shown)

    # Current directory
    getApp().reg().writeStringEntry("SETTINGS", "directory", @filelist.directory)

    # Quit
    getApp().exit(0)
  end

  # Command message from the file list
  def onCmdFileList(sender, sel, index)
    if index >= 0
      if @filelist.isItemDirectory(index)
        @filelist.directory = @filelist.getItemPathname(index)
      elsif @filelist.isItemFile(index)
        @filename = @filelist.getItemPathname(index)
        @mrufiles.appendFile(@filename)
        loadImage(@filename)
      end
    end
    return 1
  end

  # Scale
  def onCmdScale(sender, sel, ptr)
    image = @imageview.image
    sx = FXDataTarget.new(image.width)
    sy = FXDataTarget.new(image.height)
    scalepanel = FXDialogBox.new(self, "Scale Image To Size")
    frame = FXHorizontalFrame.new(scalepanel,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(frame, "W:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, sx, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXLabel.new(frame, "H:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, sy, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXButton.new(frame, "Cancel", nil, scalepanel, FXDialogBox::ID_CANCEL,
      LAYOUT_CENTER_Y|FRAME_RAISED|FRAME_THICK,
      :padLeft => 20, :padRight => 20, :padTop => 4, :padBottom => 4)
    FXButton.new(frame, "OK", nil, scalepanel, FXDialogBox::ID_ACCEPT,
      LAYOUT_CENTER_Y|FRAME_RAISED|FRAME_THICK,
      :padLeft => 30, :padRight => 30, :padTop => 4, :padBottom => 4)
    return 1 if (scalepanel.execute == 0)
    return 1 if (sx.value < 1 || sy.value < 1)
    image.scale(sx.value, sy.value)
    @imageview.image = image
  end

  # Crop
  def onCmdCrop(sender, sel, ptr)
    image = @imageview.image
    cx = FXDataTarget.new(0)
    cy = FXDataTarget.new(0)
    cw = FXDataTarget.new(image.width)
    ch = FXDataTarget.new(image.height)
    croppanel = FXDialogBox.new(self, "Crop image")
    frame = FXHorizontalFrame.new(croppanel,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(frame, "X:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, cx, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXLabel.new(frame, "Y:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, cy, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXLabel.new(frame, "W:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, cw, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXLabel.new(frame, "H:", nil, LAYOUT_CENTER_Y)
    FXTextField.new(frame, 5, ch, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|FRAME_SUNKEN|FRAME_THICK|JUSTIFY_RIGHT)
    FXButton.new(frame, "Cancel", nil, croppanel, FXDialogBox::ID_CANCEL,
      LAYOUT_CENTER_Y|FRAME_RAISED|FRAME_THICK,
      :padLeft => 20, :padRight => 20, :padTop => 4, :padBottom => 4)
    FXButton.new(frame, "OK", nil, croppanel, FXDialogBox::ID_ACCEPT,
      LAYOUT_CENTER_Y|FRAME_RAISED|FRAME_THICK,
      :padLeft => 30, :padRight => 30, :padTop => 4, :padBottom => 4)
    return 1 if (croppanel.execute == 0)
    return 1 if (cx.value < 0 || cy.value < 0 ||
        cx.value+cw.value > image.width ||
        cy.value+ch.value > image.height)
    image.crop(cx.value, cy.value, cw.value, ch.value)
    @imageview.image = image
  end

  # Update image
  def onUpdImage(sender, sel, ptr)
    if @imageview.image
      sender.handle(self, FXSEL(SEL_COMMAND, FXWindow::ID_ENABLE), nil)
    else
      sender.handle(self, FXSEL(SEL_COMMAND, FXWindow::ID_DISABLE), nil)
    end
  end

  # Create and show window
  def create
    # Get size, etc. from registry
    xx = getApp().reg().readIntEntry("SETTINGS", "x", 0)
    yy = getApp().reg().readIntEntry("SETTINGS", "y", 0)
    ww = getApp().reg().readIntEntry("SETTINGS", "width", 850)
    hh = getApp().reg().readIntEntry("SETTINGS", "height", 600)

    fh = getApp().reg().readIntEntry("SETTINGS", "fileheight", 100)
    fs = getApp().reg().readBoolEntry("SETTINGS", "filesshown", true)

    dir = getApp().reg().readStringEntry("SETTINGS", "directory", "~")

    # Starting directory for the files list
    @filelist.directory = dir

    # Height of files list
    @filebox.height = fh

    # Is it visible right now?
    if !fs
      @filebox.hide
    end
 
    # Reposition window to specified x, y, w and h
    position(xx, yy, ww, hh)

    # Create and show
    super   # i.e. FXMainWindow::create()
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("ImageViewer", "FoxTest")

  # Make window
  window = ImageWindow.new(application)

  # Handle interrupts to terminate program gracefully
  application.addSignal("SIGINT", window.method(:onCmdQuit))

  # Create it
  application.create

  # Passed image file?
  if ARGV.length > 0
    window.loadImage(ARGV[0])
  end

  # Run
  application.run
end

