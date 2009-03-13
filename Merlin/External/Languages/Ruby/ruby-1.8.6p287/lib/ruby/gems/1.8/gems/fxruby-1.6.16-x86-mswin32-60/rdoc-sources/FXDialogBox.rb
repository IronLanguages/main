module Fox
  #
  # Dialog box window.
  #
  # When a dialog box receives a +SEL_COMMAND+ message with identifier
  # +ID_CANCEL+ or +ID_ACCEPT+, the dialog box breaks out of the modal
  # loop and returns a completion code of either 0 or 1, respectively.
  #
  # To close a dialog box when it's not running modally, simply call
  # FXDialogBox#hide (or send it the +ID_HIDE+ command message).
  #
  # === Message identifiers
  #
  # +ID_CANCEL+::	Close the dialog, cancel the entry
  # +ID_ACCEPT+::	Close the dialog, accept the entry
  #
  class FXDialogBox < FXTopWindow
    #
    # Construct an empty dialog box.
    # If the _owner_ is an FXWindow instance, the dialog will always float over that window.
    # If the _owner_ is an FXApp instance, it will be a free-floating dialog.
    #
    def initialize(owner, title, opts=DECOR_TITLE|DECOR_BORDER, x=0, y=0, width=0, height=0, padLeft=10, padRight=10, padTop=10, padBottom=10, hSpacing=4, vSpacing=4) # :yields: theDialogBox
    end

    #
    # Run a modal invocation of the dialog, with specified initial _placement_.
    #
    def execute(placement=PLACEMENT_CURSOR); end
  end
end

