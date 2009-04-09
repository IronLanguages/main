module Fox
  #
  # An FXPseudoMouse object provides a simple means to operate widgets
  # programmatically, to aid test driven design. An FXPseudoMouse instance
  # can be pointed at an FXObject and will manage the sending of events to
  # it.
  #
  # For example:
  #
  #   canvas = FXCanvas.new(...)
  #   pm = FXPseudoMouse.new(canvas)
  #   pm.doLeftButtonPress # sends a SEL_LEFTBUTTONPRESS message to the canvas
  #
  class FXPseudoMouse < FXObject

    attr_accessor :target

    def initialize(tgt=nil)
      @target = tgt
    end
    
    def doMotion
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_MOTION
        @target.handle(self, Fox.FXSEL(Fox::SEL_MOTION, 0), evt)
      end
    end

    def doMouseWheel
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_MOUSEWHEEL
        @target.handle(self, Fox.FXSEL(Fox::SEL_MOUSEWHEEL, 0), evt)
      end
    end

    def doLeftButtonPress
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_LEFTBUTTONPRESS
        @target.handle(self, Fox.FXSEL(Fox::SEL_LEFTBUTTONPRESS, 0), evt)
      end
    end

    def doLeftButtonRelease
      unless @target.nil?
       evt = FXEvent.new
        evt.type = Fox::SEL_LEFTBUTTONRELEASE
       @target.handle(self, Fox.FXSEL(Fox::SEL_LEFTBUTTONRELEASE, 0), evt)
      end
    end

    def doMiddleButtonPress
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_MIDDLEBUTTONPRESS
        @target.handle(self, Fox.FXSEL(Fox::SEL_MIDDLEBUTTONPRESS, 0), evt)
      end
    end

    def doMiddleButtonRelease
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_MIDDLEBUTTONRELEASE
        @target.handle(self, Fox.FXSEL(Fox::SEL_MIDDLEBUTTONRELEASE, 0), evt)
      end
    end

    def doRightButtonPress
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_RIGHTBUTTONPRESS
        @target.handle(self, Fox.FXSEL(Fox::SEL_RIGHTBUTTONPRESS, 0), evt)
      end
    end

    def doRightButtonRelease
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_RIGHTBUTTONRELEASE
        @target.handle(self, Fox.FXSEL(Fox::SEL_RIGHTBUTTONRELEASE, 0), evt)
      end
    end
  end
end

