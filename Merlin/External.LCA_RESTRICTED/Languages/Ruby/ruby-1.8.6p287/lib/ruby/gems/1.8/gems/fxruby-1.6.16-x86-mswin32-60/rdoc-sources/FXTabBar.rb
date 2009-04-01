module Fox
  #
  # The FXTabBar layout manager arranges tab items side by side,
  # and raises the active tab item above the neighboring tab items.
  # In a the horizontal arrangement, the tab bar can have the tab
  # items on the top or on the bottom.  In the vertical arrangement,
  # the tabs can be on the left or on the right.
  # When one of the tab items is pressed, the tab bar's #setCurrent()
  # method is called with _notify_ of +true+. This in turn causes the tab bar
  # to send a +SEL_COMMAND+ message to its target.
  #
  # === Events
  #
  # The following messages are sent by FXTabBar to its target:
  #
  # +SEL_COMMAND+::
  #   sent whenever the current tab item changes;
  #   the message data is an integer indicating the new current tab item's index.
  #
  # === Tab book options
  #
  # +TABBOOK_TOPTABS+::		Tabs on top (default)
  # +TABBOOK_BOTTOMTABS+::	Tabs on bottom
  # +TABBOOK_SIDEWAYS+::	Tabs on left
  # +TABBOOK_LEFTTABS+::	Tabs on left
  # +TABBOOK_RIGHTTABS+::	Tabs on right
  # +TABBOOK_NORMAL+::		same as <tt>TABBOOK_TOPTABS</tt>
  #
  # === Message identifiers
  #
  # +ID_OPEN_ITEM+::	Sent from one of the FXTabItems
  # +ID_OPEN_FIRST+::	Switch to the first panel
  # +ID_OPEN_SECOND+::	x
  # +ID_OPEN_THIRD+::	x
  # +ID_OPEN_FOURTH+::	x
  # +ID_OPEN_FIFTH+::	x
  # +ID_OPEN_SIXTH+::	x
  # +ID_OPEN_SEVENTH+::	x
  # +ID_OPEN_EIGHTH+::	x
  # +ID_OPEN_NINETH+::	x
  # +ID_OPEN_TENTH+::	x
  # +ID_OPEN_LAST+::	x
  #
  class FXTabBar < FXPacker
    # Currently active tab item's index [Integer]
    attr_accessor :current
    
    # Tab bar style [Integer]
    attr_accessor :tabStyle

    #
    # Return an initialized FXTabBar instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tar bar [FXComposite]
    # +target+::	the message target, if any, for this tar bar [FXObject]
    # +selector+::	the message identifier for this tab bar [Integer]
    # +opts+::	tar bar options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=TABBOOK_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING) # :yields: theTabBar
    end

    #
    # Change currently active tab item; this raises the active tab item 
    # slightly above the neighboring tab items.
    # If _notify_ is +true+, a +SEL_COMMAND+ message is sent to the tab bar's message target
    #
    def setCurrent(index, notify=false); end
  end
end

