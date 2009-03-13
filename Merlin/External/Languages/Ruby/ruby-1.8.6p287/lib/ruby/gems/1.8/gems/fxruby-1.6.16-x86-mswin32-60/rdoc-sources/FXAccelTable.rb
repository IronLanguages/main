module Fox
  #
  # The accelerator table sends a message to a specific
  # target object when the indicated key and modifier combination
  # is pressed.
  #
  class FXAccelTable < FXObject
    #
    # Construct empty accelerator table.
    #
    def initialize # :yields: acceleratorTable
    end

    #
    # Add an accelerator to the table. The _hotKey_ is a code returned
    # by the Fox.fxparseAccel method. For example, to associate the
    # Ctrl+S keypress with sending a "save" command to a document, you
    # might use code like this:
    #
    #   hotKey = fxparseAccel("Ctrl+S")
    #   accelTable.addAccel(hotKey, doc, FXSEL(SEL_COMMAND, MyDocument::ID_SAVE))
    #
    # ==== Parameters:
    #
    # +hotKey+::	the hotkey associated with this accelerator [Integer]
    # +target+::		message target [FXObject]
    # +seldn+::		selector for the +SEL_KEYPRESS+ event [Integer]
    # +selup+::		selector for the +SEL_KEYRELEASE+ event [Integer]
    #
    def addAccel(hotKey, target=nil, seldn=0, selup=0) ; end

    #
    # Remove an accelerator from the table.
    #
    def removeAccel(hotKey) ; end

    #
    # Return +true+ if accelerator specified.
    # Here, _hotKey_ is a code representing an accelerator key as returned
    # by the Fox.fxparseAccel method. For example,
    #
    #   if accelTable.hasAccel?(fxparseAccel("Ctrl+S"))
    #     ...
    #   end
    #
    def hasAccel?(hotKey) ; end

    #
    # Return the target object of the given accelerator, or +nil+ if
    # the accelerator is not present in this accelerator table.
    # Here, _hotKey_ is a code representing an accelerator key as returned
    # by the Fox.fxparseAccel method. For example,
    #
    #   doc = accelTable.targetofAccel(fxparseAccel("Ctrl+S"))
    #
    def targetOfAccel(hotKey) ; end
  
    #
    # Remove mapping for specified hot key.
    # Here, _hotKey_ is a code representing an accelerator key as returned
    # by the Fox.fxparseAccel method. For example,
    #
    #   accelTable.removeAccel(fxparseAccel("Ctrl+S"))
    #
    def removeAccel(hotKey) ; end
  end

  #
  # Parse accelerator from string, yielding modifier and
  # key code.  For example, parseAccel("Ctl+Shift+X")
  # yields MKUINT(KEY_X,CONTROLMASK|SHIFTMASK).
  #
  def parseAccel(string); end

  #
  # Unparse hot key comprising modifier and key code back
  # into a string suitable for parsing with #parseHotKey.
  #
  def unparseAccel(key); end

  #
  # Parse hot key from string, yielding modifier and
  # key code.  For example, parseHotKey(""Salt && &Pepper!"")
  # yields MKUINT(KEY_p,ALTMASK).
  #
  def parseHotKey(string); end

  #
  # Obtain hot key offset in string, or -1 if not found.
  # For example, findHotKey("Salt && &Pepper!") yields 7.
  # Note that this is the byte-offset, not the character
  # index!
  #
  def findHotKey(string); end

  #
  # Strip hot key combination from the string.
  # For example, stripHotKey("Salt && &Pepper") should
  # yield "Salt & Pepper".
  #
  def stripHotKey(string); end
end
