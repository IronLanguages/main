module Fox
  #
  # Printer selection dialog
  #
  # == Message identifiers
  #
  # +ID_TO_PRINTER+::		x
  # +ID_TO_FILE+::		x
  # +ID_PRINTER_NAME+::		x
  # +ID_FILE_NAME+::		x
  # +ID_LANDSCAPE+::		x
  # +ID_PORTRAIT+::		x
  # +ID_MEDIA+::		x
  # +ID_COLLATE_NORMAL+::	x
  # +ID_COLLATE_REVERSED+::	x
  # +ID_PAGES_ALL+::		x
  # +ID_PAGES_EVEN+::		x
  # +ID_PAGES_ODD+::		x
  # +ID_PAGES_RANGE+::		x
  # +ID_PAGES_FIRST+::		x
  # +ID_PAGES_LAST+::		x
  # +ID_BROWSE_FILE+::		x
  # +ID_PROPERTIES+::		x
  # +ID_COLOR_PRINTER+::	x
  # +ID_GRAY_PRINTER+::		x
  # +ID_NUM_COPIES+::		x
  #
  class FXPrintDialog < FXDialogBox
    # Printer information [FXPrinter]
    attr_accessor :printer

    # Construct print dialog
    def initialize(owner, name, opts=0, x=0, y=0, width=0, height=0) # :yields: thePrintDialog
    end
  end
end
