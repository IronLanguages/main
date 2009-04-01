require 'fox16'

include Fox

class DropSite < FXMainWindow
  
  def initialize(anApp)
    # Initialize base class
    super(anApp, "Drop Site", :opts => DECOR_ALL, :width => 400, :height => 300)
    
    # Fill main window with canvas
    @canvas = FXCanvas.new(self, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y)
    
    # Handle expose events on the canvas
    @canvas.connect(SEL_PAINT) do |sender, sel, event|
      FXDCWindow.new(@canvas, event) do |dc|
        dc.foreground = @canvas.backColor
        dc.fillRectangle(event.rect.x, event.rect.y, event.rect.w, event.rect.h)
      end
    end

    # Enable canvas for drag-and-drop messages
    @canvas.dropEnable
    
    # Handle SEL_DND_MOTION messages from the canvas
    @canvas.connect(SEL_DND_MOTION) do
      @canvas.acceptDrop if @canvas.offeredDNDType?(FROM_DRAGNDROP, FXWindow.colorType)
    end

    # Handle SEL_DND_DROP message from the canvas
    @canvas.connect(SEL_DND_DROP) do
      # Try to obtain the data as color values first
      data = @canvas.getDNDData(FROM_DRAGNDROP, FXWindow.colorType)

      # Update canvas background color
      @canvas.backColor = Fox.fxdecodeColorData(data) unless data.nil?
    end
  end

  def create
    # Create the main window and canvas
    super
    
    # Register the drag type for colors
    FXWindow.colorType = getApp().registerDragType(FXWindow.colorTypeName)

    # Show the main window
    show(PLACEMENT_SCREEN)
  end
end

if __FILE__ == $0
  FXApp.new("DropSite", "FXRuby") do |theApp|
    DropSite.new(theApp)
    theApp.create
    theApp.run
  end
end

