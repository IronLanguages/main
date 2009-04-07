module Fox
  # Module to include in FXDialogBox to provide an easy nonmodal version
  # of execute.
  module FTNonModal

    def initialize(*args)
      super if defined?(super)
      FXMAPFUNC(SEL_COMMAND, FXDialogBox::ID_CANCEL, :onCmdCancel)
      FXMAPFUNC(SEL_COMMAND, FXDialogBox::ID_ACCEPT, :onCmdAccept)
    end

    #
    # Creates and shows the dialog, and registers the associated block to be
    # called when the dialog is closed. The block is passed a boolean argument
    # which is true if and only if the dialog was accepted.
    #
    # For example:
    #
    #   dialogBox.execute_modal do |accepted|
    #     if accepted
    #       puts "Dialog accepted"
    #     else
    #       puts "Dialog cancelled"
    #    end
    #
    def execute_modal(placement = PLACEMENT_CURSOR, &block)
      @__FTNonModal_block = block
      execute(placement)
    end
 
    #
    # Creates and shows the dialog, and registers the associated block to be
    # called when the dialog is closed. The block is passed a boolean argument
    # which is true if and only if the dialog was accepted.
    #
    # For example:
    #
    #   dialogBox.execute_nonmodal do |accepted|
    #     if accepted
    #       puts "Dialog accepted"
    #     else
    #       puts "Dialog cancelled"
    #    end
    #
    def execute_nonmodal(placement = PLACEMENT_CURSOR, &block)
      @__FTNonModal_block = block
      create
      show placement
    end

    def onCmdCancel(*args) # :nodoc:
      on_nonmodal_close(false)
    end

    def onCmdAccept(*args) # :nodoc:
      on_nonmodal_close(true)
    end

    # Called when dialog is closed, with _accepted_ equal to +true+ if and
    # only if the user accepted the dialog.
    def on_nonmodal_close(accepted)
      @__FTNonModal_block[accepted]

      ##return 0 -- why isn't this enough to close window?
      ## oh well, let's imitate FXTopWindow:
      getApp().stopModal(self, accepted ? 1 : 0)
      hide()
    end
  end
end
