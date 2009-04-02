#!/usr/bin/env ruby

require 'fox16'

include Fox

class GroupWindow < FXMainWindow

  # Convenience function to load & construct an icon
  def getIcon(filename)
    begin
      filename = File.join("icons", filename)
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
    # Call the base class version of initialize
    super(app, "Group Box Test", :opts => DECOR_ALL)

    # Some icons we'll use here and there
    doc = getIcon("minidoc.png")
    folder_open = getIcon("minifolderopen.png")
    folder_closed = getIcon("minifolder.png")

    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "Open any", folder_open).connect(SEL_COMMAND) {
      file = FXFileDialog.getSaveFilename(self, "Save file",
        "../examples/groupbox.rb", @sourcefiles, 1)
    }
    FXMenuCommand.new(filemenu, "Open existing", folder_open).connect(SEL_COMMAND) {
      file = FXFileDialog.getOpenFilename(self, "Open file",
        "../examples/groupbox.rb", @sourcefiles, 3)
    }
    FXMenuCommand.new(filemenu, "Open multiple", folder_open).connect(SEL_COMMAND) do
      files = FXFileDialog.getOpenFilenames(self, "Open file",
        "../examples/groupbox.rb", @sourcefiles)
      FXMessageBox.information(self, MBOX_OK, "Selected Files", files.join("\n"))
    end
    FXMenuCommand.new(filemenu, "Open directory", folder_open).connect(SEL_COMMAND) {
      dir = FXFileDialog.getOpenDirectory(self, "Open directory", "../examples")
    }
    FXMenuCommand.new(filemenu, "Open directory dialog", folder_open).connect(SEL_COMMAND) {
      dirDialog = FXDirDialog.new(self, "Choose a directory")
      if dirDialog.execute != 0
        FXMessageBox.information(self, MBOX_OK, "Selected Directory", dirDialog.directory)
      end
    }
    radio1 = FXMenuRadio.new(filemenu, "Radio&1")
    radio1.connect(SEL_COMMAND, method(:onCmdRadio))
    radio1.connect(SEL_UPDATE,  method(:onUpdRadio))

    radio2 = FXMenuRadio.new(filemenu, "Radio&2")
    radio2.connect(SEL_COMMAND, method(:onCmdRadio))
    radio2.connect(SEL_UPDATE,  method(:onUpdRadio))

    radio3 = FXMenuRadio.new(filemenu, "Radio&3")
    radio3.connect(SEL_COMMAND, method(:onCmdRadio))
    radio3.connect(SEL_UPDATE,  method(:onUpdRadio))

    FXMenuCommand.new(filemenu, "Delete\tCtl-X").connect(SEL_COMMAND) {
      @group2 = nil
    }
    FXMenuCommand.new(filemenu,
      "Downsize\tF5\tResize to minimum").connect(SEL_COMMAND) {
      resize(getDefaultWidth(), getDefaultHeight())
    }
    FXMenuCommand.new(filemenu, "&Size").connect(SEL_COMMAND) {
      resize(getDefaultWidth(), getDefaultHeight())
    }
    FXMenuCommand.new(filemenu, "Dump Widgets", nil, getApp(), FXApp::ID_DUMP)
  
    # Make edit popup menu
    editmenu = FXMenuPane.new(self)
      FXMenuCommand.new(editmenu, "Undo")
      FXMenuCommand.new(editmenu, "Cut")
      submenu1 = FXMenuPane.new(self)
        FXMenuCommand.new(submenu1, "&One")
        FXMenuCommand.new(submenu1, "&Two")
        FXMenuCommand.new(submenu1, "Th&ree")
        FXMenuCommand.new(submenu1, "&Four")
      FXMenuCascade.new(editmenu, "&Submenu1", nil, submenu1)
  
    FXMenuCascade.new(filemenu, "&Edit", nil, editmenu)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    helpmenu = FXMenuPane.new(self)
    FXMenuCommand.new(helpmenu, "&About FOX...").connect(SEL_COMMAND) {
      FXMessageBox.information(self, MBOX_OK,
        "About FOX:- An intentionally long title",
        "FOX is a really, really cool C++ library!\nExample written by Jeroen")
    }
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)
  
    @popupmenu = FXMenuPane.new(self)
      poptext = FXTextField.new(@popupmenu, 10, :opts => FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
      poptext.setText("Popup with text")
    
    # Status bar
    status = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
    @clockLabel = FXLabel.new(status, Time.now().strftime("%I:%M:%S %p"), nil,
      LAYOUT_FILL_Y|LAYOUT_RIGHT|FRAME_SUNKEN)
  
    # Content
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y)
  
    group1 = FXGroupBox.new(contents, "Title Left",
      GROUPBOX_TITLE_LEFT|FRAME_RIDGE|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    group2 = FXGroupBox.new(contents, "Slider Tests",
      GROUPBOX_TITLE_CENTER|FRAME_RIDGE|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    group3 = FXGroupBox.new(contents, "Title Right",
      GROUPBOX_TITLE_RIGHT|FRAME_RIDGE|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    testlabel = FXLabel.new(group1,
      "&This is a multi-line\nlabel widget\nwith a big font", nil,
      LAYOUT_CENTER_X|JUSTIFY_CENTER_X)
    testlabel.setFont(FXFont.new(getApp(), "helvetica", 24, FONTWEIGHT_BOLD,
                                 FONTSLANT_ITALIC, FONTENCODING_DEFAULT))
    FXButton.new(group1, "Small &Button", nil, nil, 0, FRAME_RAISED|FRAME_THICK)
    FXButton.new(group1, "Big Fat Wide Button\nComprising\nthree lines", :opts => FRAME_RAISED|FRAME_THICK)
    FXToggleButton.new(group1,
      "C&losed\tTooltip for closed\tHelp for closed",
      "O&pen\nState\tTooltip for open\tHelp for open",
      folder_closed, folder_open, nil, 0,
      ICON_BEFORE_TEXT|JUSTIFY_LEFT|FRAME_RAISED|FRAME_THICK)
  
    pop = FXPopup.new(self)
    numbers =%w{first second third fourth}
    0.upto(3) do |idx|
      FXOption.new(pop, "#{numbers[idx].capitalize}\tTip #{idx+1}\tHelp #{numbers[idx]}", :opts => JUSTIFY_HZ_APART|ICON_AFTER_TEXT).connect(SEL_COMMAND) {
          FXMessageBox.information(self, MBOX_OK, "Option Menu", "Chose option #{idx+1}")
      }
    end
    
    FXOptionMenu.new(group1, pop,
      LAYOUT_TOP|FRAME_RAISED|FRAME_THICK|JUSTIFY_HZ_APART|ICON_AFTER_TEXT)
  
    FXLabel.new(group1, "Te&kstje", nil, LAYOUT_TOP|JUSTIFY_LEFT)
    FXButton.new(group1,
      "Add an `&&' by doubling\tTooltip\tHelp text for status", :opts => LAYOUT_TOP|FRAME_RAISED|FRAME_THICK)
    FXButton.new(group1, "Te&kstje", :opts => LAYOUT_TOP|FRAME_RAISED|FRAME_THICK).connect(SEL_COMMAND) {
      x, y, buttons = getRoot().getCursorPosition()
      @popupmenu.popup(nil, x, y)
    }
    
    FXMenuButton.new(group1, "&Menu", :opts => MENUBUTTON_ATTACH_BOTH|MENUBUTTON_DOWN|JUSTIFY_HZ_APART|LAYOUT_TOP|FRAME_RAISED|FRAME_THICK|ICON_AFTER_TEXT)
    FXMenuButton.new(group1, "&Menu", nil, filemenu, MENUBUTTON_UP|LAYOUT_TOP|FRAME_RAISED|FRAME_THICK|ICON_AFTER_TEXT)
  
    coolpop = FXPopup.new(self, POPUP_HORIZONTAL)
    FXButton.new(coolpop, "A\tTipA",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 30, :height => 30)
    FXButton.new(coolpop, "B\tTipB",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 30, :height => 30)
    FXButton.new(coolpop, "C\tTipC",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 30, :height => 30)
    FXButton.new(coolpop, "D\tTipD",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 30, :height => 30)
    FXMenuButton.new(group1, "&S\tSideways", nil, coolpop,
      (MENUBUTTON_ATTACH_BOTH|MENUBUTTON_LEFT|MENUBUTTON_NOARROWS|LAYOUT_TOP|
       FRAME_RAISED|FRAME_THICK|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT), :width => 30, :height => 30)
    
    matrix = FXMatrix.new(group1, 3,
      FRAME_RAISED|LAYOUT_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    FXButton.new(matrix, "A", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_ROW)
    FXButton.new(matrix, "&Wide button", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X)
    FXButton.new(matrix, "A", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X)
    
    FXButton.new(matrix, "BBBB", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_ROW|LAYOUT_FILL_COLUMN)
    FXButton.new(matrix, "B", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    FXButton.new(matrix, "BB", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_COLUMN)
    
    FXButton.new(matrix, "C", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_CENTER_X|LAYOUT_FILL_ROW)
    FXButton.new(matrix, "&wide", :opts => FRAME_RAISED|FRAME_THICK)
    FXButton.new(matrix, "CC", :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_RIGHT)
    
    FXLabel.new(group2, "No Arrow")
    FXSlider.new(group2, :opts => LAYOUT_TOP|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|SLIDER_HORIZONTAL, :width => 200, :height => 30)
    
    FXLabel.new(group2, "Up Arrow")
    FXSlider.new(group2, :opts => LAYOUT_TOP|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|SLIDER_HORIZONTAL|SLIDER_ARROW_UP, :width => 200, :height => 30)
    
    FXLabel.new(group2, "Down Arrow")
    FXSlider.new(group2, :opts => LAYOUT_TOP|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT|SLIDER_HORIZONTAL|SLIDER_ARROW_DOWN, :width => 200, :height => 30)
    
    FXLabel.new(group2, "Inside Bar")
    slider = FXSlider.new(group2, :opts => LAYOUT_TOP|LAYOUT_FILL_X|LAYOUT_FIX_HEIGHT|SLIDER_HORIZONTAL|SLIDER_INSIDE_BAR, :width => 200, :height => 20)  
    slider.range = 0..3
    
    frame = FXHorizontalFrame.new(group2, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    FXSlider.new(frame, nil, 0,
      LAYOUT_FIX_HEIGHT|SLIDER_VERTICAL, 0, 0, 30, 200)
    FXSlider.new(frame, nil, 0,
      LAYOUT_FIX_HEIGHT|SLIDER_VERTICAL|SLIDER_ARROW_RIGHT, 0, 0, 30, 200)
    FXSlider.new(frame, nil, 0,
      LAYOUT_FIX_HEIGHT|SLIDER_VERTICAL|SLIDER_ARROW_LEFT, 0, 0, 30, 200)
    FXSlider.new(frame, nil, 0,
      LAYOUT_FIX_HEIGHT|SLIDER_VERTICAL|SLIDER_INSIDE_BAR, 0, 0, 20, 200)
    FXScrollBar.new(frame, nil, 0,
      SCROLLBAR_VERTICAL|LAYOUT_FIX_HEIGHT|LAYOUT_FIX_WIDTH, 0, 0, 20, 300)
  
    vframe1 = FXVerticalFrame.new(frame, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXArrowButton.new(vframe1, nil, 0,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK|ARROW_UP)
    FXArrowButton.new(vframe1, nil, 0,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK|ARROW_DOWN)
    FXArrowButton.new(vframe1, nil, 0,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK|ARROW_LEFT)
    FXArrowButton.new(vframe1, nil, 0,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK|ARROW_RIGHT)
  
    vframe2 = FXVerticalFrame.new(frame, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXArrowButton.new(vframe2, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK|ARROW_UP|ARROW_TOOLBAR)
    FXArrowButton.new(vframe2, nil, 0, (LAYOUT_FILL_X|LAYOUT_FILL_Y|
      FRAME_RAISED|FRAME_THICK|ARROW_DOWN|ARROW_TOOLBAR))
    FXArrowButton.new(vframe2, nil, 0, (LAYOUT_FILL_X|LAYOUT_FILL_Y|
      FRAME_RAISED|FRAME_THICK|ARROW_LEFT|ARROW_TOOLBAR))
    FXArrowButton.new(vframe2, nil, 0, (LAYOUT_FILL_X|LAYOUT_FILL_Y|
      FRAME_RAISED|FRAME_THICK|ARROW_RIGHT|ARROW_TOOLBAR))
  
    gp_datatarget = FXDataTarget.new(0)
    gp = FXGroupBox.new(group3, "Group Box",
      LAYOUT_SIDE_TOP|FRAME_GROOVE|LAYOUT_FILL_X, 0, 0, 0, 0)
    FXRadioButton.new(gp, "Hilversum &1", gp_datatarget, FXDataTarget::ID_OPTION+0,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXRadioButton.new(gp, "Hilversum &2", gp_datatarget, FXDataTarget::ID_OPTION+1,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXRadioButton.new(gp, "One multi-line\nRadiobox Widget", gp_datatarget, FXDataTarget::ID_OPTION+2,
      JUSTIFY_LEFT|JUSTIFY_TOP|ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXRadioButton.new(gp, "Radio Stad Amsterdam", gp_datatarget, FXDataTarget::ID_OPTION+3,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    
    vv = FXGroupBox.new(group3, "Group Box",
      LAYOUT_SIDE_TOP|FRAME_GROOVE|LAYOUT_FILL_X, 0, 0, 0, 0)
    FXCheckButton.new(vv, "Hilversum 1", nil, 0,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXCheckButton.new(vv, "Hilversum 2", nil, 0,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXCheckButton.new(vv, "One multi-line\nCheckbox Widget", nil, 0,
      JUSTIFY_LEFT|JUSTIFY_TOP|ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    FXCheckButton.new(vv, "Radio Stad Amsterdam", nil, 0,
      ICON_BEFORE_TEXT|LAYOUT_SIDE_TOP)
    
    spinner = FXSpinner.new(group3, 20, nil, 0,
      SPIN_NORMAL|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    spinner.range = 1..20
    
    combobox = FXComboBox.new(group3, 5, nil, 0,
      COMBOBOX_INSERT_LAST|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    combobox.appendItem("Very Wide Item")
    for i in 0...3
      combobox.appendItem("%04d" % i)
    end
    
    treebox = FXTreeListBox.new(group3, nil, 0,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP, 0, 0, 200, 0)
  
    topmost = treebox.appendItem(nil, "Top", folder_open, folder_closed)
    topmost2 = treebox.appendItem(nil, "Top2", folder_open, folder_closed)
             treebox.appendItem(topmost2, "First", doc, doc)
  
    treebox.appendItem(topmost, "First", doc, doc)
    treebox.appendItem(topmost, "Second", doc, doc)
    treebox.appendItem(topmost, "Third", doc, doc)
    branch = treebox.appendItem(topmost, "Fourth", folder_open, folder_closed)
      treebox.appendItem(branch, "Fourth-First", doc, doc)
      treebox.appendItem(branch, "Fourth-Second", doc, doc)
      twig = treebox.appendItem(branch, "Fourth-Third", folder_open, folder_closed)
        treebox.appendItem(twig, "Fourth-Third-First", doc, doc)
        treebox.appendItem(twig, "Fourth-Third-Second", doc, doc)
        treebox.appendItem(twig, "Fourth-Third-Third", doc, doc)
        leaf = treebox.appendItem(twig, "Fourth-Third-Fourth", folder_open, folder_closed)
          treebox.appendItem(leaf, "Fourth-Third-Fourth-First", doc, doc)
          treebox.appendItem(leaf, "Fourth-Third-Fourth-Second", doc, doc)
          treebox.appendItem(leaf, "Fourth-Third-Fourth-Third", doc, doc)
      twig = treebox.appendItem(branch, "Fourth-Fourth", folder_open, folder_closed)
        treebox.appendItem(twig, "Fourth-Fourth-First", doc, doc)
        treebox.appendItem(twig, "Fourth-Fourth-Second", doc, doc)
        treebox.appendItem(twig, "Fourth-Fourth-Third", doc, doc)
    
    FXLabel.new(group3, "H&it the hotkey", nil,
      LAYOUT_CENTER_X|JUSTIFY_CENTER_X|FRAME_RAISED)
    textfield1 = FXTextField.new(group3, 20, nil, 0,
      JUSTIFY_RIGHT|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    textfield1.text = "Normal Text Field"
    textfield2 = FXTextField.new(group3, 20, nil, 0,
      JUSTIFY_RIGHT|TEXTFIELD_PASSWD|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    textfield2.text = "Password"
    textfield3 = FXTextField.new(group3, 20, nil, 0,
      TEXTFIELD_READONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    textfield3.text = "Read Only"
    textfield4 = FXTextField.new(group3, 20, nil, 0,
      TEXTFIELD_READONLY|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP)
    textfield4.text = "Grayed out"
    textfield4.disable
    
    realnumber = FXTextField.new(group3, 20, nil, 0,
      TEXTFIELD_REAL|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP|LAYOUT_FIX_HEIGHT,
      0, 0, 0, 30)
    realnumber.text = "1.0E+3"
    intnumber = FXTextField.new(group3, 20, nil, 0,
      TEXTFIELD_INTEGER|FRAME_SUNKEN|FRAME_THICK|LAYOUT_SIDE_TOP|LAYOUT_FIX_HEIGHT,
      0, 0, 0, 30)
    intnumber.text = "1000"
    
    dial2 = FXDial.new(group3, nil, 0, (DIAL_CYCLIC|DIAL_HAS_NOTCH|
      DIAL_HORIZONTAL|LAYOUT_FILL_X|FRAME_RAISED|FRAME_THICK), 0, 0, 120, 0)
    FXScrollBar.new(group3, nil, 0,
      SCROLLBAR_HORIZONTAL|LAYOUT_FIX_HEIGHT|LAYOUT_FIX_WIDTH, 0, 0, 300, 20)
    
    pbar = FXProgressBar.new(group3, nil, 0,
      LAYOUT_FILL_X|FRAME_SUNKEN|FRAME_THICK|PROGRESSBAR_PERCENTAGE)
    pbar.progress = 48
    pbar.total = 360
    pbar2 = FXProgressBar.new(group3, nil, 0, (LAYOUT_FILL_Y|FRAME_SUNKEN|
      FRAME_THICK|PROGRESSBAR_VERTICAL|PROGRESSBAR_PERCENTAGE|LAYOUT_SIDE_LEFT))
    pbar2.total = 360
    dial1 = FXDial.new(group3, nil, 0, (DIAL_CYCLIC|DIAL_HAS_NOTCH|
      DIAL_VERTICAL|FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_Y|LAYOUT_SIDE_LEFT))
    pbar2.progress = 48
    dial1.target = pbar2
    dial1.selector = FXWindow::ID_SETVALUE
    dial2.target = pbar
    dial2.selector = FXWindow::ID_SETVALUE
  
    # Currently selected choice from the radio buttons
    @choice = 0

    # File filter for file dialogs
    @sourcefiles = "All Files (*)\n" +
                   "C++ Source Files (*.cpp,*.cxx,*.cc)\n" +
                   "C Source Files (*.c)\n" +
                   "C++ Header Files (*.hpp,*.hxx,*.hh,*.h)\n" +
                   "*.o\n" +
                   "Any Extension (*.*)\n" +
                   "Three Letter (*.???)\n" +
                   "README*"
  end

  # Set choice
  def onCmdRadio(sender, sel, ptr)
    @choice = FXSELID(sel)
    return 1
  end

  # Update menu based on choice
  def onUpdRadio(sender, sel, ptr)
    sender.check = (FXSELID(sel) == @choice)
    return 1
  end

  # Create the main window and show it
  def create
    super
    show(PLACEMENT_SCREEN)

    # Create a thread to update the clock
    @clockThread = Thread.new(@clockLabel) { |clockLabel|
      while true
        clockLabel.text = Time.now.strftime("%I:%M:%S %p")
        sleep(1)
      end
    }
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("Groupbox", "FoxTest")
  
  # Make window
  GroupWindow.new(application)
  
  # Create app
  application.create
  
  # Run
  application.run
end

