#!/usr/bin/env ruby

require 'fox16'

include Fox

GORTS_BLURB = "Icons courtesy of Gort's Icons:\nhttp://www.forrestwalter.com/icons"

class ShutterItem < FXShutterItem
  def initialize(p, text, icon=nil, opts=0)
    super(p, text, icon, opts|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT, :padding => 10, :hSpacing => 10, :vSpacing => 10)
    button.padTop = 2
    button.padBottom = 2
  end
end

class ShutterButton < FXButton
  def initialize(p, txt, ic=nil)
    super(p, txt, ic, :opts => BUTTON_TOOLBAR|TEXT_BELOW_ICON|FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    self.backColor = p.backColor
    self.textColor = FXRGB(255, 255, 255)
  end
end

class ShutterWindow < FXMainWindow

  # This is just a helper function that loads an ICO file from disk
  # and constructs and returns a ICO icon object.

  def loadIcon(filename)
    begin
      filename = File.join("icons", filename)
      icon = nil
      File.open(filename, "rb") do |f|
        icon = FXICOIcon.new(getApp(), f.read)
      end
      icon
    rescue
      raise RuntimeError, "Couldn't load icon: #{filename}"
    end
  end

  def initialize(app)
    # Invoke base class initialize first
    super(app, "Look Out!", :opts => DECOR_ALL, :width => 600, :height => 600)

    # Menubar along the top
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)

    # Edit menu
    editmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Edit", nil, editmenu)

    # View menu
    viewmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&View", nil, viewmenu)

    # Favorites menu
    favmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "Fav&orites", nil, favmenu)

    # Tools menu
    toolsmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Tools", nil, toolsmenu)

    # Actions menu
    actionsmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Actions", nil, actionsmenu)

    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu)

    # Status bar along the bottom
    status = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)

    # Main contents area is split left-to-right
    splitter = FXSplitter.new(self, (LAYOUT_SIDE_TOP|LAYOUT_FILL_X|
      LAYOUT_FILL_Y|SPLITTER_TRACKING))

    # Shutter area on the left
    @shutter = FXShutter.new(splitter,
      :opts => FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padding => 0, :hSpacing => 0, :vSpacing => 0)
    
    fatBot = loadIcon("FatBot.ico")
    angryGuy = loadIcon("AngryGuyInBunnySuit.ico")
    sawBlade = loadIcon("SawBlade.ico")
    redMacOS = loadIcon("RedMacOS.ico")
    leGoon = loadIcon("LeGoon.ico")
    flippedySwitch = loadIcon("FlippedySwitch.ico")
    net = loadIcon("Net.ico")
    
    shutterItem = ShutterItem.new(@shutter, "Lookout Shortcuts", nil, LAYOUT_FILL_Y)
    ShutterButton.new(shutterItem.content, "Lookout Today", fatBot).connect(SEL_COMMAND) { @switcher.current = 0 }
    ShutterButton.new(shutterItem.content, "Inbox", angryGuy).connect(SEL_COMMAND) { @switcher.current = 1 }
    ShutterButton.new(shutterItem.content, "Calendar", sawBlade).connect(SEL_COMMAND) { @switcher.current = 2 }
    ShutterButton.new(shutterItem.content, "Contacts", redMacOS).connect(SEL_COMMAND) { @switcher.current = 3 }
    ShutterButton.new(shutterItem.content, "Tasks", leGoon).connect(SEL_COMMAND) { @switcher.current = 4 }
    ShutterButton.new(shutterItem.content, "Notes", flippedySwitch).connect(SEL_COMMAND) { @switcher.current = 5 }
    ShutterButton.new(shutterItem.content, "Deleted Items", net).connect(SEL_COMMAND) { @switcher.current = 6 }
  
    shutterItem = ShutterItem.new(@shutter, "My Shortcuts")
    ShutterButton.new(shutterItem.content, "Drafts", fatBot).connect(SEL_COMMAND) { @switcher.current = 7 }
    ShutterButton.new(shutterItem.content, "Outbox", angryGuy).connect(SEL_COMMAND) { @switcher.current = 8 }
    ShutterButton.new(shutterItem.content, "Sent Items", sawBlade).connect(SEL_COMMAND) { @switcher.current = 9 }
    ShutterButton.new(shutterItem.content, "Journal", redMacOS).connect(SEL_COMMAND) { @switcher.current = 10 }
    ShutterButton.new(shutterItem.content, "Lookout Update", flippedySwitch).connect(SEL_COMMAND) { @switcher.current = 11 }
  
    shutterItem = ShutterItem.new(@shutter, "Other Shortcuts")
    ShutterButton.new(shutterItem.content, "My Computer", angryGuy).connect(SEL_COMMAND) { @switcher.current = 12 }
    ShutterButton.new(shutterItem.content, "My Documents", net).connect(SEL_COMMAND) { @switcher.current = 13 }
    ShutterButton.new(shutterItem.content, "Favorites", leGoon).connect(SEL_COMMAND) { @switcher.current = 14 }
      
    # Right pane is a switcher
    # For a real application, each panel in the switcher would have real, working contents...
    @switcher = FXSwitcher.new(splitter,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y, :padding => 0)

    FXLabel.new(@switcher,
      "Lookout Today!\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Inbox\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Calendar\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Contacts\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Tasks\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Notes\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Deleted Items\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    FXLabel.new(@switcher, "Drafts\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Outbox\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Sent Items\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Journal\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Lookout Update\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    FXLabel.new(@switcher, "My Computer\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "My Documents\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    FXLabel.new(@switcher, "Favorites\n\n#{GORTS_BLURB}", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  end

  def create
    super
    @shutter.width = 1.25*@shutter.width
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  application = FXApp.new("Shutter", "FoxTest")
  ShutterWindow.new(application)
  application.create
  application.run
end
