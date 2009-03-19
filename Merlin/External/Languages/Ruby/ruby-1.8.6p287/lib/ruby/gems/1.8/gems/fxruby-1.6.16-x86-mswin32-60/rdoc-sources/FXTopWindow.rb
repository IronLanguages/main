module Fox
  #
  # Abstract base class for all top-level windows.
  #
  # TopWindows are usually managed by a Window Manager under X11 and
  # therefore borders and window-menus and other decorations like resize-
  # handles are subject to the Window Manager's interpretation of the
  # decoration hints.
  # When a TopWindow is closed, it sends a SEL_CLOSE message to its
  # target.  The target should return 0 in response to this message if
  # there is no objection to proceed with the closing of the window, and
  # return 1 otherwise.  After the SEL_CLOSE message has been sent and
  # no objection was raised, the window will delete itself.
  # When the session is closed, the window will send a SEL_SESSION_NOTIFY
  # message to its target, allowing the application to write any unsaved
  # data to the disk.  If the target returns 0, then the system will proceed
  # to close the session.  Subsequently a SEL_SESSION_CLOSED will be received
  # which causes the window to be closed with prejudice by calling the
  # function close(FALSE).
  # When receiving a SEL_UPDATE, the target can update the title string
  # of the window, so that the title of the window reflects the name
  # of the document, for example.
  # For convenience, TopWindow provides the same layout behavior as
  # the Packer widget, as well as docking and undocking of toolbars.
  # TopWindows can be owned by other windows, or be free-floating.
  # Owned TopWindows will usually remain stacked on top of the owner
  # windows. The lifetime of an owned window should not exceed that of
  # the owner.
  #
  # === Events
  #
  # The following messages are sent by FXTopWindow to its target:
  #
  # +SEL_MINIMIZE+::
  #   sent when the user clicks the minimize button in the upper right-hand
  #   corner of the top-level window.
  # +SEL_MAXIMIZE+::
  #   sent when the user clicks the maximize button in the upper right-hand
  #   corner of the top-level window.
  # +SEL_RESTORE+::
  #   sent when the user clicks the restore button in the upper right-hand
  #   corner of the top-level window.
  # +SEL_CLOSE+::
  #   sent when the user clicks the close button in the upper right-hand
  #   corner of the top-level window.
  # +SEL_SESSION_NOTIFY+:
  #   sent when the session is closed.:
  # +SEL_SESSION_CLOSED+::
  #   sent after the session is closed.
  #
  # === Title and border decorations
  #
  # +DECOR_NONE+::            Borderless window
  # +DECOR_TITLE+::           Window title
  # +DECOR_MINIMIZE+::        Minimize button
  # +DECOR_MAXIMIZE+::        Maximize button
  # +DECOR_CLOSE+::           Close button
  # +DECOR_BORDER+::          Border
  # +DECOR_SHRINKABLE+::      Window can become smaller
  # +DECOR_STRETCHABLE+::     Window can become larger
  # +DECOR_RESIZE+::          Resize handles
  # +DECOR_MENU+::            Window menu
  # +DECOR_ALL+::             All of the above
  #
  # === Initial window placement
  #
  # +PLACEMENT_DEFAULT+::     Place it at the default size and location
  # +PLACEMENT_VISIBLE+::     Place window to be fully visible
  # +PLACEMENT_CURSOR+::      Place it under the cursor position
  # +PLACEMENT_OWNER+::       Place it centered on its owner
  # +PLACEMENT_SCREEN+::      Place it centered on the screen
  # +PLACEMENT_MAXIMIZED+::   Place it maximized to the screen size
  #
  # === Message identifiers
  #
  # +ID_MAXIMIZE+::		Maximize the window
  # +ID_MINIMIZE+::		Minimize the window
  # +ID_RESTORE+::		Restore the window
  # +ID_CLOSE+::		Close the window
  # +ID_QUERY_DOCK+::		Toolbar asks to dock
  #

  class FXTopWindow < FXShell

    # Window title [String]
    attr_accessor	:title
    
    # Top padding, in pixels [Integer]
    attr_accessor	:padTop
    
    # Bottom padding, in pixels [Integer]
    attr_accessor	:padBottom
    
    # Left padding, in pixels [Integer]
    attr_accessor	:padLeft
    
    # Right padding, in pixels [Integer]
    attr_accessor	:padRight
    
    # Horizontal spacing between child widgets, in pixels [Integer]
    attr_accessor	:hSpacing
    
    # Vertical spacing between child widgets, in pixels [Integer]
    attr_accessor	:vSpacing
    
    # Packing hints for child widgets [Integer]
    attr_accessor	:packingHints
    
    # Title and border decorations (see above) [Integer]
    attr_accessor	:decorations
    
    # Window icon [FXIcon]
    attr_accessor	:icon
    
    # Window mini (title) icon [FXIcon]
    attr_accessor	:miniIcon

    # Show this window with given _placement_
    # (one of +PLACEMENT_DEFAULT+, +PLACEMENT_VISIBLE+, +PLACEMENT_CURSOR+, +PLACEMENT_OWNER+, +PLACEMENT_SCREEN+ or +PLACEMENT_MAXIMIZED+).
    def show(placement) ; end
  
    # Position the window based on _placement_
    # (one of +PLACEMENT_DEFAULT+, +PLACEMENT_VISIBLE+, +PLACEMENT_CURSOR+, +PLACEMENT_OWNER+, +PLACEMENT_SCREEN+ or +PLACEMENT_MAXIMIZED+).
    def place(placement) ; end
    
    # Obtain border sizes added to our window by the window manager.
    # Returns a 4-element array containing the left, right, top and bottom border sizes (in pixels).
    def getWMBorders(); end
    
    # Return +true+ if window is maximized.
    def maximized? ; end
    
    # Return +true+ if window is minimized.
    def minimized? ; end

    #
    # Maximize window and return +true+ if maximized.
    # If _notify_ is +true+, sends a +SEL_MAXIMIZE+ message to its message target.
    #
    def maximize(notify=false); end
    
    #
    # Minimize or iconify window and return +true+ if minimized.
    # If _notify_ is +true+, sends a +SEL_MINIMIZE+ message to its message target.
    #
    def minimize(notify=false); end
    
    #
    # Restore window to normal and return +true+ if restored.
    # If _notify_ is +true+, sends a +SEL_RESTORE+ message to its message target.
    #
    def restore(notify=false); end
    
    #
    # Close the window, return +true+ if actually closed. If _notify_ is +true+, the target
    # will receive a +SEL_CLOSE+ message to determine if it is OK to close the window.
    # If the target ignores the +SEL_CLOSE+ message or returns 0, the window will
    # be closed, and subsequently deleted. When the last main window has been
    # closed, the application will receive an +ID_QUIT+ message and will be closed.
    #
    def close(notify=false); end
  end
end
