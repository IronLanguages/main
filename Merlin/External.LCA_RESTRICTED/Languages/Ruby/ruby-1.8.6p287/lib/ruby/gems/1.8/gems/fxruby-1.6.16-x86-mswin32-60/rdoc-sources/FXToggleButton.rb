module Fox
  #
  # The toggle button provides a two-state button, which toggles between the
  # on and the off state each time it is pressed.  For each state, the toggle
  # button has a unique icon and text label.
  #
  # === Events
  #
  # The following messages are sent by FXToggleButton to its target:
  #
  # +SEL_COMMAND+::		sent when the toggle button is pressed.
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  # === Toggle button flags
  #
  # +TOGGLEBUTTON_AUTOGRAY+::	Automatically gray out when not updated
  # +TOGGLEBUTTON_AUTOHIDE+::	Automatically hide toggle button when not updated
  # +TOGGLEBUTTON_TOOLBAR+::	Toolbar style toggle button [flat look]
  # +TOGGLEBUTTON_KEEPSTATE+::  Draw button according to state
  # +TOGGLEBUTTON_NORMAL+::	<tt>FRAME_RAISED|FRAME_THICK|JUSTIFY_NORMAL|ICON_BEFORE_TEXT</tt>
  #
  class FXToggleButton < FXLabel

    # Alternate text, shown when toggled [String]
    attr_accessor	:altText

    # Alternate icon, shown when toggled [FXIcon]
    attr_accessor	:altIcon

    # Toggled state [+true+ or +false+]
    attr_accessor	:state

    # Alternate status line help text, shown when toggled [String]
    attr_accessor	:altHelpText

    # Alternate tool tip message, shown when toggled [String]
    attr_accessor	:altTipText

    # Toggle button style [Integer]
    attr_accessor	:toggleStyle

    #
    # Return an initialized FXToggleButton instance.
    #
    # ==== Parameters:
    #
    # +p+::		the parent window for this toggle button [FXComposite]
    # <tt>text1</tt>::	the text for this toggle button's first state [String]
    # <tt>text2</tt>::	the text for this toggle button's second state [String]
    # <tt>icon1</tt>::	the icon, if any, for this toggle button's first state [FXIcon]
    # <tt>icon2</tt>::	the icon, if any, for this toggle button's second state [FXIcon]
    # +target+::		the message target, if any, for this toggle button [FXObject]
    # +selector+::		the message identifier for this toggle button [Integer]
    # +opts+::		toggle button options [Integer]
    # +x+::		initial x-position [Integer]
    # +y+::		initial y-position [Integer]
    # +width+::		initial width [Integer]
    # +height+::		initial height [Integer]
    # +padLeft+::		internal padding on the left side, in pixels [Integer]
    # +padRight+::		internal padding on the right side, in pixels [Integer]
    # +padTop+::		internal padding on the top side, in pixels [Integer]
    # +padBottom+::		internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, text1, text2, icon1=nil, icon2=nil, target=nil, selector=0, opts=TOGGLEBUTTON_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theToggleButton
    end
  end
end
