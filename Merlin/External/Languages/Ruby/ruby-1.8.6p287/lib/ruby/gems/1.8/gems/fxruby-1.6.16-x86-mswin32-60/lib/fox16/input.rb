module Fox
  
  class FXApp

    alias addInputOrig addInput # :nodoc:

    #
    # Add a file descriptor _fileDesc_ to be watched for activity as determined
    # by _mode_, where _mode_ is a bitwise OR (+INPUT_READ+, +INPUT_WRITE+, +INPUT_EXCEPT+).
    # A message of type +SEL_IO_READ+, +SEL_IO_WRITE+, or +SEL_IO_EXCEPT+ will be sent 
    # to the target when the specified activity is detected on the file descriptor.
    #
    # There are several forms for #addInput; the original form (from FOX)
    # takes four arguments:
    #
    #   anApp.addInput(fileDesc, mode, anObject, aMessageId)
    #
    # A second form takes three arguments:
    #
    #   anApp.addInput(fileDesc, mode, aMethod)
    #
    # For this form, _aMethod_ should have the standard argument list
    # for a FOX message handler. That is, the method should take three
    # arguments, for the message _sender_ (an FXObject), the message _selector_,
    # and the message _data_ (if any).
    #
    # The last form of #addInput takes a block:
    #
    #   anApp.addInput(fileDesc, mode) { |sender, sel, data|
    #     ... handle the I/O event ...
    #   }
    #

    def addInput(fd, mode, *args, &block)
      params = {}
      params = args.pop if args.last.is_a? Hash
      tgt, sel = nil, 0
      if args.length > 0
        if args[0].respond_to? :call
          tgt = FXPseudoTarget.new
          tgt.pconnect(SEL_IO_READ, args[0], params)
          tgt.pconnect(SEL_IO_WRITE, args[0], params)
          tgt.pconnect(SEL_IO_EXCEPT, args[0], params)
        else # it's some other kind of object
          tgt = args[0]
          sel = args[1]
        end
      else
        tgt = FXPseudoTarget.new
        tgt.pconnect(SEL_IO_READ, block, params)
        tgt.pconnect(SEL_IO_WRITE, block, params)
        tgt.pconnect(SEL_IO_EXCEPT, block, params)
      end
      addInputOrig(fd, mode, tgt, sel)
    end
    
  end # class FXApp
  
end # module Fox
