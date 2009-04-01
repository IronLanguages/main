module Fox
  #
  # The FXMenuRadio widget is used to change a state in the application from a menu.
  # Menu radio commands may reflect the state of the application by graying out, becoming
  # hidden, or by displaying a bullet.
  # When activated, a menu radio sends a +SEL_COMMAND+ to its target;
  # the message data contains the new state.
  # A collection of menu radio widgets which belong to each other
  # is supposed to be updated by a common +SEL_UPDATE+ handler to
  # properly maintain the state between them.
  #
  # === Events
  #
  # The following messages are sent by FXMenuRadio to its target:
  #
  # +SEL_COMMAND+::		sent when the command is activated
  #
  class FXMenuRadio < FXMenuCommand

    # Radio button state, one of +TRUE+, +FALSE+ or +MAYBE+
    attr_accessor :check
    
    # Radio background color [FXColor]
    attr_accessor :radioColor

    #
    # Construct a menu radio
    #
    def initialize(p, text, target=nil, selector=0, opts=0) # :yields: theMenuRadio
    end
  end
end

