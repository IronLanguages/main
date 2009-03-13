#!/usr/bin/env ruby

require 'fox16'
require 'fox16/colors'

include Fox

TYGER = <<END_OF_POEM
The Tyger

Tyger! Tyger! burning bright
In the forests of the night
What immortal hand or eye
Could frame thy fearful symmetry?

In what distant deeps or skies
Burnt the fire of thine eyes?
On what wings dare he aspire?
What the hand dare seize the fire?

And what shoulder, and what art,
Could twist the sinews of thy heart,
And when thy heart began to beat,
What dread hand? and what dread feet?

What the hammer? what the chain?
In what furnace was thy brain?
What the anvil? what dread grasp
Dare its deadly terrors clasp?

When the stars threw down their spears,
And water'd heaven with their tears,
Did he smile his work to see?
Did he who made the Lamb make thee?

Tyger! Tyger! burning bright
In the forests of the night,
What immortal hand or eye,
Dare frame thy fearful symmetry?



               - William Blake
END_OF_POEM

class MDITestWindow  < FXMainWindow

  def initialize(app)
    # Invoke base class initialize method first
    super(app, "MDI Widget Test", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Create the font
    @font = FXFont.new(getApp(), "courier", 15, FONTWEIGHT_BOLD)
  
    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
  
    # Status bar
    FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
  
    # MDI Client
    @mdiclient = FXMDIClient.new(self, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  
    # Icon for MDI Child
    @mdiicon = nil
    File.open(File.join("icons", "penguin.png"), "rb") do |f|
      @mdiicon = FXPNGIcon.new(getApp(), f.read)
    end

    # Make MDI Menu
    @mdimenu = FXMDIMenu.new(self, @mdiclient)
  
    # MDI buttons in menu:- note the message ID's!!!!!
    # Normally, MDI commands are simply sensitized or desensitized;
    # Under the menubar, however, they're hidden if the MDI Client is
    # not maximized.  To do this, they must have different ID's.
    FXMDIWindowButton.new(menubar, @mdimenu, @mdiclient, FXMDIClient::ID_MDI_MENUWINDOW,
      LAYOUT_LEFT)
    FXMDIDeleteButton.new(menubar, @mdiclient, FXMDIClient::ID_MDI_MENUCLOSE,
      FRAME_RAISED|LAYOUT_RIGHT)
    FXMDIRestoreButton.new(menubar, @mdiclient, FXMDIClient::ID_MDI_MENURESTORE,
      FRAME_RAISED|LAYOUT_RIGHT)
    FXMDIMinimizeButton.new(menubar, @mdiclient,
      FXMDIClient::ID_MDI_MENUMINIMIZE, FRAME_RAISED|LAYOUT_RIGHT)
  
    # Create a few test windows to get started
    mdichild = createTestWindow(10, 10, 400, 300)
    @mdiclient.setActiveChild(mdichild)
    createTestWindow(20, 20, 400, 300)
    createTestWindow(30, 30, 400, 300)
  
    # File menu
    filemenu = FXMenuPane.new(self)
    newCmd = FXMenuCommand.new(filemenu, "&New\tCtl-N\tCreate new document.")
    newCmd.connect(SEL_COMMAND, method(:onCmdNew))
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q\tQuit application.", nil,
      getApp(), FXApp::ID_QUIT, 0)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
      
    # Window menu
    windowmenu = FXMenuPane.new(self)
    FXMenuCommand.new(windowmenu, "Tile &Horizontally", nil,
      @mdiclient, FXMDIClient::ID_MDI_TILEHORIZONTAL)
    FXMenuCommand.new(windowmenu, "Tile &Vertically", nil,
      @mdiclient, FXMDIClient::ID_MDI_TILEVERTICAL)
    FXMenuCommand.new(windowmenu, "C&ascade", nil,
      @mdiclient, FXMDIClient::ID_MDI_CASCADE)
    FXMenuCommand.new(windowmenu, "&Close", nil,
      @mdiclient, FXMDIClient::ID_MDI_CLOSE)
    sep1 = FXMenuSeparator.new(windowmenu)
    sep1.setTarget(@mdiclient)
    sep1.setSelector(FXMDIClient::ID_MDI_ANY)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_1)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_2)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_3)
    FXMenuCommand.new(windowmenu, nil, nil, @mdiclient, FXMDIClient::ID_MDI_4)
    FXMenuCommand.new(windowmenu, "&Others...", nil, @mdiclient, FXMDIClient::ID_MDI_OVER_5)
    FXMenuTitle.new(menubar,"&Window", nil, windowmenu)
    
    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuCommand.new(helpmenu, "&About FOX...").connect(SEL_COMMAND) {
      FXMessageBox.information(self, MBOX_OK, "About MDI Test",
        "Test of the FOX MDI Widgets\nWritten by Jeroen van der Zijp")
    }
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)
  end

  # Create a new MDI child window
  def createTestWindow(x, y, w, h)
    mdichild = FXMDIChild.new(@mdiclient, "Child", @mdiicon, @mdimenu,
      0, x, y, w, h)
    scrollwindow = FXScrollWindow.new(mdichild, 0)
    scrollwindow.verticalScrollBar.setLine(@font.fontHeight)
    btn = FXButton.new(scrollwindow, TYGER,
      :opts => LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT, :width => 600, :height => 1000)
    btn.font = @font
    btn.backColor = FXColor::White
    mdichild
  end

  # New
  def onCmdNew(sender, sel, ptr)
    mdichild = createTestWindow(20, 20, 300, 200)
    mdichild.create
    return 1
  end

  # Start
  def create
    super

    # At the time the first three MDI windows are constructed, we don't
    # yet know the font height and so we cannot accurately set the line
    # height for the vertical scrollbar. Now that the real font has been
    # created, we can go back and fix the scrollbar line heights for these
    # windows.
    @font.create
    @mdiclient.each_child do |mdichild|
      mdichild.contentWindow.verticalScrollBar.setLine(@font.fontHeight)
    end

    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("MDIApp", "FoxTest")
  
  # Make window
  MDITestWindow.new(application)
  
  # Create app
  application.create
  
  # Run
  application.run
end
