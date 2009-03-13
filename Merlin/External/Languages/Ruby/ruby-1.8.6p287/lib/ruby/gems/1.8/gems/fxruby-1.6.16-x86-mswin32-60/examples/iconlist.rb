require 'fox16'

include Fox

class IconListWindow < FXMainWindow
  
  # Load the named PNG icon from a file
  def loadIcon(filename)
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

  # Main window constructor
  def initialize(app)
    # Initialize base class first
    super(app, "Icon List Test", :opts => DECOR_ALL, :width => 800, :height => 600)
    
    # Menu bar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
      
    # Status bar
    status = FXStatusBar.new(self, LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
    
    # Main window interior
    group = FXVerticalFrame.new(self, LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)

    # Files
    FXLabel.new(group, "Icon List Widget", nil, LAYOUT_TOP|LAYOUT_FILL_X|FRAME_SUNKEN)
    subgroup = FXVerticalFrame.new(group, FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
  
    # Icon list on the right
    iconlist = FXIconList.new(subgroup, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|ICONLIST_BIG_ICONS|ICONLIST_EXTENDEDSELECT)
    
    iconlist.appendHeader("Name", nil, 200)
    iconlist.appendHeader("Type", nil, 100)
    iconlist.appendHeader("Size", nil, 60)
    iconlist.appendHeader("Modified Date", nil, 150)
    iconlist.appendHeader("User", nil, 50)
    iconlist.appendHeader("Group", nil, 50)
  
    big_folder = loadIcon("bigfolder.png")
    mini_folder = loadIcon("minifolder.png")
  
    iconlist.appendItem("Really BIG and wide item to test\tDocument\t10000\tJune 13, 1999\tUser\tSoftware", big_folder, mini_folder)
    1.upto(400) do |i|
      iconlist.appendItem("Filename_#{i}\tDocument\t10000\tJune 13, 1999\tUser\tSoftware", big_folder, mini_folder)
    end
    iconlist.currentItem = iconlist.numItems - 1

    # Arrange menu
    FXMenuPane.new(self) do |menuPane|
      FXMenuCommand.new(menuPane, "&Details", nil, iconlist, FXIconList::ID_SHOW_DETAILS)
      FXMenuCommand.new(menuPane, "&Small Icons", nil, iconlist, FXIconList::ID_SHOW_MINI_ICONS)
      FXMenuCommand.new(menuPane, "&Big Icons", nil, iconlist, FXIconList::ID_SHOW_BIG_ICONS)
      FXMenuCommand.new(menuPane, "&Rows", nil, iconlist, FXIconList::ID_ARRANGE_BY_ROWS)
      FXMenuCommand.new(menuPane, "&Columns", nil, iconlist, FXIconList::ID_ARRANGE_BY_COLUMNS)
      FXMenuTitle.new(menubar, "&Arrange", nil, menuPane)
    end
    # Let's see a tooltip
    FXToolTip.new(getApp())
  end
  
  # Overrides base class version
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  FXApp.new("IconList", "FXRuby") do |theApp|
    IconListWindow.new(theApp)
    theApp.create
    theApp.run
  end
end

