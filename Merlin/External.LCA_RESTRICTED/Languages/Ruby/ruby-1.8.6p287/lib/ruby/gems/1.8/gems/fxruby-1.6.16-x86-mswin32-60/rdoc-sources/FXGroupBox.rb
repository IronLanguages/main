module Fox
  # An FXGroupBox widget provides a nice raised or sunken border
  # around a group of widgets, providing a visual delineation.
  # Typically, a title is placed over the border to provide some
  # clarification.
  #
  # === Group box options
  #
  # +GROUPBOX_TITLE_LEFT+::	Title is left-justified
  # +GROUPBOX_TITLE_CENTER+::	Title is centered
  # +GROUPBOX_TITLE_RIGHT+::	Title is right-justified
  # +GROUPBOX_NORMAL+::		same as <tt>GROUPBOX_TITLE_LEFT</tt>

  class FXGroupBox < FXPacker

    # Group box title text [String]
    attr_accessor :text
    
    # Group box style [Integer]
    attr_accessor :groupBoxStyle
    
    # Title font [FXFont]
    attr_accessor :font
    
    # Title text color [FXColor]
    attr_accessor :textColor

    # Construct group box layout manager
    def initialize(parent, text, opts=GROUPBOX_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theGroupBox
    end
    
    # Return the group box's title text
    def to_s; text; end
  end
end

