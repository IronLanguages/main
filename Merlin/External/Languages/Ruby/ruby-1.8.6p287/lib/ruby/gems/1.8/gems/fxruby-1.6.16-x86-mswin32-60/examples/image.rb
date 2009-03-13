#!/usr/bin/env ruby

require 'fox16'
require 'fox16/colors'

include Fox

class ImageWindow < FXMainWindow

  def initialize(app)
    # Invoke base class initializer first
    super(app, "Image Application", :opts => DECOR_ALL, :width => 800, :height => 600)

    # Create a color dialog for later use
    colordlg = FXColorDialog.new(self, "Color Dialog")
    
    contents = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y,
      :padLeft => 0, :padRight => 0, :padTop => 0, :padBottom => 0)
  
    # LEFT pane to contain the canvas
    canvasFrame = FXVerticalFrame.new(contents,
      FRAME_SUNKEN|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10)
  
    # Label above the canvas               
    FXLabel.new(canvasFrame, "Canvas Frame", :opts => JUSTIFY_CENTER_X|LAYOUT_FILL_X)
  
    # Horizontal divider line
    FXHorizontalSeparator.new(canvasFrame, SEPARATOR_GROOVE|LAYOUT_FILL_X)

    # Drawing canvas
    @canvas = FXCanvas.new(canvasFrame, :opts => (FRAME_SUNKEN|
      FRAME_THICK|LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT))
    @canvas.connect(SEL_PAINT, method(:onCanvasRepaint))

    # RIGHT pane for the buttons
    buttonFrame = FXVerticalFrame.new(contents,
      :opts => FRAME_SUNKEN|LAYOUT_FILL_Y|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 10, :padBottom => 10)
      

    # Label above the buttons  
    FXLabel.new(buttonFrame, "Button Frame", nil,
      JUSTIFY_CENTER_X|LAYOUT_FILL_X);
    
    # Horizontal divider line
    FXHorizontalSeparator.new(buttonFrame, SEPARATOR_RIDGE|LAYOUT_FILL_X)

    FXLabel.new(buttonFrame, "&Background\nColor well", :opts => JUSTIFY_CENTER_X|LAYOUT_FILL_X)
    @backwell = FXColorWell.new(buttonFrame, FXColor::White,
      :opts => LAYOUT_CENTER_X|LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT,
      :width => 100, :height => 30)
    @backwell.connect(SEL_COMMAND, method(:onCmdWell))
    
    FXLabel.new(buttonFrame, "B&order\nColor well", :opts => JUSTIFY_CENTER_X|LAYOUT_FILL_X)
    @borderwell = FXColorWell.new(buttonFrame, FXColor::Black,
      :opts => LAYOUT_CENTER_X|LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT,
      :width => 100, :height => 30)
    @borderwell.connect(SEL_COMMAND, method(:onCmdWell))
    
    FXLabel.new(buttonFrame, "&Text\nColor well", :opts => JUSTIFY_CENTER_X|LAYOUT_FILL_X)
    @textwell = FXColorWell.new(buttonFrame, FXColor::Black,
      :opts => LAYOUT_CENTER_X|LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FIX_WIDTH|LAYOUT_FIX_HEIGHT,
      :width => 100, :height => 30)
    @textwell.connect(SEL_COMMAND, method(:onCmdWell))
    
    # Button to draw
    FXButton.new(buttonFrame, "&Colors...\tPop the color dialog", nil,
      colordlg, FXWindow::ID_SHOW,
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 5, :padBottom => 5)

    # Button to draw
    saveBtn = FXButton.new(buttonFrame,
      "Save Image...\tRead back image and save to file",
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 5, :padBottom => 5)
    saveBtn.connect(SEL_COMMAND, method(:onCmdRestore))
    
    # Exit button
    FXButton.new(buttonFrame, "E&xit\tQuit ImageApp", nil,
      getApp(), FXApp::ID_QUIT,
      :opts => FRAME_THICK|FRAME_RAISED|LAYOUT_FILL_X|LAYOUT_TOP|LAYOUT_LEFT,
      :padLeft => 10, :padRight => 10, :padTop => 5, :padBottom => 5)

    # Allocate the color arrays
    imgWidth = 512
    imgHeight = 50

    # Create images with dithering
    @grey = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_DITHER|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @red = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_DITHER|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @green = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_DITHER|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @blue = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_DITHER|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
  
    # Create image with nearest color instead of dithering
    @grey_nodither = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_NEAREST|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @red_nodither = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_NEAREST|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @green_nodither = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_NEAREST|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
    @blue_nodither = FXImage.new(getApp(), nil,
      IMAGE_OWNED|IMAGE_NEAREST|IMAGE_SHMI|IMAGE_SHMP, imgWidth, imgHeight)
  
    # Result image
    @picture = FXBMPImage.new(getApp(), nil, IMAGE_SHMI|IMAGE_SHMP, 850, 600)

    # Fill up the color-ramp byte arrays (strings)
    grey_data = @grey.data
    grey_nodither_data = @grey_nodither.data
    red_data = @red.data
    red_nodither_data = @red_nodither.data
    green_data = @green.data
    green_nodither_data = @green_nodither.data
    blue_data = @blue.data
    blue_nodither_data = @blue_nodither.data
    (0...512).each { |x|
      halfX = x >> 1
      (0...50).each { |y|
        z = (y << 9) + x
        grey_data[z]  = FXRGB(halfX, halfX, halfX)
        grey_nodither_data[z] = grey_data[z]
        red_data[z]   = FXRGB(halfX, 0, 0)
        red_nodither_data[z] = red_data[z]
        green_data[z] = FXRGB(0, halfX, 0)
        green_nodither_data[z] = green_data[z]
        blue_data[z]  = FXRGB(0, 0, halfX)
        blue_nodither_data[z] = blue_data[z]
      }
    }

    # Make font
    @font = FXFont.new(getApp(), "times", 36, FONTWEIGHT_BOLD)
  
    # Make a tip
    FXToolTip.new(getApp())
  end

  # Create and initialize
  def create
    # Create the windows
    super

    # Create images
    @grey.create
    @red.create
    @green.create
    @blue.create
    @grey_nodither.create
    @red_nodither.create
    @green_nodither.create
    @blue_nodither.create
    @picture.create

    # Create the font, too
    @font.create

    # Make the main window appear
    show(PLACEMENT_SCREEN)

    # Initial repaint
    @canvas.update
  end

  def onCanvasRepaint(sender, sel, event)
    if event.synthetic?
      dc = FXDCWindow.new(@picture)
  
      # Erase the canvas, color comes from well
      dc.foreground = @backwell.rgba

      dc.fillRectangle(0, 0, @picture.width, @picture.height)

      # Draw images
      dc.drawImage(@grey, 10, 10)
      dc.drawImage(@grey_nodither, 10, 60)
      dc.drawImage(@red, 10, 130)
      dc.drawImage(@red_nodither, 10, 180)
      dc.drawImage(@green, 10, 250)
      dc.drawImage(@green_nodither, 10, 300)
      dc.drawImage(@blue, 10, 370)
      dc.drawImage(@blue_nodither, 10, 420)

      # Draw patterns
      dc.fillStyle = FILL_OPAQUESTIPPLED
      dc.foreground = FXColor::Black
      dc.background = FXColor::White
      (STIPPLE_0..STIPPLE_16).each do |pat|
        dc.stipple = pat
        dc.fillRectangle(10 + (512*pat)/17, 490, 31, 50)
      end
      dc.fillStyle = FILL_SOLID

      # Draw borders
      dc.foreground = @borderwell.rgba
      dc.drawRectangle(10, 10, 512, 50)
      dc.drawRectangle(10, 60, 512, 50)
  
      dc.drawRectangle(10, 130, 512, 50)
      dc.drawRectangle(10, 180, 512, 50)
  
      dc.drawRectangle(10, 250, 512, 50)
      dc.drawRectangle(10, 300, 512, 50)
  
      dc.drawRectangle(10, 370, 512, 50)
      dc.drawRectangle(10, 420, 512, 50)
  
      dc.drawRectangle(10, 490, 512, 50)
  
      # Draw text
      dc.font = @font
      dc.foreground = @textwell.rgba
      dc.drawText(540,  60, "Grey")
      dc.drawText(540, 180, "Red")
      dc.drawText(540, 300, "Green")
      dc.drawText(540, 420, "Blue")
      dc.drawText(540, 540, "Patterns")

      # 
      # Call end() to unlock the drawing surface and flush out
      # the pending drawing commands.
      #
      dc.end
    end
      
    # Now repaint the screen
    sdc = FXDCWindow.new(@canvas, event)
    
    # Clear whole thing
    sdc.foreground = @backwell.rgba
    sdc.fillRectangle(0, 0, @canvas.width, @canvas.height)
    
    # Paint image
    sdc.drawImage(@picture, 0, 0)

    # 
    # Call end() to unlock the drawing surface and flush out
    # the pending drawing commands.
    #
    sdc.end
  end

  # Color well got changed
  def onCmdWell(sender, sel, ptr)
    @canvas.update
  end

  # Restore image from offscreen pixmap
  def onCmdRestore(sender, sel, ptr)
    saveDialog = FXFileDialog.new(self, "Save as BMP")
    if saveDialog.execute != 0
      FXFileStream.open(saveDialog.filename, FXStreamSave) do |outfile|
        @picture.restore
        @picture.savePixels(outfile)
      end
    end
    return 1
  end
end

if __FILE__ == $0
  # Make application
  application = FXApp.new("Image", "FoxTest")

  # Make the main window
  ImageWindow.new(application)

  # Create the application window and resources
  application.create

  # Run the application
  application.run
end
