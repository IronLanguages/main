module Fox
  #
  # A label widget can be used to place a text and/or icon for
  # explanation purposes. The text label may have an optional tooltip
  # and/or help string.  Icon and label are placed relative to the widget
  # using the justfication options, and relative to each other as determined
  # by the icon relationship options.  A large number of arrangements is
  # possible.
  #
  # === Justification modes
  #
  # +JUSTIFY_NORMAL+::      Default justification is centered text
  # +JUSTIFY_CENTER_X+::    Text is centered horizontally
  # +JUSTIFY_LEFT+::        Text is left-justified
  # +JUSTIFY_RIGHT+::       Text is right-justified
  # +JUSTIFY_HZ_APART+::    Combination of +JUSTIFY_LEFT+ & +JUSTIFY_RIGHT+
  # +JUSTIFY_CENTER_Y+::    Text is centered vertically
  # +JUSTIFY_TOP+::         Text is aligned with label top
  # +JUSTIFY_BOTTOM+::      Text is aligned with label bottom
  # +JUSTIFY_VT_APART+::    Combination of +JUSTIFY_TOP+ & +JUSTIFY_BOTTOM+
  #
  # === Relationship options for icon-labels
  #
  # +ICON_UNDER_TEXT+::    Icon appears under text
  # +ICON_AFTER_TEXT+::    Icon appears after text (to its right)
  # +ICON_BEFORE_TEXT+::   Icon appears before text (to its left)
  # +ICON_ABOVE_TEXT+::    Icon appears above text
  # +ICON_BELOW_TEXT+::    Icon appears below text
  # +TEXT_OVER_ICON+::     Same as +ICON_UNDER_TEXT+
  # +TEXT_AFTER_ICON+::    Same as +ICON_BEFORE_TEXT+
  # +TEXT_BEFORE_ICON+::   Same as +ICON_AFTER_TEXT+
  # +TEXT_ABOVE_ICON+::    Same as +ICON_BELOW_TEXT+
  # +TEXT_BELOW_ICON+::    Same as +ICON_ABOVE_TEXT+
  #
  # === Normal way to show label
  #
  # +LABEL_NORMAL+::        Same as <tt>JUSTIFY_NORMAL|ICON_BEFORE_TEXT</tt>
  #
  class FXLabel < FXFrame

    # The text for this label [String]
    attr_accessor :text
    
    # The icon for this label [FXIcon]
    attr_accessor :icon
    
    # The text font [FXFont]
    attr_accessor :font
    
    # The text color [FXColor]
    attr_accessor :textColor
    
    # Text justification mode [Integer]
    attr_accessor :justify
    
    # Icon position [Integer]
    attr_accessor :iconPosition
    
    # Status line help text [String]
    attr_accessor :helpText
    
    # Tool tip message [String]
    attr_accessor :tipText

    # Construct label with given text and icon
    def initialize(parent, text, icon=nil, opts=LABEL_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theLabel
    end
    
    # Return the label's text
    def to_s; text; end
  end
end
