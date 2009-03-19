module Fox
  
  class FXApp

    alias addSignalOrig addSignal # :nodoc:

    #
    # Register a signal processing message to be sent to target object when 
    # the specified signal is raised.
    #
    # There are several forms for #addSignal; the original form (from FOX)
    # takes (up to) five arguments:
    #
    #   anApp.addSignal(aSignal, anObject, aMessageId, sendImmediately=false, flags=0)
    #
    # Here, _aSignal_ is a string indicating the operating system signal of interest
    # (such as "SIGINT").
    # The second and third arguments are the target object and message identifier for
    # the message to be sent when this signal is raised.
    # If _sendImmediately_ is +true+, the message will be sent to the target right away;
    # this should be used with extreme care as the application is interrupted
    # at an unknown point in its execution.
    # The _flags_ are to be set as per POSIX definitions.
    #
    # A second form of #addSignal takes a Method instance as its second argument:
    #
    #   anApp.addSignal(aSignal, aMethod, sendImmediately=false, flags=0)
    #
    # For this form, the method should have the standard argument list
    # for a FOX message handler. That is, the method should take three
    # arguments, for the message _sender_ (an FXObject), the message _selector_,
    # and the message _data_ (if any).
    #
    # The last form of #addSignal takes a block:
    #
    #   anApp.addSignal(aSignal, sendImmediately=false, flags=0) { |sender, sel, data|
    #     ... handle the signal ...
    #   }
    #

    def addSignal(sig, *args, &block)
      params = {}
      params = args.pop if args.last.is_a? Hash
      tgt, sel, immediate, flags = nil, 0, false, 0
      if args.length > 0
        if args[0].respond_to? :call
          tgt = FXPseudoTarget.new
          tgt.pconnect(SEL_SIGNAL, args[0], params)
          immediate = (args.length > 1) ? args[1] : false
          flags = (args.length > 2) ? args[2] : 0
        elsif (args[0].kind_of? TrueClass) || (args[0].kind_of? FalseClass)
          tgt = FXPseudoTarget.new
          tgt.pconnect(SEL_SIGNAL, block, params)
          immediate = args[0]
          flags = (args.length > 1) ? args[1] : 0
        else # it's some other kind of object
          tgt = args[0]
          sel = (args.length > 1) ? args[1] : 0
          immediate = (args.length > 2) ? args[2] : false
          flags = (args.length > 3) ? args[3] : 0
        end
      else
        tgt = FXPseudoTarget.new
        tgt.pconnect(SEL_SIGNAL, block, params)
      end
      addSignalOrig(sig, tgt, sel, immediate, flags)
    end
    
  end # class FXApp
  
end # module Fox
