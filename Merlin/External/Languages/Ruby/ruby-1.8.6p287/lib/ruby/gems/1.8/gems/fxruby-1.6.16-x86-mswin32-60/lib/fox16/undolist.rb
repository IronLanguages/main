#
# This is a "pure Ruby" implementation of the FXUndoList and
# FXCommand classes from the standard FOX distribution. Since those
# classes are independent of the rest of FOX this is a simpler (and probably
# more efficient) approach than trying to wrap the original C++ classes.
# 
# Notes (by Jeroen, lifted from FXUndoList.cpp):
# 
# * When a command is undone, it's moved to the redo list.
# * When a command is redone, it's moved back to the undo list.
# * Whenever adding a new command, the redo list is deleted.
# * At any time, you can trim down the undo list down to a given
#   maximum size or a given number of undo records. This should
#   keep the memory overhead within sensible bounds.
# * To keep track of when we get back to an "unmodified" state, a mark
#   can be set. The <em>mark</em> is basically a counter which is incremented
#   with every undo record added, and decremented when undoing a command.
#   When we get back to 0, we are back to the unmodified state.
# 
#   If, after setting the mark, we have called FXUndoList#undo, then
#   the mark can be reached by calling FXUndoList#redo.
# 
#   If the marked position is in the redo-list, then adding a new undo
#   record will cause the redo-list to be deleted, and the marked position
#   will become unreachable.
# 
#   The marked state may also become unreachable when the undo list is trimmed.
# 
# * You can call also kill the redo list without adding a new command
#   to the undo list, although this may cause the marked position to
#   become unreachable.
# * We measure the size of the undo-records in the undo-list; when the
#   records are moved to the redo-list, they usually contain different
#   information!

require 'fox16/responder'

