#!/usr/bin/env ruby

require 'fox16'

include Fox

class SplitterWindow < FXMainWindow

  # Convenience function to load & construct an icon
  def makeIcon(filename)
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


  def initialize(app)
    # Do base class initialize first
    super(app, "Splitter Test", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Construct some icons we'll use
    folder_open   = makeIcon("minifolderopen.png")
    folder_closed = makeIcon("minifolder.png")
    doc           = makeIcon("minidoc.png")

    # Menu bar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    
    # Status bar
    status = FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Main window interior
    @splitter = FXSplitter.new(self, (LAYOUT_SIDE_TOP|LAYOUT_FILL_X|
      LAYOUT_FILL_Y|SPLITTER_REVERSED|SPLITTER_TRACKING))
    group1 = FXVerticalFrame.new(@splitter,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y, :padding => 0)
    group2 = FXVerticalFrame.new(@splitter,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    group3 = FXVerticalFrame.new(@splitter,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
  
    # Mode menu
    modemenu = FXMenuPane.new(self)
    FXMenuCommand.new(modemenu, "Reverse\t\tReverse split order").connect(SEL_COMMAND) {
      @splitter.splitterStyle |= SPLITTER_REVERSED
    }
    FXMenuCommand.new(modemenu, "Normal\t\tNormal split order").connect(SEL_COMMAND) {
      @splitter.splitterStyle &= ~SPLITTER_REVERSED
    }
    FXMenuCommand.new(modemenu, "Horizontal\t\tHorizontal split").connect(SEL_COMMAND) {
      @splitter.splitterStyle &= ~SPLITTER_VERTICAL
    }
    FXMenuCommand.new(modemenu, "Vertical\t\tVertical split").connect(SEL_COMMAND) {
      @splitter.splitterStyle |= SPLITTER_VERTICAL
    }
    trackingBtn = FXMenuCheck.new(modemenu, "Tracking\t\tToggle continuous tracking mode")
    trackingBtn.connect(SEL_COMMAND, method(:onCmdTracking))
    trackingBtn.connect(SEL_UPDATE, method(:onUpdTracking))
    FXMenuCheck.new(modemenu, "Toggle pane 1", group1, FXWindow::ID_TOGGLESHOWN)
    FXMenuCheck.new(modemenu, "Toggle pane 2", group2, FXWindow::ID_TOGGLESHOWN)
    FXMenuCheck.new(modemenu, "Toggle pane 3", group3, FXWindow::ID_TOGGLESHOWN)
   
    FXMenuTitle.new(menubar, "&Mode", nil, modemenu)
      
    tree = FXTreeList.new(group1,
      :opts => (LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_RIGHT|TREELIST_SHOWS_LINES|
      TREELIST_SHOWS_BOXES|TREELIST_ROOT_BOXES|TREELIST_EXTENDEDSELECT))
  
    topmost = tree.appendItem(nil, "Top", folder_open, folder_closed)
    tree.expandTree(topmost)
      tree.appendItem(topmost, "First", doc, doc)
      tree.appendItem(topmost, "Second", doc, doc)
      tree.appendItem(topmost, "Third", doc, doc)
      branch = tree.appendItem(topmost, "Fourth", folder_open, folder_closed)
      tree.expandTree(branch)
        tree.appendItem(branch, "Fourth-First", doc, doc)
        tree.appendItem(branch, "Fourth-Second", doc, doc)
        twig = tree.appendItem(branch, "Fourth-Third",
                                folder_open, folder_closed)
          tree.appendItem(twig, "Fourth-Third-First", doc, doc)
          tree.appendItem(twig, "Fourth-Third-Second", doc, doc)
          tree.appendItem(twig, "Fourth-Third-Third", doc, doc)
          leaf = tree.appendItem(twig, "Fourth-Third-Fourth",
                                  folder_open, folder_closed)
          leaf.setEnabled(false)
            tree.appendItem(leaf, "Fourth-Third-Fourth-First", doc, doc)
            tree.appendItem(leaf, "Fourth-Third-Fourth-Second", doc, doc)
            tree.appendItem(leaf, "Fourth-Third-Fourth-Third", doc, doc)
        twig = tree.appendItem(branch, "Fourth-Fourth",
                                folder_open, folder_closed)
          tree.appendItem(twig, "Fourth-Fourth-First", doc, doc)
          tree.appendItem(twig, "Fourth-Fourth-Second", doc, doc)
          tree.appendItem(twig, "Fourth-Fourth-Third", doc, doc)
          0.upto(9) { |i| tree.appendItem(twig, i.to_s, doc, doc) }
        twig = tree.appendItem(branch, "Fourth-Fifth",
                                folder_open, folder_closed)
        tree.expandTree(twig)
          tree.appendItem(twig, "Fourth-Fifth-First", doc, doc)
          tree.appendItem(twig, "Fourth-Fifth-Second", doc, doc)
          tree.appendItem(twig, "Fourth-Fifth-Third", doc, doc)
          0.upto(9) { |i| tree.appendItem(twig, i.to_s, doc, doc) }
      tree.appendItem(topmost, "Fifth", doc, doc)
      tree.appendItem(topmost, "Sixth", doc, doc)
      branch = tree.appendItem(topmost, "Seventh", folder_open, folder_closed)
        tree.appendItem(branch, "Seventh-First", doc, doc)
        tree.appendItem(branch, "Seventh-Second", doc, doc)
        tree.appendItem(branch, "Seventh-Third", doc, doc)
      tree.appendItem(topmost, "Eighth", doc, doc)
    
    FXLabel.new(group2, "Matrix", nil, LAYOUT_CENTER_X)
    FXHorizontalSeparator.new(group2, SEPARATOR_GROOVE|LAYOUT_FILL_X)
    matrix = FXMatrix.new(group2, 2, MATRIX_BY_COLUMNS|LAYOUT_FILL_X)
    
    FXLabel.new(matrix, "Alpha:", nil,
      JUSTIFY_RIGHT|LAYOUT_FILL_X|LAYOUT_CENTER_Y)
    FXTextField.new(matrix, 2, nil, 0, (FRAME_SUNKEN|FRAME_THICK|
      LAYOUT_FILL_X|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN))
    FXLabel.new(matrix, "Beta:", nil,
      JUSTIFY_RIGHT|LAYOUT_FILL_X|LAYOUT_CENTER_Y)
    FXTextField.new(matrix, 2, nil, 0, (FRAME_SUNKEN|FRAME_THICK|
      LAYOUT_FILL_X|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN))
    FXLabel.new(matrix, "Gamma:", nil,
      JUSTIFY_RIGHT|LAYOUT_FILL_X|LAYOUT_CENTER_Y)
    FXTextField.new(matrix, 2, nil, 0, (FRAME_SUNKEN|FRAME_THICK|
      LAYOUT_FILL_X|LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN))
   
    continuousCheck = FXCheckButton.new(group2,
      "Continuous Tracking\tSplitter continuously tracks split changes")
    continuousCheck.connect(SEL_COMMAND, method(:onCmdTracking))
    continuousCheck.connect(SEL_UPDATE, method(:onUpdTracking))
    
    FXLabel.new(group3, "Quite a Stretch", nil, LAYOUT_CENTER_X)
    FXHorizontalSeparator.new(group3, SEPARATOR_GROOVE|LAYOUT_FILL_X)
    mat = FXMatrix.new(group3, 3, LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    FXButton.new(mat, "One\nStretch the row\nStretch in Y\nStretch in X\t" +
      "The possibilities are endless..", nil, nil, 0,
      FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_ROW)
    FXButton.new(mat, "Two\nStretch in X\tThe possibilities are endless..", nil,
      nil, 0, FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X)
    FXButton.new(mat, "Three\nStretch the row\nStretch in Y\nStretch in X\t" +
      "The possibilities are endless..", nil, nil, 0,
      FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_ROW)
    
    FXButton.new(mat, "Four\nStretch the column\nStretch the row\n" +
      "Stretch in Y\nStretch in X\tThe possibilities are endless..", nil,
      nil, 0, (FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|
      LAYOUT_FILL_ROW|LAYOUT_FILL_COLUMN))
    FXButton.new(mat, "Five\nStretch the column\nStretch in Y\n" +
      "Stretch in X\tThe possibilities are endless..", nil, nil, 0,
      FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_COLUMN)
    FXButton.new(mat, "Six\nStretch the column\nStretch the row\n" +
      "Stretch in Y\nStretch in X\tThe possibilities are endless..", nil,
      nil, 0, (FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|
      LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))
    
    FXButton.new(mat, "Seven\nStretch the column\nStretch the row\n" +
      "Center in Y\nCenter in X\tThe possibilities are endless..", nil,
      nil, 0, (FRAME_RAISED|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_CENTER_X|
      LAYOUT_FILL_ROW|LAYOUT_FILL_COLUMN))
    FXButton.new(mat,
      "Eight\nStretch the column\tThe possibilities are endless..",nil,
      nil, 0, FRAME_RAISED|FRAME_THICK|LAYOUT_FILL_COLUMN)
    FXButton.new(mat, "Nine\nStretch the column\nStretch the row\n" +
      "Stretch in Y\tThe possibilities are endless..", nil, nil, 0,
      (FRAME_RAISED|FRAME_THICK|LAYOUT_RIGHT|LAYOUT_FILL_Y|
      LAYOUT_FILL_ROW|LAYOUT_FILL_COLUMN))
    
    # Make a tool tip
    FXToolTip.new(getApp(), 0)
  end

  def onCmdTracking(sender, sel, ptr)
    @splitter.splitterStyle ^= SPLITTER_TRACKING
    return 1
  end

  def onUpdTracking(sender, sel, ptr)
    if (@splitter.splitterStyle & SPLITTER_TRACKING) != 0
      sender.handle(self, FXSEL(SEL_COMMAND, ID_CHECK), nil)
    else
      sender.handle(self, FXSEL(SEL_COMMAND, ID_UNCHECK), nil)
    end
    return 1
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  application = FXApp.new("Splitter", "FoxTest")
  SplitterWindow.new(application)
  application.create
  application.run
end
