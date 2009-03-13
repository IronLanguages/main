module Fox
  #
  # A window device context allows drawing into an FXDrawable, such as an
  # on-screen window (i.e. FXWindow and its derivatives) or an off-screen image (FXImage
  # and its derivatives).
  # Because certain hardware resources are locked down, only one FXDCWindow may be 
  # locked on a drawable at any one time.
  #
  class FXDCWindow < FXDC
    #
    # Construct a device context for drawing into a window (specified by _drawable_).
    # If _event_ is +nil_, the device context is constructed for normal drawing, and the
    # clip rectangle is set to the whole rectange.
    # If _event_ is a reference to an FXEvent, the device context is constructed for
    # painting in response to an expose; this sets the clip rectangle to the exposed rectangle.
    # If an optional code block is provided, the new device context will be passed into the block as an
    # argument and #end will be called automatically when the block terminates.
    #
    def initialize(drawable, event=nil)	# :yields: dc
    end

    #
    # Lock in a drawable surface.
    #
    def begin(drawable) ; end

    #
    # Unlock the drawable surface.
    #
    def end() ; end
  end
end

