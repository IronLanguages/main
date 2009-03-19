#!/usr/bin/env ruby

require 'fox16'
require 'fox16/colors'

include Fox

BLURB = <<END
FXDataTarget can be used to connect a widget to an application variable without any of the
tradional "glue" programming code.

The widgets below are connected (via FXDataTarget) to an integer, real, string, option, and
color variable, respectively.

Changing one of them will cause all widgets connected to the same FXDataTarget to
update so as to reflect the value of the application variable.

The progress bar below shows a time-varying variable, demonstrating that widgets
can be updated via FXDataTarget's regardless how the variables are changed.

Note that the "Option" pulldown menu is also connected to the option variable!
END

class DataTargetWindow < FXMainWindow
  def initialize(app)
    # Initialize base class
    super(app, "Data Targets Test", :opts => DECOR_ALL, :x => 20, :y => 20, :width => 700, :height => 460)

    # Create a data target with an integer value
    @intTarget = FXDataTarget.new(10)

    # Create a data target with a floating point value
    @floatTarget = FXDataTarget.new(3.1415927)

    # Create a data target with a string value
    @stringTarget = FXDataTarget.new("FOX")

    # Create a data target with a color value
    @colorTarget = FXDataTarget.new(FXColor::Red)

    # Create an integer data target to track the selected option
    @optionTarget = FXDataTarget.new(1)

    # Create another integer data target to track the "progress"
    @progressTarget = FXDataTarget.new(0)

    # Menubar
    menubar = FXMenuBar.new(self, LAYOUT_SIDE_TOP|LAYOUT_FILL_X)
    
    # File menu
    filemenu = FXMenuPane.new(self)
    FXMenuCommand.new(filemenu, "Progress dialog...").connect(SEL_COMMAND) do
      @progressdialog.show(PLACEMENT_OWNER)
    end
    FXMenuCommand.new(filemenu, "&Quit\tCtl-Q", nil, getApp(), FXApp::ID_QUIT)
    FXMenuTitle.new(menubar, "&File", nil, filemenu)

    # Create a progress dialog that's also tied into the integer data target's value
    @progressdialog = FXProgressDialog.new(self, "Progress", "Incoming...", PROGRESSDIALOG_CANCEL|DECOR_BORDER|DECOR_RESIZE)
    @progressdialog.target = @intTarget
    @progressdialog.selector = FXDataTarget::ID_VALUE

    # Option menu
    optionmenu = FXMenuPane.new(self)
    FXMenuCheck.new(optionmenu, "Option 1", @optionTarget, FXDataTarget::ID_OPTION+1)
    FXMenuCheck.new(optionmenu, "Option 2", @optionTarget, FXDataTarget::ID_OPTION+2)
    FXMenuCheck.new(optionmenu, "Option 3", @optionTarget, FXDataTarget::ID_OPTION+3)
    FXMenuCheck.new(optionmenu, "Option 4", @optionTarget, FXDataTarget::ID_OPTION+4)
    FXMenuTitle.new(menubar, "&Option", nil, optionmenu)

    # Lone progress bar at the bottom
    FXProgressBar.new(self, @progressTarget, FXDataTarget::ID_VALUE,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|FRAME_SUNKEN|FRAME_THICK)

    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_TOP|SEPARATOR_GROOVE|LAYOUT_FILL_X)

    horframe = FXHorizontalFrame.new(self, LAYOUT_SIDE_TOP | LAYOUT_FILL_X)
    FXLabel.new(horframe, BLURB, nil, LAYOUT_SIDE_TOP | JUSTIFY_LEFT)

    FXProgressBar.new(horframe, @intTarget, FXDataTarget::ID_VALUE,
      PROGRESSBAR_PERCENTAGE | PROGRESSBAR_DIAL | LAYOUT_SIDE_TOP |
      LAYOUT_SIDE_RIGHT | LAYOUT_RIGHT | LAYOUT_FILL_Y | LAYOUT_FILL_X)

    FXHorizontalSeparator.new(self,
      LAYOUT_SIDE_TOP|SEPARATOR_GROOVE|LAYOUT_FILL_X)
    FXSlider.new(self, @intTarget, FXDataTarget::ID_VALUE, (SLIDER_VERTICAL|
      SLIDER_INSIDE_BAR|LAYOUT_SIDE_RIGHT|LAYOUT_FILL_Y|LAYOUT_FIX_WIDTH),
      :width => 20)

    # Arrange nicely
    matrix = FXMatrix.new(self, 7,
      MATRIX_BY_COLUMNS|LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y)

    # First row
    FXLabel.new(matrix, "&Integer", nil,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|JUSTIFY_RIGHT|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @intTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @intTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW)
    FXSlider.new(matrix, @intTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_FILL_ROW|LAYOUT_FIX_WIDTH, :width => 100)
    FXDial.new(matrix, @intTarget, FXDataTarget::ID_VALUE, (LAYOUT_CENTER_Y|
      LAYOUT_FILL_ROW|LAYOUT_FIX_WIDTH|DIAL_HORIZONTAL|DIAL_HAS_NOTCH),
      :width => 100)
    FXSpinner.new(matrix, 5, @intTarget, FXDataTarget::ID_VALUE,
      SPIN_CYCLIC|FRAME_SUNKEN|FRAME_THICK|LAYOUT_CENTER_Y|LAYOUT_FILL_ROW)
    FXProgressBar.new(matrix, @intTarget, FXDataTarget::ID_VALUE,
      (LAYOUT_CENTER_Y|LAYOUT_FILL_X|FRAME_SUNKEN|FRAME_THICK|
      PROGRESSBAR_PERCENTAGE|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW))

    # Second row
    FXLabel.new(matrix, "&Real", nil,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|JUSTIFY_RIGHT|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @floatTarget, FXDataTarget::ID_VALUE,
      (TEXTFIELD_REAL|LAYOUT_CENTER_Y|LAYOUT_CENTER_X|
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW))
    FXTextField.new(matrix, 10, @floatTarget, FXDataTarget::ID_VALUE,
      (TEXTFIELD_REAL|LAYOUT_CENTER_Y|LAYOUT_CENTER_X|
      FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW))
    FXSlider.new(matrix, @floatTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW|LAYOUT_FIX_WIDTH, :width => 100)
    FXDial.new(matrix, @floatTarget, FXDataTarget::ID_VALUE,
      (LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW|LAYOUT_FIX_WIDTH|
      DIAL_HORIZONTAL|DIAL_HAS_NOTCH), :width => 100)

    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)

    # Third row
    FXLabel.new(matrix, "&String", nil,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|JUSTIFY_RIGHT|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @stringTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @stringTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)

    # Fourth row
    FXLabel.new(matrix, "&Option", nil,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|JUSTIFY_RIGHT|LAYOUT_FILL_ROW)
    FXTextField.new(matrix, 10, @optionTarget, FXDataTarget::ID_VALUE,
      (TEXTFIELD_INTEGER|LAYOUT_CENTER_Y|LAYOUT_CENTER_X|FRAME_SUNKEN|FRAME_THICK|LAYOUT_FILL_ROW))
    FXRadioButton.new(matrix, "Option &1",
      @optionTarget, FXDataTarget::ID_OPTION + 1,
      LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW|ICON_BEFORE_TEXT)
    FXRadioButton.new(matrix, "Option &2",
      @optionTarget, FXDataTarget::ID_OPTION + 2,
      LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW|ICON_BEFORE_TEXT)
    FXRadioButton.new(matrix, "Option &3",
      @optionTarget, FXDataTarget::ID_OPTION + 3,
      LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW|ICON_BEFORE_TEXT)
    FXRadioButton.new(matrix, "Option &4",
      @optionTarget, FXDataTarget::ID_OPTION + 4,
      LAYOUT_CENTER_Y|LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW|ICON_BEFORE_TEXT)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)

    # Fifth
    FXLabel.new(matrix, "&Color", nil,
      LAYOUT_CENTER_Y|LAYOUT_CENTER_X|JUSTIFY_RIGHT|LAYOUT_FILL_ROW)
    FXColorWell.new(matrix, 0, @colorTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
    FXColorWell.new(matrix, 0, @colorTarget, FXDataTarget::ID_VALUE,
      LAYOUT_CENTER_Y|LAYOUT_FILL_X|LAYOUT_FILL_ROW, :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)
    FXFrame.new(matrix, LAYOUT_FILL_COLUMN|LAYOUT_FILL_ROW)

    # Install an accelerator
    self.accelTable.addAccel(fxparseAccel("Ctl-Q"), getApp(), FXSEL(SEL_COMMAND, FXApp::ID_QUIT))
  end

  # Timer expired; update the progress
  def onTimeout(sender, sel, ptr)
    # Increment the progress modulo 100
    @progressTarget.value = (@progressTarget.value + 1) % 100
  
    # Reset the timer for next time
    getApp().addTimeout(80, method(:onTimeout))
  end

    # Quit
    def onCmdQuit(sender, sel, ptr)
      getApp.exit(0)
    end

  # Start
  def create
    # Create window
    super

    # Kick off the timer
    getApp().addTimeout(80, method(:onTimeout))

    # Show the main window
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  # Make an application
  application = FXApp.new("DataTarget", "FoxTest")

  # Current threads implementation causes problems for this example, so disable
  application.threadsEnabled = false

  # Create main window
  window = DataTargetWindow.new(application)

  # Handle interrupts to quit application gracefully
  application.addSignal("SIGINT", window.method(:onCmdQuit))

  # Create the application
  application.create

  # Run
  application.run
end
