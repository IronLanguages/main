#!/usr/bin/env ruby

require 'fox16'

include Fox

class FourSplitWindow < FXMainWindow
  def initialize(app)
    # Call the base class initialize() first
    super(app, "4-Way Splitter Test", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Menu bar, along the top
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
  
    # Status bar, along the bottom
    FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
    
    # The top-level splitter takes up the rest of the space
    splitter = FX4Splitter.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y|FOURSPLITTER_TRACKING)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q\tQuit the application.", nil,
      getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Expand menu
    expandmenu = FXMenuPane.new(self)
    FXMenuCommand.new(expandmenu, "All four", nil,
      splitter, FX4Splitter::ID_EXPAND_ALL)
    FXMenuCommand.new(expandmenu, "Top/left", nil,
      splitter, FX4Splitter::ID_EXPAND_TOPLEFT)
    FXMenuCommand.new(expandmenu, "Top/right", nil,
      splitter, FX4Splitter::ID_EXPAND_TOPRIGHT)
    FXMenuCommand.new(expandmenu, "Bottom/left", nil,
      splitter, FX4Splitter::ID_EXPAND_BOTTOMLEFT)
    FXMenuCommand.new(expandmenu, "Bottom/right", nil,
      splitter, FX4Splitter::ID_EXPAND_BOTTOMRIGHT)
    FXMenuTitle.new(menubar, "&Expand", nil, expandmenu)
    
    # The 4-splitter accepts exactly four child widgets, and the
    # order in which they are added matters (top left, top right,
    # bottom left and bottom right, in that order). For our case,
    # the first three child widgets are just regular pushbuttons,
    # but the fourth is itself another 4-splitter. There is no
    # restriction on nesting these kinds of widgets.

    FXButton.new(splitter, "Top &Left\tThis splitter tracks", :opts => FRAME_RAISED|FRAME_THICK)

    FXButton.new(splitter, "Top &Right\tThis splitter tracks", :opts => FRAME_RAISED|FRAME_THICK)

    FXButton.new(splitter, "&Bottom Left\tThis splitter tracks", :opts => FRAME_SUNKEN|FRAME_THICK)

    subsplitter = FX4Splitter.new(splitter, LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # Create the four children of the sub-splitter...
    FXButton.new(subsplitter, "&Of course\tThis splitter does NOT track") do |theButton|
      theButton.frameStyle = FRAME_SUNKEN|FRAME_THICK
      theButton.backColor = FXRGB(0, 128, 0)
      theButton.textColor = FXRGB(255, 255, 255)
    end

    button = FXButton.new(subsplitter,
      "the&y CAN\tThis splitter does NOT track", :opts => FRAME_SUNKEN|FRAME_THICK)
    button.backColor = FXRGB(128, 0, 0)
    button.textColor = FXRGB(255, 255, 255)

    button = FXButton.new(subsplitter,
      "be &NESTED\tThis splitter does NOT track", :opts => FRAME_SUNKEN|FRAME_THICK)
    button.backColor = FXRGB(0, 0, 200)
    button.textColor = FXRGB(255, 255, 255)

    button = FXButton.new(subsplitter,
      "&arbitrarily!\tThis splitter does NOT track", :opts => FRAME_SUNKEN|FRAME_THICK)
    button.backColor = FXRGB(128, 128, 0)
    button.textColor = FXRGB(255, 255, 255)
    
    # Finally, create the tool tip object
    FXToolTip.new(getApp())
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

def runme
  application = FXApp.new("FourSplit", "FoxTest")
  FourSplitWindow.new(application)
  application.create
  application.run
end

runme
