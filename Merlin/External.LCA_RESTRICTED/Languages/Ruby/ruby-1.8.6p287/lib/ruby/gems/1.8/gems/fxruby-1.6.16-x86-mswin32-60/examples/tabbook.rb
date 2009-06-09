#!/usr/bin/env ruby

require 'fox16'

include Fox

class TabBookWindow < FXMainWindow

  def initialize(app)
    # Call the base class initializer first
    super(app, "Tab Book Test", :opts => DECOR_ALL, :width => 600, :height => 400)

    # Make a tooltip
    FXToolTip.new(getApp())

    # Menubar appears along the top of the main window
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)

    # Separator
    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|SEPARATOR_GROOVE)

    # Contents
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|FRAME_NONE|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH)
  
    # Switcher
    @tabbook = FXTabBook.new(contents,:opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_RIGHT)
  
    # First item is a list
    @tab1 = FXTabItem.new(@tabbook, "&Simple List", nil)
    listframe = FXHorizontalFrame.new(@tabbook, FRAME_THICK|FRAME_RAISED)
    simplelist = FXList.new(listframe, :opts => LIST_EXTENDEDSELECT|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    simplelist.appendItem("First Entry")
    simplelist.appendItem("Second Entry")
    simplelist.appendItem("Third Entry")
    simplelist.appendItem("Fourth Entry")
      
    # Second item is a file list
    @tab2 = FXTabItem.new(@tabbook, "F&ile List", nil)
    @fileframe = FXHorizontalFrame.new(@tabbook, FRAME_THICK|FRAME_RAISED)
    filelist = FXFileList.new(@fileframe, :opts => ICONLIST_EXTENDEDSELECT|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # Third item is a directory list
    @tab3 = FXTabItem.new(@tabbook, "T&ree List", nil)
    dirframe = FXHorizontalFrame.new(@tabbook, FRAME_THICK|FRAME_RAISED)
    dirlist = FXDirList.new(dirframe,
      :opts => DIRLIST_SHOWFILES|TREELIST_SHOWS_LINES|TREELIST_SHOWS_BOXES|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # File Menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Simple List", nil,
      @tabbook, FXTabBar::ID_OPEN_FIRST+0)
    FXMenuCommand.new(filemenu, "F&ile List", nil,
      @tabbook, FXTabBar::ID_OPEN_FIRST+1)
    FXMenuCommand.new(filemenu, "T&ree List", nil,
      @tabbook, FXTabBar::ID_OPEN_FIRST+2)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil,
      getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Tab side
    tabmenu = FXMenuPane.new(self)
    hideShow = FXMenuCheck.new(tabmenu, "Hide/Show Tab 2")
    hideShow.connect(SEL_COMMAND) {
      if @tab2.shown?
        @tab2.hide
        @fileframe.hide
      else
        @tab2.show
        @fileframe.show
      end
      @tab2.recalc
      @fileframe.recalc
    }
    hideShow.connect(SEL_UPDATE) { hideShow.check = @tab2.shown? }
    
    FXMenuSeparator.new(tabmenu)
    
    topTabsCmd = FXMenuRadio.new(tabmenu, "&Top Tabs")
    topTabsCmd.connect(SEL_COMMAND) do
      @tabbook.tabStyle = TABBOOK_TOPTABS
      @tab1.tabOrientation = TAB_TOP
      @tab2.tabOrientation = TAB_TOP
      @tab3.tabOrientation = TAB_TOP
    end
    topTabsCmd.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.check = (@tabbook.tabStyle == TABBOOK_TOPTABS)
    end
    
    bottomTabsCmd = FXMenuRadio.new(tabmenu, "&Bottom Tabs")
    bottomTabsCmd.connect(SEL_COMMAND) do
      @tabbook.tabStyle = TABBOOK_BOTTOMTABS
      @tab1.tabOrientation = TAB_BOTTOM
      @tab2.tabOrientation = TAB_BOTTOM
      @tab3.tabOrientation = TAB_BOTTOM
    end
    bottomTabsCmd.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.check = (@tabbook.tabStyle == TABBOOK_BOTTOMTABS)
    end
    
    leftTabsCmd = FXMenuRadio.new(tabmenu, "&Left Tabs")
    leftTabsCmd.connect(SEL_COMMAND) do
      @tabbook.tabStyle = TABBOOK_LEFTTABS
      @tab1.tabOrientation = TAB_LEFT
      @tab2.tabOrientation = TAB_LEFT
      @tab3.tabOrientation = TAB_LEFT
    end
    leftTabsCmd.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.check = (@tabbook.tabStyle == TABBOOK_LEFTTABS)
    end
    
    rightTabsCmd = FXMenuRadio.new(tabmenu, "&Right Tabs")
    rightTabsCmd.connect(SEL_COMMAND) do
      @tabbook.tabStyle = TABBOOK_RIGHTTABS
      @tab1.tabOrientation = TAB_RIGHT
      @tab2.tabOrientation = TAB_RIGHT
      @tab3.tabOrientation = TAB_RIGHT
    end
    rightTabsCmd.connect(SEL_UPDATE) do |sender, sel, ptr|
      sender.check = (@tabbook.tabStyle == TABBOOK_RIGHTTABS)
    end
    
    FXMenuSeparator.new(tabmenu)
    
    addTabCmd = FXMenuCommand.new(tabmenu, "Add Tab")
    addTabCmd.connect(SEL_COMMAND) do
      FXTabItem.new(@tabbook, "New Tab")
      FXHorizontalFrame.new(@tabbook, FRAME_THICK|FRAME_RAISED) do |hf|
        FXLabel.new(hf, "Always add tab item and contents together.", :opts => LAYOUT_FILL)
      end
      @tabbook.create # realize widgets
      @tabbook.recalc # mark parent layout dirty
    end
    
    removeTabCmd = FXMenuCommand.new(tabmenu, "Remove Last Tab")
    removeTabCmd.connect(SEL_COMMAND) do
      numTabs = @tabbook.numChildren/2
      doomedTab = numTabs - 1
      @tabbook.removeChild(@tabbook.childAtIndex(2*doomedTab+1))
      @tabbook.removeChild(@tabbook.childAtIndex(2*doomedTab))
    end

    FXMenuTitle.new(menubar, "&Tab Placement", nil, tabmenu)
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Make an application
  application = FXApp.new("TabBook", "FoxTest")

  # Build the main window
  TabBookWindow.new(application)

  # Create the application and its windows
  application.create

  # Run
  application.run
end