module Fox

  #
  # The undo list manages a list of undoable (and redoable) commands for a FOX
  # application; it works hand-in-hand with subclasses of FXCommand and is
  # an application of the well-known <em>Command</em> pattern. Your application
  # code should implement any number of command classes and then add then to an
  # FXUndoList instance. For an example of how this works, see the textedit
  # example program from the FXRuby distribution.
  # 
  # == Class Constants
  # 
  # [FXUndoList::ID_UNDO]	Message identifier for the undo method.
  #				When a +SEL_COMMAND+ message with this identifier
  #				is sent to an undo list, it undoes the last command.
  #				FXUndoList also provides a +SEL_UPDATE+ handler for this
  #				identifier, that enables or disables the sender
  #				depending on whether it's possible to undo.
  #
  # [FXUndoList::ID\_UNDO\_ALL]	Message identifier for the "undo all" method. FXUndoList handles both
  #				the +SEL_COMMAND+ and +SEL_UPDATE+ messages for this message
  #				identifier.
  #
  # [FXUndoList::ID_REDO]	Message identifier for the redo method. When a +SEL_COMMAND+ message
  #				with this identifier is sent to an undo list, it redoes the last command.
  #				FXUndoList also provides a +SEL_UPDATE+ handler for this identifier,
  #				that enables or disables the sender depending on whether it's possible to
  #				redo.
  #
  # [FXUndoList::ID\_REDO\_ALL]	Message identifier for the "redo all" method. FXUndoList handles both
  #				the +SEL_COMMAND+ and +SEL_UPDATE+ messages for this message
  #				identifier.
  #
  # [FXUndoList::ID_CLEAR]	Message identifier for the "clear" method. FXUndoList handles both
  #				the +SEL_COMMAND+ and +SEL_UPDATE+ messages for this message
  #				identifier.
  #
  # [FXUndoList::ID_REVERT]	Message identifier for the "revert" method. FXUndoList handles both
  #				the +SEL_COMMAND+ and +SEL_UPDATE+ messages for this message
  #				identifier.
  #
  class FXUndoList < FXObject

    include Responder

    ID_CLEAR,
    ID_REVERT,
    ID_UNDO,
    ID_REDO,
    ID_UNDO_ALL,
    ID_REDO_ALL,
    ID_LAST = enum(0, 7)

    #
    # Returns an initialized FXUndoList instance.
    #
    def initialize
      # Be sure to call base class initialize
      super

      # Set up the message map for this instance
      FXMAPFUNC(SEL_COMMAND, ID_CLEAR,    "onCmdClear")
      FXMAPFUNC(SEL_UPDATE,  ID_CLEAR,    "onUpdClear")
      FXMAPFUNC(SEL_COMMAND, ID_REVERT,   "onCmdRevert")
      FXMAPFUNC(SEL_UPDATE,  ID_REVERT,   "onUpdRevert")
      FXMAPFUNC(SEL_COMMAND, ID_UNDO,     "onCmdUndo")
      FXMAPFUNC(SEL_UPDATE,  ID_UNDO,     "onUpdUndo")
      FXMAPFUNC(SEL_COMMAND, ID_REDO,     "onCmdRedo")
      FXMAPFUNC(SEL_UPDATE,  ID_REDO,     "onUpdRedo")
      FXMAPFUNC(SEL_COMMAND, ID_UNDO_ALL, "onCmdUndoAll")
      FXMAPFUNC(SEL_UPDATE,  ID_UNDO_ALL, "onUpdUndo")
      FXMAPFUNC(SEL_COMMAND, ID_REDO_ALL, "onCmdRedoAll")
      FXMAPFUNC(SEL_UPDATE,  ID_REDO_ALL, "onUpdRedo")

      # Instance variables
      @undolist = []
      @redolist = []
      @marker = nil
      @size = 0
    end

    #
    # Cut the redo list
    #
    def cut
      @redolist.clear
      unless @marker.nil?
        @marker = nil if @marker < 0
      end
    end

    #
    # Add new _command_ (an FXCommand instance) to the list.
    # If _doit_ is +true+, the command is also executed.
    #
    def add(command, doit=false)
      # Cut redo list
      cut

      # No command given?
      return true if command.nil?

      # Add it to the end of the undo list
      @undolist.push(command)

      # Execute it right now?
      command.redo if doit

      # Update size
      @size += command.size	# measured after redo

      # Update the mark distance
      @marker = @marker + 1 unless @marker.nil?

      # Done
      return true
    end

    #
    # Undo last command.
    #
    def undo
      unless @undolist.empty?
	command = @undolist.pop
	@size -= command.size
	command.undo
	@redolist.push(command)
	@marker = @marker - 1 unless @marker.nil?
	return true
      end
      return false
    end

    #
    # Redo next command
    #
    def redo
      unless @redolist.empty?
	command = @redolist.pop
	command.redo
	@undolist.push(command)
	@size += command.size
	@marker = @marker + 1 unless @marker.nil?
	return true
      end
      return false
    end

    #
    # Undo all commands
    #
    def undoAll
      undo while canUndo?
    end

    #
    # Redo all commands
    #
    def redoAll
      redo while canRedo?
    end

    #
    # Revert to marked
    #
    def revert
      unless @marker.nil?
	undo while (@marker > 0)
	redo while (@marker < 0)
	return true
      end
      return false
    end

    #
    # Return +true+ if we can still undo some commands
    # (i.e. the undo list is not empty).
    #
    def canUndo?
      (@undolist.empty? == false)
    end

    #
    # Return +true+ if we can still redo some commands
    # (i.e. the redo list is not empty).
    #
    def canRedo?
      (@redolist.empty? == false)
    end

    #
    # Return +true+ if there is a previously marked
    # state that we can revert to.
    #
    def canRevert?
      (@marker != nil) && (@marker != 0)
    end

    #
    # Returns the current undo command.
    #
    def current
      @undolist.last
    end

    #
    # Return the name of the first available undo command.
    # If no undo command is available, returns +nil+.
    #
    def undoName
      if canUndo?
        current.undoName
      else
        nil
      end
    end
    
    #
    # Return the name of the first available redo command.
    # If no redo command is available, returns +nil+.
    #
    def redoName
      if canRedo?
        @redolist.last.redoName
      else
        nil
      end
    end

    #
    # Returns the number of undo records.
    #
    def undoCount
      @undolist.size
    end

    #
    # Returns the total size of undo information.
    #
    def undoSize
      @size
    end

    #
    # Clear the list
    #
    def clear
      @undolist.clear
      @redolist.clear
      @marker = nil
      @size = 0
    end

    #
    # Trim undo list down to at most _nc_ commands.
    #
    def trimCount(nc)
      if @undolist.size > nc
	numRemoved = @undolist.size - nc
	@undolist[0, numRemoved].each { |command| @size -= command.size }
	@undolist[0, numRemoved] = nil
	@marker = nil if (@marker != nil && @marker > @undolist.size)
      end
    end

    #
    # Trim undo list down to at most _size_.
    #
    def trimSize(sz)
      if @size > sz
	s = 0
	@undolist.reverse.each_index { |i|
          j = @undolist.size - (i + 1)
          s += @undolist[j].size
          @undolist[j] = nil if (s > sz)
	}
	@undolist.compact!
	@marker = nil if (@marker != nil && @marker > @undolist.size)
      end
    end

    #
    # Mark current state
    #
    def mark
      @marker = 0
    end

    #
    # Unmark undo list
    #
    def unmark
      @marker = nil
    end

    #
    # Return +true+ if the undo list is marked.
    #
    def marked?
      @marker == 0
    end

    def onCmdUndo(sender, sel, ptr) # :nodoc:
      undo
      return 1
    end

    def onUpdUndo(sender, sel, ptr) # :nodoc:
      if canUndo?
	sender.handle(self, MKUINT(FXWindow::ID_ENABLE, SEL_COMMAND), nil)
      else
	sender.handle(self, MKUINT(FXWindow::ID_DISABLE, SEL_COMMAND), nil)
      end
      return 1
    end

    def onCmdRedo(sender, sel, ptr) # :nodoc:
      self.redo
      return 1
    end

    def onUpdRedo(sender, sel, ptr) # :nodoc:
      if canRedo?
	sender.handle(self, MKUINT(FXWindow::ID_ENABLE, SEL_COMMAND), nil)
      else
	sender.handle(self, MKUINT(FXWindow::ID_DISABLE, SEL_COMMAND), nil)
      end
      return 1
    end

    def onCmdClear(sender, sel, ptr) # :nodoc:
      clear
      return 1
    end

    def onUpdClear(sender, sel, ptr) # :nodoc:
      if canUndo? || canRedo?
	sender.handle(self, MKUINT(FXWindow::ID_ENABLE, SEL_COMMAND), nil)
      else
	sender.handle(self, MKUINT(FXWindow::ID_DISABLE, SEL_COMMAND), nil)
      end
      return 1
    end

    def onCmdRevert(sender, sel, ptr) # :nodoc:
      revert
      return 1
    end

    def onUpdRevert(sender, sel, ptr) # :nodoc:
      if canRevert?
	sender.handle(self, MKUINT(FXWindow::ID_ENABLE, SEL_COMMAND), nil)
      else
	sender.handle(self, MKUINT(FXWindow::ID_DISABLE, SEL_COMMAND), nil)
      end
      return 1
    end

    def onCmdUndoAll(sender, sel, ptr) # :nodoc:
      undoAll
      return 1
    end

    def onCmdRedoAll(sender, sel, ptr) # :nodoc:
      redoAll
      return 1
    end
  end

  #
  # FXCommand is an "abstract" base class for your application's commands. At a
  # minimum, your concrete subclasses of FXCommand should implement the
  # #undo, #redo, #undoName, and #redoName methods.
  #
  class FXCommand
    #
    # Undo this command; this should save enough information for a
    # subsequent redo.
    #
    def undo
      raise NotImpError
    end

    #
    # Redo this command; this should save enough information for a
    # subsequent undo.
    #
    def redo
      raise NotImpError
    end

    #
    # Name of the undo command to be shown on a button or menu command;
    # for example, "Undo Delete".
    #
    def undoName
      raise NotImpError
    end

    #
    # Name of the redo command to be shown on a button or menu command;
    # for example, "Redo Delete".
    #
    def redoName
      raise NotImpError
    end

    #
    # Returns the size of the information in the undo record, i.e. the
    # number of bytes required to store it in memory. This is only used
    # by the FXUndoList#trimSize method, which can be called to reduce
    # the memory use of the undo list to a certain size.
    #
    def size
      0
    end
  end
end
