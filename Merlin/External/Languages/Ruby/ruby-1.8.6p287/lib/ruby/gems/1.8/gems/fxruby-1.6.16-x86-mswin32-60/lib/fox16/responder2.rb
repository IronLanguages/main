require 'fox16/responder'

module Fox

  #
  # FXPseudoTarget instances act as the message target for any widgets that
  # elect to use the #connect method to map certain message types to
  # blocks.
  #
  class FXPseudoTarget < FXObject

    include Responder

    @@targets_of_pending_timers = {}
    @@targets_of_pending_chores = {}
    @@targets_of_pending_signals = {}
    @@targets_of_pending_inputs = {}

    #
    # Returns an initialized FXPseudoTarget object.
    #
    def initialize
      super
      @context = {}
    end

    #
    # Store an association between a message of type
    # _message_type_ with a callable object.
    #
    def pconnect(message_type, callable_object, params={})
      @context[message_type] = { :callable => callable_object, :params => params }
      FXMAPTYPE(message_type, :onHandleMsg)
      case message_type
      when SEL_TIMEOUT
        @@targets_of_pending_timers[self] = self
      when SEL_CHORE
        @@targets_of_pending_chores[self] = self
      when SEL_SIGNAL
        @@targets_of_pending_signals[self] = self
      when SEL_IO_READ, SEL_IO_WRITE, SEL_IO_EXCEPT
        @@targets_of_pending_inputs[self] = self
      end
    end

    #
    # Handle a message from _sender_, with selector _sel_ and
    # message data _ptr_.
    #
    def onHandleMsg(sender, sel, ptr)
      message_type = Fox.FXSELTYPE(sel)
      ctx = @context[message_type]
      callable_object = ctx[:callable]
      params = ctx[:params]
      result = callable_object.call(sender, sel, ptr)
      case message_type
      when SEL_TIMEOUT
        if params[:repeat]
          FXApp.instance.addTimeout(params[:delay], callable_object, params)
        else
          @@targets_of_pending_timers.delete(self)
        end
      when SEL_CHORE
        if params[:repeat]
          FXApp.instance.addChore(callable_object, params)
        else
          @@targets_of_pending_chores.delete(self)
        end
      end
      result
    end
    
  end # class FXPseudoTarget
  
end # module Fox

#
# The Responder2 module provides the #connect method,
# which is mixed-in to all classes that have a message
# target (i.e. Fox::FXDataTarget, Fox::FXRecentFiles
# and Fox::FXWindow).
# 
module Responder2
  #
  # Assign a "handler" for all FOX messages of type _messageType_
  # sent from this widget. When called with only one argument,
  # a block is expected, e.g.
  #
  #     aButton.connect(SEL_COMMAND) { |sender, selector, data|
  #       ... code to handle this event ...
  #     }
  #
  # The arguments passed into the block are the _sender_ of the
  # message (i.e. the widget), the _selector_ for the message, and
  # any message-specific _data_.
  #
  # When #connect is called with two arguments, the second argument
  # should be some callable object such as a Method or Proc instance, e.g.
  #
  #     aButton.connect(SEL_COMMAND, method(:onCommand))
  #
  # As with the one-argument form of #connect, the callable object
  # will be "called" with three arguments (the sender, selector and
  # message data).
  #
  def connect(message_type, callable_object=nil, &block)
    unless instance_variables.include?('@pseudoTarget')
      @pseudoTarget = Fox::FXPseudoTarget.new
      self.target = @pseudoTarget
    end
    @pseudoTarget.pconnect(message_type, callable_object ? callable_object : block)
  end
  
end

module Fox
  class FXDataTarget
    include Responder2
  end
  class FXRecentFiles
    include Responder2
  end
  class FXWindow
    include Responder2
  end
end

require 'fox16/timeout'
require 'fox16/chore'
require 'fox16/signal'
require 'fox16/input'
