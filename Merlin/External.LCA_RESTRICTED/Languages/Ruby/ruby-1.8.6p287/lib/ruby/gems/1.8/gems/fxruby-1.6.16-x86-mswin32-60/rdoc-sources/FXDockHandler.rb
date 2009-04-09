module Fox
  #
  # The dock handler exists as a common base class for tool bar grip
  # and dock title.  
  #
  # === Events
  #
  # The following messages are sent by FXDockHandler to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  #
  class FXDockHandler < FXFrame
    # Status line help text [String]
    attr_accessor :helpText
    
    # Tool tip text [String]
    attr_accessor :tipText
    
    #
    # Return an initialized FXDockHandler instance.
    #
    def initialize(p, tgt, sel, opts, x, y, w, h, pl, pr, pt, pb) # :yields: aDockHandler
    end
  end
end
