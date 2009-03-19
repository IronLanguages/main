module Fox
  #
  # A debug target prints out every message it receives.
  # To use it, simply make the FXDebugTarget a target of the widget
  # whose messages you want to see.
  #
  class FXDebugTarget < FXObject
    #
    # Returns an array of strings containing the names of the message types.
    # So, for example,
    #
    #	puts FXDebugTarget.messageTypeName[SEL_COMMAND]
    #
    # should print the text "SEL_COMMAND".
    #
    def FXDebugTarget.messageTypeName ; end
    
    #
    # Construct a debug target.
    #
    def initialize # :yields: theDebugTarget
    end
  end
end

