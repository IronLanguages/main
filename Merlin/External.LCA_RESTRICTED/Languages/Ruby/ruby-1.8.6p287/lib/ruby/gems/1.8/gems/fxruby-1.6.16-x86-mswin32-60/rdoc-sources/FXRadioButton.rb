module Fox
  #
  # A radio button is a tri-state button.  Normally, it is either
  # +TRUE+ or +FALSE+; a third state +MAYBE+ may be set to indicate that no selection
  # has been made yet by the user, or that the state is ambiguous.
  # When pressed, the radio button sets its state to +TRUE+ and sends a +SEL_COMMAND+
  # message to its target, with the message data set to the state of the radio button.
  # A group of radio buttons can be made mutually exclusive by linking them
  # to a common data target (i.e. an instance of FXDataTarget).
  # Alternatively, an application can implement a common +SEL_UPDATE+ handler to
  # check and uncheck radio buttons as appropriate.
  #
  # === Events
  #
  # The following messages are sent by FXRadioButton to its target:
  #
  # +SEL_COMMAND+::		sent when the radio button is pressed.
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  # === RadioButton flags
  #
  # +RADIOBUTTON_AUTOGRAY+::	Automatically gray out when not updated
  # +RADIOBUTTON_AUTOHIDE+::	Automatically hide when not updated
  # +RADIOBUTTON_NORMAL+::	<tt>JUSTIFY_NORMAL|ICON_BEFORE_TEXT</tt>
  #
  class FXRadioButton < FXLabel

    # Radio button state, one of +TRUE+, +FALSE+ or +MAYBE+ [Integer]
    attr_accessor :checkState
    
    # Radio button style [Integer]
    attr_accessor :radioButtonStyle
    
    # Radio ball color [FXColor]
    attr_accessor :radioColor
    
    # Radio disk color [FXColor]
    attr_accessor :diskColor

    #
    # Construct new radio button
    #
    def initialize(parent, text, target=nil, selector=0, opts=RADIOBUTTON_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_PAD, padRight=DEFAULT_PAD, padTop=DEFAULT_PAD, padBottom=DEFAULT_PAD) # :yields: theRadioButton
    end

    #
    # Return +true+ if the radio button state is +TRUE+
    #
    def checked?
      self.checkState == Fox::TRUE
    end
    
    #
    # Return +true+ if the radio button state is +FALSE+
    #
    def unchecked?
      self.checkState == Fox::FALSE
    end

    #
    # Return +true+ if the radio button state is +MAYBE+
    #
    def maybe?
      self.checkState == Fox::MAYBE
    end
  end
end
