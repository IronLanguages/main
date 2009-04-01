module Fox
  #
  # Button with an arrow; the arrow can point in any direction.
  # When clicked, the arrow button sends a SEL_COMMAND to its target.
  # When ARROW_REPEAT is passed, the arrow button sends a SEL_COMMAND
  # repeatedly while the button is pressed.
  # The option ARROW_AUTO together with ARROW_REPEAT makes the arrow
  # button work in repeat mode simply by hovering the cursor over it.
  #
  # === Events
  #
  # The following messages are sent by FXArrowButton to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::
  #   sent when the button is clicked (or repeatedly while the button is held
  #   down, if the +ARROW_REPEAT+ option is in effect).
  #
  # === Arrow style options
  #
  # +ARROW_NONE+::      no arrow
  # +ARROW_UP+::        arrow points up
  # +ARROW_DOWN+::      arrow points down
  # +ARROW_LEFT+::      arrow points left
  # +ARROW_RIGHT+::     arrow points right
  # +ARROW_AUTO+::	automatically fire when hovering mouse over button
  # +ARROW_REPEAT+::    button repeats if held down
  # +ARROW_AUTOGRAY+::  automatically gray out when not updated
  # +ARROW_AUTOHIDE+::  automatically hide when not updated
  # +ARROW_TOOLBAR+::   button is toolbar-style
  # +ARROW_NORMAL+::    same as <tt>FRAME_RAISED|FRAME_THICK|ARROW_UP</tt>
  #
  # === Message identifiers
  #
  # +ID_REPEAT+::
  #   message identifier used by the timer (internally) that handles
  #   the auto-repeat feature (activated by the +ARROW_REPEAT+ option).

  class FXArrowButton < FXFrame

    # Arrow button state, where +true+ means the button is down [Boolean]
    attr_accessor :state

    # Status line help text for this arrow button [String]
    attr_accessor :helpText

    # Tool tip message for this arrow button [String]
    attr_accessor :tipText

    # Arrow style flags (see above)
    attr_accessor :arrowStyle

    # Default arrow size, in pixels [Integer]
    attr_accessor :arrowSize

    # Justification mode
    attr_accessor :justify

    # Fill color for the arrow [FXColor]
    attr_accessor :arrowColor

    # Construct arrow button
    def initialize(parent, target=nil, selector=0, opts=ARROW_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theArrowButton
    end
  end
end
