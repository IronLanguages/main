module Fox
  #
  # The FXMenuCheck widget is used to change a state in the application from a menu.
  # Menu checks may reflect the state of the application by graying out, becoming
  # hidden, or by a check mark.
  # When activated, a menu check sends a +SEL_COMMAND+ message to its target;
  # the message data contains the new state (one of +TRUE+, +FALSE+ or +MAYBE+).
  #
  # === Events
  #
  # The following messages are sent by FXMenuCheck to its target:
  #
  # +SEL_COMMAND+::		sent when the command is activated
  #
  class FXMenuCheck < FXMenuCommand

    # Check state, one of +TRUE+, +FALSE+ or +MAYBE+
    attr_accessor :check
    
    # Box background color [FXColor]
    attr_accessor :boxColor

    #
    # Construct a menu check
    #
    def initialize(p, text, target=nil, selector=0, opts=0) # :yields: theMenuCheck
    end
  end
end

