module Fox
  #
  # The status line normally shows its permanent or "normal" message; when
  # moving the mouse over a widget which provides status line help, the status line
  # temporarily replaces its normal message with the help information; the status
  # line obtains this help message by sending the widget a +ID_QUERY_HELP+ message
  # with type +SEL_UPDATE+.
  # If this query does not result in a new status string, the target of
  # the status line is tried via an ordinary +SEL_UPDATE+ message.
  # If _none_ of the above work, the status line will display the normal text
  # (i.e. the string set via the #normalText= accessor method).
  # If the message contains a newline character, then the part before the newline
  # will be displayed in the highlight color, while the part after the newline
  # will be shown using the normal text color.
  #
  # === Events
  #
  # The following messages are sent by FXStatusLine to its target:
  #
  # +SEL_UPDATE+::
  #   Sent when the widget currently under the mouse cursor doesn't respond
  #   to a +SEL_UPDATE+ message with identifier +ID_QUERY_HELP+, as described
  #   above.
  #
  class FXStatusLine < FXFrame

    # Temporary status message [String]
    attr_accessor :text
    
    # Permanent status message [String]
    attr_accessor :normalText
    
    # Text font [FXFont]
    attr_accessor :font
    
    # Text color [FXColor]
    attr_accessor :textColor
    
    # Highlight text color [FXColor]
    attr_accessor :textHighlightColor

    #
    # Return an initialized FXStatusLine instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this shutter [FXComposite]
    # +target+::	the message target, if any, for this shutter [FXObject]
    # +selector+::	the message identifier for this shutter [Integer]
    #
    def initialize(p, target=nil, selector=0) # :yields: theStatusLine
    end
    
    # Returns the temporary status message (i.e. same as _text_)
    def to_s
      text
    end
  end
end

