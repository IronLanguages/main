module Fox
  #
  # Text search dialog
  #
  class FXSearchDialog < FXReplaceDialog
    #
    # Return an initialized FXSearchDialog instance.
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for this dialog box [FXWindow]
    # +caption+::	the caption (title) string for this dialog box [String]
    # +ic+::		the icon [FXIcon]
    # +opts+::		the options [Integer]
    # +x+::		initial x-position [Integer]
    # +y+::		initial y-position [Integer]
    # +width+::		initial width [Integer]
    # +height+::		initial height [Integer]
    #
    def initialize(owner, caption, ic=nil, opts=0, x=0, y=0, width=0, height=0) # :yields: theSearchDialog
    end
  end
end

