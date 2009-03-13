module Fox
  #
  # A button provides a push button, with optional icon and/or text label.
  # When pressed, the button widget sends a <tt>SEL_COMMAND</tt> to its target.
  # Passing the <tt>BUTTON_TOOLBAR</tt> style option gives buttons a "flat" look, and
  # causes the edge of the button to be raised when the cursor moves over it.
  # Passing <tt>BUTTON_DEFAULT</tt> allows the button to become the default button in
  # a dialog, when the focus moves to it.  The default widget in a dialog
  # is the widget which will accept the _Return_ key when it is pressed.
  # The <tt>BUTTON_INITIAL</tt> flag makes the button the default widget when the
  # focus moves to a widget which can not itself be a default widget.
  # There should be only a single button in the dialog which is the
  # initial default; typically this is the _OK_ or _Close_ button.
  # The option <tt>BUTTON_AUTOGRAY</tt> (<tt>BUTTON_AUTOHIDE</tt>) causes the button to be grayed
  # out (hidden) if its handler does not respond to the <tt>SEL_UPDATE</tt> message.
  # This is useful when messages are delegated, for example when using a
  # multiple document interface, where the ultimate destination of a message
  # can be changed.
  #
  # === Events
  #
  # The following messages are sent by FXButton to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::		sent when the button is clicked.
  #
  # === Button state bits
  #
  # +STATE_UP+::        Button is up
  # +STATE_DOWN+::      Button is down
  # +STATE_ENGAGED+::   Button is engaged
  # +STATE_UNCHECKED+:: Same as +STATE_UP+ (used for check buttons or radio buttons)
  # +STATE_CHECKED+::   Same as +STATE_ENGAGED+ (used for check buttons or radio buttons)
  #
  # === Button flags
  #
  # +BUTTON_AUTOGRAY+:: Automatically gray out when not updated
  # +BUTTON_AUTOHIDE+:: Automatically hide button when not updated
  # +BUTTON_TOOLBAR+::  Toolbar style button [flat look]
  # +BUTTON_DEFAULT+::  May become default button when receiving focus
  # +BUTTON_INITIAL+::  This button is the initial default button
  # +BUTTON_NORMAL+::   Default button flags (<tt>FRAME_RAISED|FRAME_THICK|JUSTIFY_NORMAL|ICON_BEFORE_TEXT</tt>)
  #
  class FXButton < FXLabel
  
    # Button state (one of +STATE_UP+, +STATE_DOWN+, etc.) [Integer]
    attr_accessor :state
    
    # Button style flags (some combination of +BUTTON_AUTOGRAY+, +BUTTON_AUTOHIDE+, etc.) [Integer]
    attr_accessor :buttonStyle

    # Construct button with specified _text_ and _icon_.
    def initialize(parent, text, icon=nil, target=nil, selector=0, opts=BUTTON_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theButton
    end
  end
end
