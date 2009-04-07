module Fox
  #
  # Font selection widget
  #
  # === Message identifiers
  #
  # +ID_FAMILY+::	x
  # +ID_WEIGHT+::	x
  # +ID_STYLE+::	x
  # +ID_STYLE_TEXT+::	x
  # +ID_SIZE+::		x
  # +ID_SIZE_TEXT+::	x
  # +ID_CHARSET+::	x
  # +ID_SETWIDTH+::	x
  # +ID_PITCH+::	x
  # +ID_SCALABLE+::	x
  # +ID_ALLFONTS+::	x
  #
  class FXFontSelector < FXPacker

    # The "Accept" button [FXButton]
    attr_reader :acceptButton

    # The "Cancel" button [FXButton]
    attr_reader :cancelButton

    # Font selection [FXFontDesc]
    attr_accessor :fontSelection

    #
    # Return an initialized FXFontSelector instance.
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theFontSelector
    end
  end
end

