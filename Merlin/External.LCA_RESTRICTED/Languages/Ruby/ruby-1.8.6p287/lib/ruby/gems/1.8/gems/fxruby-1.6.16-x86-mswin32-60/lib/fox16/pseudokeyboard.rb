module Fox
  #
  # An FXPseudoKeyboard object provides a simple means to operate widgets
  # programmatically, to aid test driven design. An FXPseudoKeyboard instance
  # can be pointed at an FXObject and will manage the sending of events to
  # it.
  #
  # For example:
  #
  #   textfield = FXTextField.new(...)
  #   pk = FXPseudoKeyboard.new(textfield)
  #   pk.doKeyPress     # sends a SEL_KEYPRESS message to the textfield
  #   pk.doKeyRelease   # sends a SEL_KEYRELEASE message to the textfield
  #
  class FXPseudoKeyboard

    attr_accessor :target

    def initialize(tgt=nil)
      @target = tgt
    end
    
    def doKeyPress
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_KEYPRESS
        @target.handle(self, Fox.FXSEL(Fox::SEL_KEYPRESS, 0), evt)
      end
    end

    def doKeyRelease
      unless @target.nil?
        evt = FXEvent.new
        evt.type = Fox::SEL_KEYRELEASE
        @target.handle(self, Fox.FXSEL(Fox::SEL_KEYRELEASE, 0), evt)
      end
    end
  end
end

