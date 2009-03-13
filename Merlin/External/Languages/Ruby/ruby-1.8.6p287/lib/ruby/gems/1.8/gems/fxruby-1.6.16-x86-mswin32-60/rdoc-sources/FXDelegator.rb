module Fox
  #
  # A delegator forwards messages to a delegate object.
  # Delegators are used when you need to multiplex messages
  # toward any number of target objects.  
  # For example, many controls may be connected to FXDelegator,
  # instead of directly to the document object.  Changing the
  # delegate in FXDelegator will then reconnect the controls with their
  # new target.
  #
  class FXDelegator < FXObject

    # The object to which all messages are delegated [FXObject]
    attr_accessor	:delegate

    #
    # Construct a new delegator
    #
    def initialize(delegate=nil) # :yields: theDelegate
    end
  end
end

