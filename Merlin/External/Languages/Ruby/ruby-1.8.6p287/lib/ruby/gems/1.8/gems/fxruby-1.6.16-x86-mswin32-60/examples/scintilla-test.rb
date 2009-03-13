#!/usr/bin/env ruby

require 'fox16'
require 'fox16/scintilla'

include Fox

ABOUT_MSG = <<EOM
The FOX GUI toolkit is developed by Jeroen van der Zijp.
The Scintilla source code editing component is developed by Neil Hodgson.
The FXScintilla widget is developed by Gilles Filippini.
and FXRuby is developed by Lyle Johnson.
EOM

class ScintillaTest  < FXMainWindow

  def initialize(app)
    # Invoke base class initialize method first
    super(app, "Scintilla Test", nil, nil, DECOR_ALL, 0, 0, 800, 600)

    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
  
    # Status bar
    FXStatusBar.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|STATUSBAR_WITH_DRAGCORNER)

    # Scintilla widget takes up the rest of the space
    sunkenFrame = FXHorizontalFrame.new(self,
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @scintilla = FXScintilla.new(sunkenFrame, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "&Open\tCtl-O\tOpen...").connect(SEL_COMMAND) {
      openDialog = FXFileDialog.new(self, "Open Document")
      openDialog.selectMode = SELECTFILE_EXISTING
      openDialog.patternList = ["All Files (*.*)", "Ruby Files (*.rb)"]
      if openDialog.execute != 0
        loadFile(openDialog.filename)
      end
    }
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q\tQuit application.", nil,
      getApp(), FXApp::ID_QUIT, 0)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)
      
    # Help menu
    helpmenu = FXMenuPane.new(self)
    FXMenuCommand.new(helpmenu, "&About FXRuby...").connect(SEL_COMMAND) {
      FXMessageBox.information(self, MBOX_OK, "About FXRuby", ABOUT_MSG)
    }
    FXMenuTitle.new(menubar, "&Help", nil, helpmenu, LAYOUT_RIGHT)
  end

  def loadFile(filename)
    getApp().beginWaitCursor do
      text = File.open(filename, "r").read
      @scintilla.setText(text)
    end
  end

  # Start
  def create
    super
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("ScintillaTest", "FoxTest")
  
  # Make window
  ScintillaTest.new(application)
  
  # Create app
  application.create
  
  # Run
  application.run
end
