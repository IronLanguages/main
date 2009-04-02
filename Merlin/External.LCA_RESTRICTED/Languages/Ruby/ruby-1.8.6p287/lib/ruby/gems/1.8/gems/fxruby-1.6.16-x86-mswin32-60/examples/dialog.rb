#!/usr/bin/env ruby

require 'fox16'

include Fox

# A little dialog box to use in our tests
class FXTestDialog < FXDialogBox

  def initialize(owner)
    # Invoke base class initialize function first
    super(owner, "Test of Dialog Box", DECOR_TITLE|DECOR_BORDER)

    # Bottom buttons
    buttons = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_BOTTOM|FRAME_NONE|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH,
      :padLeft => 40, :padRight => 40, :padTop => 20, :padBottom => 20)

    # Separator
    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|SEPARATOR_GROOVE)
  
    # Contents
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH)
  
    submenu = FXMenuPane.new(self)
    FXMenuCommand.new(submenu, "One")
    FXMenuCommand.new(submenu, "Two")
    FXMenuCommand.new(submenu, "Three")
    
    # Menu
    menu = FXMenuPane.new(self)
    FXMenuCommand.new(menu, "&Accept", nil, self, ID_ACCEPT)
    FXMenuCommand.new(menu, "&Cancel", nil, self, ID_CANCEL)
    FXMenuCascade.new(menu, "Submenu", nil, submenu)
    FXMenuCommand.new(menu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
  
    # Popup menu
    pane = FXPopup.new(self)
    %w{One Two Three Four Five Six Seven Eight Nine Ten}.each do |s|
      FXOption.new(pane, s, :opts => JUSTIFY_HZ_APART|ICON_AFTER_TEXT)
    end
  
    # Option menu
    FXOptionMenu.new(contents, pane, (FRAME_RAISED|FRAME_THICK|
      JUSTIFY_HZ_APART|ICON_AFTER_TEXT|LAYOUT_CENTER_X|LAYOUT_CENTER_Y))

    # Button to pop menu
    FXMenuButton.new(contents, "&Menu", nil, menu, (MENUBUTTON_DOWN|
      JUSTIFY_LEFT|LAYOUT_TOP|FRAME_RAISED|FRAME_THICK|ICON_AFTER_TEXT|
      LAYOUT_CENTER_X|LAYOUT_CENTER_Y))

    # Accept
    accept = FXButton.new(buttons, "&Accept", nil, self, ID_ACCEPT,
      FRAME_RAISED|FRAME_THICK|LAYOUT_RIGHT|LAYOUT_CENTER_Y)
  
    # Cancel
    FXButton.new(buttons, "&Cancel", nil, self, ID_CANCEL,
      FRAME_RAISED|FRAME_THICK|LAYOUT_RIGHT|LAYOUT_CENTER_Y)
    
    accept.setDefault  
    accept.setFocus
  end
  
end

# Subclassed main window
class DialogTester < FXMainWindow

  def initialize(app)
    # Invoke base class initialize first
    super(app, "Dialog Test", :opts => DECOR_ALL, :width => 400, :height => 200)

    # Tooltip
    FXToolTip.new(getApp())
  
    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
  
    # Separator
    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|SEPARATOR_GROOVE)

    # File Menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Quit", nil, getApp(), FXApp::ID_QUIT, 0)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
  
    # Contents
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH)

    # Button to pop normal dialog
    nonModalButton = FXButton.new(contents,
      "&Non-Modal Dialog...\tDisplay normal dialog",
      :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_CENTER_X|LAYOUT_CENTER_Y)
    nonModalButton.connect(SEL_COMMAND, method(:onCmdShowDialog))
  
    # Button to pop modal dialog
    modalButton = FXButton.new(contents,
      "&Modal Dialog...\tDisplay modal dialog",
      :opts => FRAME_RAISED|FRAME_THICK|LAYOUT_CENTER_X|LAYOUT_CENTER_Y)
    modalButton.connect(SEL_COMMAND, method(:onCmdShowDialogModal))
  
    # Build a dialog box
    @dialog = FXTestDialog.new(self)

  end

  # Show the non-modal dialog
  def onCmdShowDialog(sender, sel, ptr)
    @dialog.show
  end

  # Show a modal dialog
  def onCmdShowDialogModal(sender, sel, ptr)
    FXTestDialog.new(self).execute
    return 1
  end

  # Start
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

def run
  # Make an application
  application = FXApp.new("Dialog", "FoxTest")

  # Construct the application's main window
  DialogTester.new(application)

  # Create the application
  application.create

  # Run the application
  application.run
end

run
