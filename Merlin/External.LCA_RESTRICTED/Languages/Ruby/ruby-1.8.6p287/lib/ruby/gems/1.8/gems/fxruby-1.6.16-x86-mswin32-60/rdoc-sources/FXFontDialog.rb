module Fox
  #
  # Font selection dialog
  #
  class FXFontDialog < FXDialogBox

    # Current font selection [FXFontDesc]
    attr_accessor :fontSelection

    # Return an initialized FXFontDialog instance.
    def initialize(owner, name, opts=0, x=0, y=0, width=600, height=380); end
  end
end

