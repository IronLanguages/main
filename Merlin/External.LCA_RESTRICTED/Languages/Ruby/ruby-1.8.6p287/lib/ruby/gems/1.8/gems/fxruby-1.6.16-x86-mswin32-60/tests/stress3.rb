#!/usr/bin/env ruby

require 'fox16'
require 'test/unit'

include Fox

class ShutterItem < FXShutterItem
  def initialize(p, text, icon=nil, opts=0)
    super(p, text, icon, opts|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT, 0, 0, 0, 0, 10, 10, 10, 10, 10, 10)
    button.padTop = 2
    button.padBottom = 2
  end
end

class ShutterButton < FXButton
  def initialize(p, txt, ic=nil)
    super(p, txt, ic, nil, 0, BUTTON_TOOLBAR|TEXT_BELOW_ICON|FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT)
    self.backColor = p.backColor
    self.textColor = FXRGB(255, 255, 255)
  end
end

class ShutterWindow < FXMainWindow

  attr_accessor :shutter

  def initialize(app)
    # Invoke base class initialize first
    super(app, "Look Out!", nil, nil, DECOR_ALL, 0, 0, 600, 600)

    # Main contents area is split left-to-right
    splitter = FXSplitter.new(self, (LAYOUT_SIDE_TOP|LAYOUT_FILL_X|
      LAYOUT_FILL_Y|SPLITTER_TRACKING))

    # Shutter area on the left
    @shutter = FXShutter.new(splitter, nil, 0,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    
    shutterItem = ShutterItem.new(@shutter, "Shutter Item 1", nil, LAYOUT_FILL_Y)
    ShutterButton.new(shutterItem.content, "1-1")
    ShutterButton.new(shutterItem.content, "1-2")
    ShutterButton.new(shutterItem.content, "1-3")
    ShutterButton.new(shutterItem.content, "1-4")
    ShutterButton.new(shutterItem.content, "1-5")
    ShutterButton.new(shutterItem.content, "1-6")
    ShutterButton.new(shutterItem.content, "1-7")
  
    shutterItem = ShutterItem.new(@shutter, "Shutter Item 2")
    ShutterButton.new(shutterItem.content, "2-1")
    ShutterButton.new(shutterItem.content, "2-2")
    ShutterButton.new(shutterItem.content, "2-3")
    ShutterButton.new(shutterItem.content, "2-4")
    ShutterButton.new(shutterItem.content, "2-5")
  
    shutterItem = ShutterItem.new(@shutter, "Shutter Item 3")
    ShutterButton.new(shutterItem.content, "3-1")
    ShutterButton.new(shutterItem.content, "3-2")
    ShutterButton.new(shutterItem.content, "3-3")
      
    # Right pane is a switcher
    # For a real application, each panel in the switcher would have real, working contents...
    @switcher = FXSwitcher.new(splitter,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y, 0, 0, 0, 0, 0, 0, 0, 0)

    FXLabel.new(@switcher,
      "Lookout Today!", nil, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  end

  def create
    # Create base class
    super
    
    # Run the garbage collector now
    GC.start
    
    # Safe to drop out any time now...
    getApp().addChore(getApp(), FXApp::ID_QUIT)
  end
end

class TC_stress3 < Test::Unit::TestCase
  def test_main
    # Run the program
    theApp = FXApp.new("Shutter", "FoxTest")
    shutterWindow = ShutterWindow.new(theApp)
    theApp.create
    theApp.run
    
    #
    # Check to see if anyone's missing in action.
    # First, the shutter itself should have three
    # shutter items as its children.
    #
    assert_equal(3, shutterWindow.shutter.numChildren)
    
    # Each shutter item has two children
    shutterWindow.shutter.each_child { |c|
      assert_equal(2, c.numChildren)
    }
    
    # First item's content should have 7 children
    shutterItem1 = shutterWindow.shutter.first
    assert_equal(7, shutterItem1.content.numChildren)
    
    # Second item's content should have 5 children
    shutterItem2 = shutterItem1.next 
    assert_equal(5, shutterItem2.content.numChildren)

    # Third item's content should have 3 children
    shutterItem3 = shutterItem2.next 
    assert_equal(3, shutterItem3.content.numChildren)
  end
end


