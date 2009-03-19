module Fox
  #
  # A tab item is placed in a tab bar or tab book.
  # When selected, the tab item sends a message to its
  # parent, and causes itself to become the active tab,
  # and raised slightly above the other tabs.
  # In the tab book, activating a tab item also causes
  # the corresponding panel to be raised to the top.
  #
  # === Events
  #
  # The following messages are sent by FXTabItem to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  #
  # === Tab item orientations (which affect border)
  #
  # +TAB_TOP+::			Top side tabs
  # +TAB_LEFT+::		Left side tabs
  # +TAB_RIGHT+::		Right side tabs
  # +TAB_BOTTOM+::		Bottom side tabs
  # +TAB_TOP_NORMAL+::		same as <tt>JUSTIFY_NORMAL|ICON_BEFORE_TEXT|TAB_TOP|FRAME_RAISED|FRAME_THICK</tt>
  # +TAB_BOTTOM_NORMAL+::	same as <tt>JUSTIFY_NORMAL|ICON_BEFORE_TEXT|TAB_BOTTOM|FRAME_RAISED|FRAME_THICK</tt>
  # +TAB_LEFT_NORMAL+::		same as <tt>JUSTIFY_LEFT|JUSTIFY_CENTER_Y|ICON_BEFORE_TEXT|TAB_LEFT|FRAME_RAISED|FRAME_THICK</tt>
  # +TAB_RIGHT_NORMAL+::	same as <tt>JUSTIFY_LEFT|JUSTIFY_CENTER_Y|ICON_BEFORE_TEXT|TAB_RIGHT|FRAME_RAISED|FRAME_THICK</tt>
  #
  class FXTabItem < FXLabel

    #
    # Current tab item orientation, one of +TAB_TOP+, +TAB_LEFT+, +TAB_RIGHT+
    # or +TAB_BOTTOM+ [Integer].
    #
    attr_accessor :tabOrientation

    #
    # Return an initialized FXTabItem instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent tab book (or tab bar) for this tab item [FXTabBar]
    # +text+::	the text for this tab item [String]
    # +ic+::	the icon for this tab item, if any [FXIcon]
    # +opts+::	tab item options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, text, ic=nil, opts=TAB_TOP_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theTabItem
    end
  end
end

