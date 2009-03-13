module Fox
  #
  # The FXMenuCommand widget is used to invoke a command in the
  # application from a menu.  Menu commands may reflect
  # the state of the application by graying out or becoming hidden.
  # When activated, a menu command sends a +SEL_COMMAND+ to its target.
  #
  # === Events
  #
  # The following messages are sent by FXMenuCommand to its target:
  #
  # +SEL_COMMAND+::		sent when the command is activated
  #
  class FXMenuCommand < FXMenuCaption

    # Accelerator text [String]
    attr_accessor :accelText

    #
    # Construct a menu command
    #
    def initialize(p, text, ic=nil, target=nil, selector=0, opts=0) # :yields: theMenuCommand
    end
  end
end

