#!/usr/bin/env ruby

require 'fox16'

include Fox

class ScribbleWindow < FXMainWindow

  def initialize(app)
    # Call base class initializer first
    super(app, "Scribble Application", :width => 800, :height => 600)

    # Construct a horizontal frame to hold the main window's contents
    @contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)

    # Left pane contains the canvas
    @canvasFrame = FXVerticalFrame.new(@contents,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10)

    # Place a label above the canvas
    FXLabel.new(@canvasFrame, "Canvas Frame", nil, JUSTIFY_CENTER_X|LAYOUT_FILL_X)

    # Horizontal divider line
    FXHorizontalSeparator.new(@canvasFrame, SEPARATOR_GROOVE|LAYOUT_FILL_X)

    # Drawing canvas
    @canvas = FXCanvas.new(@canvasFrame, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)
    @canvas.connect(SEL_PAINT) do |sender, sel, event|
      FXDCWindow.new(@canvas, event) do |dc|
        dc.foreground = @canvas.backColor
        dc.fillRectangle(event.rect.x, event.rect.y, event.rect.w, event.rect.h)
      end
    end
    @canvas.connect(SEL_LEFTBUTTONPRESS) do
      @canvas.grab
      @mouseDown = true
    end
    @canvas.connect(SEL_MOTION) do |sender, sel, event|
      if @mouseDown
        # Get device context for the canvas
        dc = FXDCWindow.new(@canvas)

        # Set the foreground color for drawing
        dc.foreground = @drawColor

        # Draw a line from the previous mouse coordinates to the current ones
        if @mirrorMode.value
          cW = @canvas.width
          cH = @canvas.height
          dc.drawLine(cW-event.last_x, event.last_y,
                      cW-event.win_x, event.win_y)
          dc.drawLine(event.last_x, cH-event.last_y,
                      event.win_x, cH-event.win_y)
          dc.drawLine(cW-event.last_x, cH-event.last_y,
                      cW-event.win_x, cH-event.win_y)
        end
        dc.drawLine(event.last_x, event.last_y, event.win_x, event.win_y)

        # We have drawn something, so now the canvas is dirty
        @dirty = true

        # Release the DC immediately
        dc.end
      end
    end
    @canvas.connect(SEL_LEFTBUTTONRELEASE) do |sender, sel, event|
      @canvas.ungrab
      if @mouseDown
        # Get device context for the canvas
        dc = FXDCWindow.new(@canvas)

        # Set the foreground color for drawing
        dc.foreground = @drawColor

        # Draw a line from the previous mouse coordinates to the current ones
        dc.drawLine(event.last_x, event.last_y, event.win_x, event.win_y)

        # We have drawn something, so now the canvas is dirty
        @dirty = true

        # Mouse no longer down
        @mouseDown = false

        # Release this DC immediately
        dc.end
      end
    end

    # Right pane for the buttons
    @buttonFrame = FXVerticalFrame.new(@contents,
      FRAME_SUNKEN|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10)

    # Label above the buttons
    FXLabel.new(@buttonFrame, "Button Frame", nil, JUSTIFY_CENTER_X|LAYOUT_FILL_X)

    # Horizontal divider line
    FXHorizontalSeparator.new(@buttonFrame, SEPARATOR_RIDGE|LAYOUT_FILL_X)

    # Enable or disable mirror mode
    @mirrorMode = FXDataTarget.new(false)
    FXCheckButton.new(@buttonFrame, "Mirror", @mirrorMode, FXDataTarget::ID_VALUE, CHECKBUTTON_NORMAL|LAYOUT_FILL_X)

    # Button to clear the canvas
    clearButton = FXButton.new(@buttonFrame, "&Clear",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 5, :padBottom => 5)
    clearButton.connect(SEL_COMMAND) do
      FXDCWindow.new(@canvas) do |dc|
        dc.foreground = @canvas.backColor
        dc.fillRectangle(0, 0, @canvas.width, @canvas.height)
        @dirty = false
      end
    end
    clearButton.connect(SEL_UPDATE) do |sender, sel, ptr|
      # This procedure handles the update message sent by the Clear button
      # to its target. Every widget in FOX receives a message (SEL_UPDATE)
      # during idle processing, asking it to update itself. For example,
      # buttons could be enabled or disabled as the state of the application
      # changes.
      #
      # In this case, we'll disable the sender (the Clear button) when the
      # canvas has already been cleared (i.e. it's "clean"), and enable it when
      # it has been painted (i.e. it's "dirty").
      message = @dirty ? FXWindow::ID_ENABLE : FXWindow::ID_DISABLE
      sender.handle(self, MKUINT(message, SEL_COMMAND), nil)
    end

    # Exit button
    FXButton.new(@buttonFrame, "&Exit", nil, app, FXApp::ID_QUIT,
      FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 5, :padBottom => 5)

    # Initialize other member variables
    @drawColor = "red"
    @mouseDown = false
    @dirty = false
  end

  # Create and show the main window
  def create
    super                  # Create the windows
    show(PLACEMENT_SCREEN) # Make the main window appear
  end
end

if __FILE__ == $0
  # Construct the application object
  application = FXApp.new('Scribble', 'FoxTest')
  
  # Construct the main window
  scribble = ScribbleWindow.new(application)
  
  # Create the application
  application.create
  
  # Run the application
  application.run
end
