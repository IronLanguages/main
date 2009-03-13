#!/usr/bin/env ruby

require 'fox16'
require 'fox16/colors'

include Fox

PREAMBLE = <<EOM
We the people of the United States, in order to form a more perfect union,
establish justice, insure domestic tranquility, provide for the common defense,
promote the general welfare, and secure the blessings of liberty to ourselves and
our posterity, do ordain and establish this Constitution for the United States of America.
EOM
 
class StyledTextWindow < FXMainWindow
  def initialize(app)
    # Call the base class initialize() first
    super(app, "Styled Text Test")
    self.width = 400
    self.height = 300

    # Menu bar, along the top
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
  
    # Button bar along the bottom
    buttons = FXHorizontalFrame.new(self, LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X)
    
    # The frame takes up the rest of the space
    textframe = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_SUNKEN|FRAME_THICK)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q\tQuit the application.", nil,
      getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
    
    # Text window
    text = FXText.new(textframe, nil, 0,
      TEXT_READONLY|TEXT_WORDWRAP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # Construct some hilite styles
    hs1 = FXHiliteStyle.from_text(text)
    hs1.normalForeColor = FXColor::Red
    hs1.normalBackColor = FXColor::Blue
    hs1.style = FXText::STYLE_BOLD

    hs2 = FXHiliteStyle.from_text(text)
    hs2.normalForeColor = FXColor::Blue
    hs2.normalBackColor = FXColor::Yellow
    hs2.style = FXText::STYLE_UNDERLINE
    
    # Enable the style buffer for this text widget
    text.styled = true
    
    # Set the styles
    text.hiliteStyles = [hs1, hs2]
    
    # Set the text
    text.text = PREAMBLE.gsub!(/\n/, "")
    
    # Change the style for this phrase to hs1 [index 1]
    phrase = "a more perfect union"
    text.changeStyle(PREAMBLE.index(phrase), phrase.length, 1)

    # Change the style for this phrase to hs2 [index 2]
    phrase = "United States of America"
    text.changeStyle(PREAMBLE.index(phrase), phrase.length, 2)
  end

  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  application = FXApp.new("StyledText", "FoxTest")
  StyledTextWindow.new(application)
  application.create
  application.run
end
