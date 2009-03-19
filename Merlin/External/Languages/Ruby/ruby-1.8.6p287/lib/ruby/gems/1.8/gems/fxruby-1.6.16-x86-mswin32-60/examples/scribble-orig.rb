#!/usr/bin/env ruby

require 'fox16'
require 'fox16/responder'
require 'fox16/colors'

include Fox

class ScribbleWindow < FXMainWindow

  include Responder

  def initialize(app)
    # Call base class initializer first
    super(app, "Scribble Application", nil, nil, DECOR_ALL,
      0, 0, 800, 600)

    # Message identifiers for this class
    identifier :ID_CANVAS, :ID_CLEAR

    # And here's the message map for this class
    FXMAPFUNC(SEL_PAINT,             ID_CANVAS, :onPaint)
    FXMAPFUNC(SEL_LEFTBUTTONPRESS,   ID_CANVAS, :onMouseDown)
    FXMAPFUNC(SEL_LEFTBUTTONRELEASE, ID_CANVAS, :onMouseUp)
    FXMAPFUNC(SEL_MOTION,            ID_CANVAS, :onMouseMove)
    FXMAPFUNC(SEL_COMMAND,           ID_CLEAR,  :onCmdClear)
    FXMAPFUNC(SEL_UPDATE,            ID_CLEAR,  :onUpdClear)

    # Construct a horizontal frame to hold the main window's contents
    @contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y, 0, 0, 0, 0, 0, 0, 0, 0)

    # Left pane contains the canvas
    @canvasFrame = FXVerticalFrame.new(@contents,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      0, 0, 0, 0, 10, 10, 10, 10)

    # Place a label above the canvas
    FXLabel.new(@canvasFrame, "Canvas Frame", nil,
      JUSTIFY_CENTER_X|LAYOUT_FILL_X)

    # Horizontal divider line
    FXHorizontalSeparator.new(@canvasFrame, SEPARATOR_GROOVE|LAYOUT_FILL_X)

    # Drawing canvas
    @canvas = FXCanvas.new(@canvasFrame, self, ID_CANVAS,
      LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT)

    # Right pane for the buttons
    @buttonFrame = FXVerticalFrame.new(@contents,
      FRAME_SUNKEN|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      0, 0, 0, 0, 10, 10, 10, 10)

    # Label above the buttons
    FXLabel.new(@buttonFrame, "Button Frame", nil,
      JUSTIFY_CENTER_X|LAYOUT_FILL_X)

    # Horizontal divider line
    FXHorizontalSeparator.new(@buttonFrame,
      SEPARATOR_RIDGE|LAYOUT_FILL_X)

    # Button to clear the canvas
    FXButton.new(@buttonFrame, "&Clear", nil, self, ID_CLEAR,
      FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      0, 0, 0, 0, 10, 10, 5, 5)

    # Exit button
    FXButton.new(@buttonFrame, "&Exit", nil, app, FXApp::ID_QUIT,
      FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      0, 0, 0, 0, 10, 10, 5, 5)

    # Initialize other member variables
    @drawColor = FXColor::Red
    @mouseDown = false
    @dirty = false
  end

  # Create and show the main window
  def create
    super                  # Create the windows
    show(PLACEMENT_SCREEN) # Make the main window appear
  end

  # Mouse button was pressed somewhere
  def onMouseDown(sender, sel, ptr)
    @canvas.grab
    @mouseDown = true
    return 1
  end

  # Mouse has moved, so draw a line
  def onMouseMove(sender, sel, event)
    if @mouseDown
      # Get device context for the canvas
      dc = FXDCWindow.new(@canvas)

      # Set the foreground color for drawing
      dc.setForeground(@drawColor)

      # Draw a line from the previous mouse coordinates to the current ones
      dc.drawLine(event.last_x, event.last_y, event.win_x, event.win_y)

      # We have drawn something, so now the canvas is dirty
      @dirty = true

      # Release the DC immediately
      dc.end
    end
    return 1
  end

  # Mouse button released
  def onMouseUp(sender, sel, event)
    @canvas.ungrab
    if @mouseDown
      # Get device context for the canvas
      dc = FXDCWindow.new(@canvas)

      # Set the foreground color for drawing
      dc.setForeground(@drawColor)

      # Draw a line from the previous mouse coordinates to the current ones
      dc.drawLine(event.last_x, event.last_y, event.win_x, event.win_y)

      # We have drawn something, so now the canvas is dirty
      @dirty = true

      # Mouse no longer down
      @mouseDown = false

      # Release the DC immediately
      dc.end
    end
    return 1
  end

  # Paint the canvas
  def onPaint(sender, sel, event)
    dc = FXDCWindow.new(@canvas, event)
    dc.setForeground(@canvas.backColor)
    dc.fillRectangle(event.rect.x, event.rect.y, event.rect.w, event.rect.h)
    dc.end
  end

  # Handle the clear message
  def onCmdClear(sender, sel, ptr)
    dc = FXDCWindow.new(@canvas)
    dc.setForeground(@canvas.backColor)
    dc.fillRectangle(0, 0, @canvas.width, @canvas.height)
    @dirty = false
    dc.end
    return 1
  end

  # This function handles the update message sent by the Clear button
  # to its target. Every widget in FOX receives a message (SEL_UPDATE)
  # during idle processing, asking it to update itself. For example,
  # buttons could be enabled or disabled as the state of the application
  # changes.
  #
  # In this case, we'll disable the sender (the Clear button) when the
  # canvas has already been cleared (i.e. it's "clean"), and enable it when
  # it has been painted (i.e. it's "dirty").
  #
  def onUpdClear(sender, sel, ptr)
    message = @dirty ? FXWindow::ID_ENABLE : FXWindow::ID_DISABLE
    sender.handle(self, MKUINT(message, SEL_COMMAND), nil)
    return 1
  end
end

def run
  # Construct the application object
  application = FXApp.new('Scribble', 'FoxTest')
  
  # Construct the main window
  scribble = ScribbleWindow.new(application)
  
  # Create the application
  application.create
  
  # Run the application
  application.run
end

run
