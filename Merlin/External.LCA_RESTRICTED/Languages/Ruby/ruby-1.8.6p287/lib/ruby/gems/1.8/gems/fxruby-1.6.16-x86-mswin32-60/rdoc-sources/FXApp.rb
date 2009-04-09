module Fox
  # FOX Event 
  class FXEvent
  
    # Event type [Integer]
    attr_reader :type
    
    # Time of last event [Integer]
    attr_reader :time
    
    # Window-relative x-coordinate [Integer]
    attr_reader :win_x
    
    # Window-relative y-coordinate [Integer]
    attr_reader :win_y
    
    # Root window x-coordinate [Integer]
    attr_reader :root_x
    
    # Root window y-coordinate [Integer]
    attr_reader :root_y
    
    # Keyboard/modifier state [Integer]
    attr_reader :state
    
    # Button, keysym or mode; DDE source [Integer]
    attr_reader :code
    
    # Text of keyboard event [String]
    attr_reader :text
    
    # Window-relative x-coordinate of previous mouse location [Integer]
    attr_reader :last_x
    
    # Window-relative y-coordinate of previous mouse location [Integer]
    attr_reader :last_y
    
    # Window-relative x-coordinate of mouse press [Integer]
    attr_reader :click_x
    
    # Window-relative y-coordinate of mouse press [Integer]
    attr_reader :click_y
    
    # Root window x-coordinate of mouse press [Integer]
    attr_reader :rootclick_x
    
    # Root window y-coordinate of mouse press [Integer]
    attr_reader :rootclick_y
    
    # Time of mouse button press [Integer]
    attr_reader :click_time
    
    # Mouse button pressed [Integer]
    attr_reader :click_button
    
    # Click count [Integer]
    attr_reader :click_count
    
    # Target drag type being requested [Integer]
    attr_reader :target

    # Return true if cursor moved since last press
    def moved? ; end

    # Exposed rectangle for paint events
    def rect ; end

    # Return true if this is a synthetic expose event
    def synthetic? ; end
  end

  #
  # Application Object
  #
  # === Events
  #
  # The FXApp object itself doesn't have a designated message target like
  # other FOX objects, but it can send messages to objects for a few
  # special events.
  #
  # [*Timers*]
  #   When a timeout event is registered with the application using the
  #   addTimeout method, a +SEL_TIMEOUT+ message is sent to the message
  #   target.
  # [*Chores*]
  #   When a chore event is registered with the application using the
  #   addChore method, a +SEL_CHORE+ message is sent to the message target.
  # [*Inputs*]
  #   When an input event is registered with the application using the
  #   addInput method, a +SEL_IO_READ+, +SEL_IO_WRITE+ or +SEL_IO_EXCEPT+
  #   message may be sent to the message target.
  # [*Signals*]
  #   When a signal handler object is registered with the application using
  #   the addSignal method, a +SEL_SIGNAL+ message may be sent to the message
  #   target.
  # 
  # === File input modes for #addInput
  #
  # +INPUT_NONE+::		inactive
  # +INPUT_READ+::		read input fd
  # +INPUT_WRITE+::		write input fd
  # +INPUT_EXCEPT+::		except input fd
  #
  # === All ways of being modal
  #
  # +MODAL_FOR_NONE+::		Non modal event loop (dispatch normally)
  # +MODAL_FOR_WINDOW+::	Modal dialog (beep if outside of modal dialog)
  # +MODAL_FOR_POPUP+::		Modal for popup (always dispatch to popup)
  #
  # === Default cursors provided by the application
  #
  # These constants symbolically represent the different cursor shapes used
  # in FOX applications, and can be used as the _which_ arguments for
  # #getDefaultCursor and #setDefaultCursor.
  #
  # +DEF_ARROW_CURSOR+::      Arrow cursor
  # +DEF_RARROW_CURSOR+::     Reverse arrow cursor
  # +DEF_TEXT_CURSOR+::       Text cursor
  # +DEF_HSPLIT_CURSOR+::     Horizontal split cursor
  # +DEF_VSPLIT_CURSOR+::     Vertical split cursor
  # +DEF_XSPLIT_CURSOR+::     Cross split cursor
  # +DEF_SWATCH_CURSOR+::     Color swatch drag cursor
  # +DEF_MOVE_CURSOR+::       Move cursor
  # +DEF_DRAGH_CURSOR+::      Resize horizontal edge
  # +DEF_DRAGV_CURSOR+::      Resize vertical edge
  # +DEF_DRAGTL_CURSOR+::     Resize upper-leftcorner
  # +DEF_DRAGBR_CURSOR+::     Resize bottom-right corner
  # +DEF_DRAGTR_CURSOR+::     Resize upper-right corner
  # +DEF_DRAGBL_CURSOR+::     Resize bottom-left corner
  # +DEF_DNDSTOP_CURSOR+::    Drag and drop stop
  # +DEF_DNDCOPY_CURSOR+::    Drag and drop copy
  # +DEF_DNDMOVE_CURSOR+::    Drag and drop move
  # +DEF_DNDLINK_CURSOR+::    Drag and drop link
  # +DEF_CROSSHAIR_CURSOR+::  Cross hair cursor
  # +DEF_CORNERNE_CURSOR+::   North-east cursor
  # +DEF_CORNERNW_CURSOR+::   North-west cursor
  # +DEF_CORNERSE_CURSOR+::   South-east cursor
  # +DEF_CORNERSW_CURSOR+::   South-west cursor
  # +DEF_HELP_CURSOR+::	      Help arrow cursor
  # +DEF_HAND_CURSOR+::	      Hand cursor
  # +DEF_ROTATE_CURSOR+::     Rotate cursor
  # +DEF_WAIT_CURSOR+::       Wait cursor
  #
  # === Messages identifiers
  #
  # +ID_QUIT+::               Terminate the application normally
  # +ID_DUMP+::               Dump the current widget tree

  class FXApp < FXObject

    # Application name [String]
    attr_reader :appName

    # Vendor name [String]
    attr_reader :vendorName
    
    # Argument count [Integer]
    attr_reader :argc
    
    # Argument vector [Array]
    attr_reader :argv

    # Display [Integer]
    attr_reader :display

    # Border color [FXColor]
    attr_accessor :borderColor

    # Background color of GUI controls [FXColor]
    attr_accessor :baseColor

    # Hilite color of GUI controls [FXColor]
    attr_accessor :hiliteColor

    # Shadow color of GUI controls [FXColor]
    attr_accessor :shadowColor

    # Default background color [FXColor]
    attr_accessor :backColor

    # Default foreground color [FXColor]
    attr_accessor :foreColor

    # Default foreground color for selected objects [FXColor]
    attr_accessor :selforeColor

    # Default background color for selected objects [FXColor]
    attr_accessor :selbackColor

    # Default foreground color for tooltips [FXColor]
    attr_accessor :tipforeColor

    # Default background color for tooltips [FXColor]
    attr_accessor :tipbackColor
    
    # Default text color for selected menu items [FXColor]
    attr_accessor :selMenuTextColor
    
    # Default background color for selected menu items [FXColor]
    attr_accessor :selMenuBackColor

    # Default visual [FXVisual]
    attr_accessor :defaultVisual

    # Default font [FXFont]
    attr_accessor :normalFont

    # Wait cursor [FXCursor]
    attr_accessor :waitCursor

    # Monochrome visual [FXVisual]
    attr_reader :monoVisual

    # Root window [FXRootWindow]
    attr_reader :rootWindow

    # The window under the cursor, if any [FXWindow]
    attr_reader :cursorWindow

    # The window at the end of the focus chain, if any [FXWindow]
    attr_reader :focusWindow
    
    # The active top-level window, if any [FXWindow]
    attr_reader :activeWindow

    # The main window, if any [FXWindow]
    attr_reader :mainWindow

    # The window of the current modal loop [FXWindow]
    attr_reader :modalWindow

    # Mode of current modal loop [Integer]
    attr_reader :modalModality

    # Typing speed used for the FXIconList, FXList and FXTreeList widgets' lookup features,
    # in milliseconds. Default value is 1000 milliseconds.
    attr_accessor :typingSpeed

    # Click speed, in milliseconds [Integer]
    attr_accessor :clickSpeed

    # Scroll speed, in milliseconds [Integer]
    attr_accessor :scrollSpeed

    # Scroll delay time, in milliseconds [Integer]
    attr_accessor :scrollDelay

    # Blink speed, in milliseconds [Integer]
    attr_accessor :blinkSpeed

    # Animation speed, in milliseconds [Integer]
    attr_accessor :animSpeed

    # Menu pause, in milliseconds [Integer]
    attr_accessor :menuPause

    # Tooltip pause, in milliseconds [Integer]
    attr_accessor :tooltipPause

    # Tooltip time, in milliseconds [Integer]
    attr_accessor :tooltipTime

    # Drag delta, in pixels [Integer]
    attr_accessor :dragDelta

    # Number of wheel lines [Integer]
    attr_accessor :wheelLines
    
    # Scroll bar size [Integer]
    attr_accessor :scrollBarSize

    # Amount of time (in milliseconds) to yield to Ruby's thread scheduler [Integer]
    attr_accessor :sleepTime
    
    # Message translator [FXTranslator]
    attr_accessor :translator

    # Copyright notice for library
    def FXApp.copyright() ; end

    #
    # Construct application object; the _appName_ and _vendorName_ strings are used
    # as keys into the registry database for this application's settings.
    # Only one single application object can be constructed.
    #
    def initialize(appName="Application", vendorName="FoxDefault") # :yields: theApp
    end

    #
    # Open connection to display; this is called by #init.
    #
    def openDisplay(dpyname=nil) ; end
  
    # Close connection to the display
    def closeDisplay() ; end

    # Return true if the application has been initialized.
    def initialized?; end

    # Return +true+ if input methods are supported.
    def hasInputMethod?; end

    #
    # Process any timeouts due at this time.
    #
    def handleTimeouts(); end
    
    #
    # Add signal processing message to be sent to target object when 
    # the signal _sig_ is raised; flags are to be set as per POSIX definitions.
    # When _immediate_ is +true+, the message will be sent to the target right away;
    # this should be used with extreme care as the application is interrupted
    # at an unknown point in its execution.
    #
    def addSignal(sig, tgt, sel, immediate=false, flags=0) ; end

    #
    # Remove signal message for signal _sig_.
    #
    def removeSignal(sig) ; end

    #
    # Remove input message and target object for the specified file descriptor
    # and mode, which is a bitwise OR of (+INPUT_READ+, +INPUT_WRITE+, +INPUT_EXCEPT+).
    #
    def removeInput(fd, mode) ; end

    # Create application's windows
    def create() ; end

    # Destroy application's windows
    def destroy() ; end

    # Detach application's windows
    def detach() ; end

    #
    # Return key state (either +true+ or +false+) for _keysym_.
    #
    def getKeyState(keysym); end

    #
    # Peek to determine if there's an event.
    #
    def peekEvent(); end

    # Perform one event dispatch; return +true+ if event was dispatched.
    def runOneEvent(blocking=true); end

    # Run the main application event loop until #stop is called,
    # and return the exit code passed as argument to #stop.
    def run(); end

    #
    # Run an event loop till some flag becomes non-zero, and
    # then return.
    #
    def runUntil(condition); end

    #
    # Run event loop while events are available, non-modally.
    # Return when no more events, timers, or chores are outstanding.
    #
    def runWhileEvents(); end

    #
    # Run event loop while there are events are available in the queue.
    # Returns 1 when all events in the queue have been handled, and 0 when
    # the event loop was terminated due to #stop or #stopModal.
    # Except for the modal window and its children, user input to all windows 
    # is blocked; if the modal window is +nil+, all user input is blocked.
    #
    def runModalWhileEvents(window=nil); end

    # Run modal event loop, blocking keyboard and mouse events to all windows
    # until #stopModal is called.
    def runModal(); end

    # Run a modal event loop for the given window, until #stop or #stopModal is 
    # called. Except for the modal window and its children, user input to all
    # windows is blocked; if the modal window is +nil+ all user input is blocked.
    def runModalFor(window); end
  
    # Run modal while window is shown, or until #stop or #stopModal is called. 
    # Except for the modal window and its children, user input to all windows
    # is blocked; if the modal window is +nil+ all user input is blocked.
    def runModalWhileShown(window); end
  
    # Run popup menu while shown, until #stop or #stopModal is called.
    # Also returns when entering previous cascading popup menu.
    def runPopup(window); end
  
    # Returns +true+ if the window is modal
    def modal?(window) ; end

    # Terminate the outermost event loop, and all inner modal loops;
    # All more deeper nested event loops will be terminated with code equal
    # to 0, while the outermost event loop will return code equal to _value_.
    def stop(value=0); end
  
    #
    # Break out of the matching modal loop, returning code equal to _value_.
    # All deeper nested event loops are terminated with code equal to 0.
    #
    def stopModal(window, value=0); end
  
    #
    # Break out of the innermost modal loop, returning code equal to _value_.
    #
    def stopModal(value=0); end

    # Force GUI refresh
    def forceRefresh(); end

    # Schedule a refresh
    def refresh(); end

    # Flush pending repaints
    def flush(sync=false); end

    # Paint all windows marked for repainting.
    # On return all the applications windows have been painted.
    def repaint(); end
  
    #
    # Return a reference to the registry (an FXRegistry instance).
    # The registry keeps settings and configuration information for an application,
    # which are automatically loaded when the application starts
    # up, and saved when the application terminates.
    #
    def reg; end

    # Initialize application.
    # Parses and removes common command line arguments, reads the registry.
    # Finally, if _connect_ is +true+, it opens the display.
    def init(argv, connect=true) ; end

    # Exit application.
    # Closes the display and writes the registry.
    def exit(code=0); end
  
    #
    # Register a drag type with the given name and return the drag
    # drag type. If this drag type has already been registered, this
    # method will return the previously returned drag type. For example,
    #
    #   yamlDragType = app.registerDragType("application/x-yaml")
    #
    # See also #getDragTypeName.
    #
    def registerDragType(name) ; end

    #
    # Return the name of a previously registered drag type, e.g.
    #
    #   dragTypeName = app.getDragTypeName(yamlDragType)
    #
    # See also #registerDragType.
    #
    def getDragTypeName(dragType) ; end

    # Beep
    def beep(); end
  
    # Return application instance
    def FXApp.instance(); end
  
    # End the most deeply nested wait-cursor block.
    # See also #beginWaitCursor.
    def endWaitCursor(); end
  
    #
    # Return a reference to one of the default application cursors (an
    # FXCursor instance), where _which_ is one of the default cursor
    # identifiers listed above, e.g.
    #
    #   rotateCursor = app.getDefaultCursor(DEF_ROTATE_CURSOR)
    # 
    # See also #setDefaultCursor.
    #
    def getDefaultCursor(which) ; end
  
    #
    # Replace one of the default application cursors with _cursor_; e.g
    #
    #   app.setDefaultCursor(DEF_ROTATE_CURSOR, myRotateCursor)
    #
    # See also #getDefaultCursor.
    #
    def setDefaultCursor(which, cursor); end
  
    #
    # Write a window and its children, and all resources reachable from this
    # window, into the stream _store_ (an FXStream instance).
    #
    # ==== Parameters:
    #
    # +store+::		[FXStream]
    # +window+::	[FXWindow]
    #
    def writeWindow(store, window); end

    #
    # Read a window and its children from the stream store, and append
    # it under father; note it is initially not created yet.
    # Return a reference to the new window.
    #
    # ==== Parameters:
    #
    # +store+::		[FXStream]
    # +father+::	[FXWindow]
    # +owner+::		[FXWindow]
    #
    def readWindow(store, father, owner); end

    #
    # Return a reference to the application-wide mutex (an FXMutex instance).
    # Normally, the main user interface thread holds this mutex,
    # insuring that no other threads are modifying data during the
    # processing of user interface messages. However, whenever the
    # main user interface thread blocks for messages, it releases
    # this mutex, to allow other threads to modify the same data.
    # When a new message becomes available, the main user interface
    # thread regains the mutex prior to dispatching the message.
    # Other threads should hold this mutex only for short durations,
    # so as to not starve the main user interface thread.
    #
    def mutex(); end

    # Dump widget information
    def dumpWidgets() ; end
    
    # Return the number of existing windows.
    def windowCount; end
    
    # Enable support for multithreaded applications
    def enableThreads(); end
  
    # Disable support for multithreaded applications
    def disableThreads(); end

    # Check to see if multithreaded applications are supported
    def threadsEnabled?(); end
  end
end
