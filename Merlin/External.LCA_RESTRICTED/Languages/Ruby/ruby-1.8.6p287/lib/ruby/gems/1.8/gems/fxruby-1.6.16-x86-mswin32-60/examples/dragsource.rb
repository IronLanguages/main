require 'fox16'

include Fox

class DragSource < FXMainWindow
  
  def initialize(anApp)
    # Initialize base class
    super(anApp, "Drag Source", :opts => DECOR_ALL, :width => 400, :height => 300)
    
    # Fill main window with canvas
    @canvas = FXCanvas.new(self, :opts => LAYOUT_FILL_X|LAYOUT_FILL_Y)
    @canvas.backColor = "red"
    
    # Handle expose events on the canvas
    @canvas.connect(SEL_PAINT) do |sender, sel, event|
      FXDCWindow.new(@canvas, event) do |dc|
        dc.foreground = @canvas.backColor
        dc.fillRectangle(event.rect.x, event.rect.y, event.rect.w, event.rect.h)
      end
    end

    # Handle left button press
    @canvas.connect(SEL_LEFTBUTTONPRESS) do
      #
      # Capture (grab) the mouse when the button goes down, so that all future
      # mouse events will be reported to this widget, even if those events occur
      # outside of this widget.
      #
      @canvas.grab

      # Advertise which drag types we can offer
      dragTypes = [FXWindow.colorType]
      @canvas.beginDrag(dragTypes)
    end
    
    # Handle mouse motion events
    @canvas.connect(SEL_MOTION) do |sender, sel, event|
      if @canvas.dragging?
        @canvas.handleDrag(event.root_x, event.root_y)
        unless @canvas.didAccept == DRAG_REJECT
          @canvas.dragCursor = getApp().getDefaultCursor(DEF_SWATCH_CURSOR)
        else
          @canvas.dragCursor = getApp().getDefaultCursor(DEF_DNDSTOP_CURSOR)
        end
      end
    end

    # Handle left button release
    @canvas.connect(SEL_LEFTBUTTONRELEASE) do
      @canvas.ungrab
      @canvas.endDrag
    end
    
    # Handle request for DND data
    @canvas.connect(SEL_DND_REQUEST) do |sender, sel, event|
      @canvas.setDNDData(FROM_DRAGNDROP, FXWindow.colorType, Fox.fxencodeColorData(@canvas.backColor)) if event.target == FXWindow.colorType
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
  FXApp.new("DragSource", "FXRuby") do |theApp|
    DragSource.new(theApp)
    theApp.create
    theApp.run
  end
end

