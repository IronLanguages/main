module Fox
  #
  # A data target allows a valuator widget such as an FXSlider or FXTextField
  # to be directly connected with a variable in the program.
  # Whenever the valuator control changes, the variable connected through
  # the data target is automatically updated; conversely, whenever the program
  # changes a variable, all the connected valuator widgets will be updated 
  # to reflect this new value on the display. For example:
  #
  #     data = FXDataTarget.new("Some Text")
  #     textfield = FXTextField.new(p, 12, data, FXDataTarget::ID_VALUE)
  #
  # Data targets also allow connecting other kinds of widgets (like FXRadioButton and
  # FXMenuCommand) to a variable. In this case, the new value of the connected variable
  # is computed by subtracting <code>FXDataTarget::ID_OPTION</code> from the message
  # identifier. For example, to tie a group of radio buttons to a single data target's
  # value (so that the buttons are mutually exclusive), use code like this:
  #
  #     data = FXDataTarget.new(0)
  #     radio1 = FXRadioButton.new(p, "1st choice", data, FXDataTarget::ID_OPTION)
  #     radio2 = FXRadioButton.new(p, "2nd choice", data, FXDataTarget::ID_OPTION + 1)
  #     radio3 = FXRadioButton.new(p, "3rd choice", data, FXDataTarget::ID_OPTION + 2)
  #
  # Note that if you'd like the data target to "forward" its +SEL_COMMAND+ or
  # +SEL_CHANGED+ to some other target object after it has updated the data
  # target value, you can do that just as you would for any other widget.
  # For example, continuing the previous code snippet:
  #
  #     data.connect(SEL_COMMAND) {
  #       puts "The new data target value is #{data.value}"
  #     }
  #
  # === Events
  #
  # The following messages are sent by FXDataTarget to its target:
  #
  # +SEL_COMMAND+::   Sent after the data target processes a +SEL_COMMAND+ message itself
  # +SEL_CHANGED+::   Sent after the data target processes a +SEL_CHANGED+ message itself
  #
  # === Message identifiers
  #
  # +ID_VALUE+::    Causes the FXDataTarget to ask sender for value
  # +ID_OPTION+::   +ID_OPTION++_i_ will set the value to _i_, where -10000 <= _i_ <= 10000
  #
  class FXDataTarget < FXObject
  
    # The message target object for this data target [FXObject]
    attr_accessor :target

    # The message identifier for this data target [Integer]
    attr_accessor :selector

    # The data target's current value [Object]
    attr_accessor :value

    #
    # Return a new FXDataTarget instance, initialized with the specified _value_.
    # If the optional message target object and message identifier (_tgt_ and _sel_)
    # are specified, the data target will forward the +SEL_COMMAND+ or +SEL_COMMAND+
    # to this other target.
    #
    def initialize(value=nil, target=nil, selector=0) # :yields: theDataTarget
    end
  end
end

