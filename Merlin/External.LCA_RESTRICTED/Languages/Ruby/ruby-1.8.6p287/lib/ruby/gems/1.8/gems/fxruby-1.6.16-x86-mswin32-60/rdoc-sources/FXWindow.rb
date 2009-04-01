module Fox
  #
  # Base class for all windows
  #
  # === Events
  #
  # The following messages are sent by FXWindow to its target:
  #
  # +SEL_MAP+::			sent when the window is mapped to the screen; the message data is an FXEvent instance.
  # +SEL_UNMAP+::		sent when the window is unmapped; the message data is an FXEvent instance.
  # +SEL_CONFIGURE+::		sent when the window's size changes; the message data is an FXEvent instance.
  # +SEL_ENTER+::		sent when the mouse cursor enters this window
  # +SEL_LEAVE+::		sent when the mouse cursor leaves this window
  # +SEL_FOCUSIN+::		sent when this window gains the focus
  # +SEL_FOCUSOUT+::		sent when this window loses the focus
  # +SEL_UPDATE+::		sent when this window needs an update
  # +SEL_UNGRABBED+::		sent when this window loses the mouse grab (or capture)
  #
  # For each of the following keyboard-related events, the message data is an FXEvent instance:
  #
  # +SEL_KEYPRESS+::		sent when a key is pressed
  # +SEL_KEYRELEASE+::		sent when a key is released
  #
  # For each of the following mouse-related events, the message data is an FXEvent instance:
  #
  # +SEL_MOTION+::		sent when the mouse moves
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up
  # +SEL_MOUSEWHEEL+::		sent when the mouse wheel is scrolled
  #
  # For each of the following selection-related events, the message data is an FXEvent instance:
  #
  # +SEL_SELECTION_GAINED+::	sent when this window acquires the selection
  # +SEL_SELECTION_LOST+::	sent when this window loses the selection
  # +SEL_SELECTION_REQUEST+::	sent when this window's selection is requested
  #
  # For each of the following clipboard-related events, the message data is an FXEvent instance:
  #
  # +SEL_CLIPBOARD_GAINED+::	sent when this window acquires the clipboard
  # +SEL_CLIPBOARD_LOST+::	sent when this window loses the clipboard
  # +SEL_CLIPBOARD_REQUEST+::	sent when this window's clipboard data is requested
  #
  # For each of the following drag-and-drop events, the message data is an FXEvent instance:
  #
  # +SEL_BEGINDRAG+::		sent at the beginning of a drag operation
  # +SEL_DRAGGED+::		sent while stuff is being dragged around
  # +SEL_ENDDRAG+::		sent at the end of a drag operation
  # +SEL_DND_ENTER+::		drag-and-drop enter
  # +SEL_DND_LEAVE+::		drag-and-drop leave
  # +SEL_DND_MOTION+::		drag-and-drop motion
  # +SEL_DND_DROP+::		drag-and-drop motion
  # +SEL_DND_REQUEST+::		drag-and-drop request
  #
  # === Layout hints for child widgets
  #
  # +LAYOUT_NORMAL+::       Default layout mode
  # +LAYOUT_SIDE_TOP+::     Pack on top side (default)
  # +LAYOUT_SIDE_BOTTOM+::  Pack on bottom side
  # +LAYOUT_SIDE_LEFT+::    Pack on left side
  # +LAYOUT_SIDE_RIGHT+::   Pack on right side
  # +LAYOUT_FILL_COLUMN+::  Matrix column is stretchable
  # +LAYOUT_FILL_ROW+::     Matrix row is stretchable
  # +LAYOUT_LEFT+::         Stick on left (default)
  # +LAYOUT_RIGHT+::        Stick on right
  # +LAYOUT_CENTER_X+::     Center horizontally
  # +LAYOUT_FIX_X+::        X fixed
  # +LAYOUT_TOP+::          Stick on top (default)
  # +LAYOUT_BOTTOM+::       Stick on bottom
  # +LAYOUT_CENTER_Y+::     Center vertically
  # +LAYOUT_FIX_Y+::        Y fixed
  # +LAYOUT_FIX_WIDTH+::    Width fixed
  # +LAYOUT_FIX_HEIGHT+::   Height fixed
  # +LAYOUT_MIN_WIDTH+::    Minimum width is the default
  # +LAYOUT_MIN_HEIGHT+::   Minimum height is the default
  # +LAYOUT_FILL_X+::       Stretch or shrink horizontally
  # +LAYOUT_FILL_Y+::       Stretch or shrink vertically
  # +LAYOUT_FILL::          Stretch or shrink in both directions
  # +LAYOUT_EXPLICIT+::     Explicit placement
  # +LAYOUT_DOCK_SAME+::    Dock on same galley, if it fits
  # +LAYOUT_DOCK_NEXT+::    Dock on next galley
  #
  # === Frame border appearance styles (for subclasses)
  #
  # +FRAME_NONE+::          Default is no frame
  # +FRAME_SUNKEN+::        Sunken border
  # +FRAME_RAISED+::        Raised border
  # +FRAME_THICK+::         Thick border
  # +FRAME_GROOVE+::        A groove or etched-in border
  # +FRAME_RIDGE+::         A ridge or embossed border
  # +FRAME_LINE+::          Simple line border
  # +FRAME_NORMAL+::        Regular raised/thick border
  #
  # === Packing style (for packers)
  #
  # +PACK_NORMAL+::         Default is each its own size
  # +PACK_UNIFORM_HEIGHT+:: Uniform height
  # +PACK_UNIFORM_WIDTH+::  Uniform width
  #
  # === Message IDs common to most windows
  #
  # +ID_NONE+::                 x
  # +ID_HIDE+::                 x
  # +ID_SHOW+::                 x
  # +ID_TOGGLESHOWN+::          x
  # +ID_LOWER+::                x
  # +ID_RAISE+::                x
  # +ID_DELETE+::               x
  # +ID_DISABLE+::              x
  # +ID_ENABLE+::               x
  # +ID_TOGGLEENABLED+::        x
  # +ID_UNCHECK+::              x
  # +ID_CHECK+::                x
  # +ID_UNKNOWN+::              x
  # +ID_UPDATE+::               x
  # +ID_AUTOSCROLL+::           x
  # +ID_HSCROLLED+::            x
  # +ID_VSCROLLED+::            x
  # +ID_SETVALUE+::             x
  # +ID_SETINTVALUE+::          x
  # +ID_SETREALVALUE+::         x
  # +ID_SETSTRINGVALUE+::       x
  # +ID_SETINTRANGE+::          x
  # +ID_SETREALRANGE+::         x
  # +ID_GETINTVALUE+::          x
  # +ID_GETREALVALUE+::         x
  # +ID_GETSTRINGVALUE+::       x
  # +ID_GETINTRANGE+::          x
  # +ID_GETREALRANGE+::         x
  # +ID_QUERY_MENU+::           x
  # +ID_HOTKEY+::               x
  # +ID_ACCEL+::                x
  # +ID_UNPOST+::               x
  # +ID_POST+::                 x
  # +ID_MDI_TILEHORIZONTAL+::   x
  # +ID_MDI_TILEVERTICAL+::     x
  # +ID_MDI_CASCADE+::          x
  # +ID_MDI_MAXIMIZE+::         x
  # +ID_MDI_MINIMIZE+::         x
  # +ID_MDI_RESTORE+::          x
  # +ID_MDI_CLOSE+::            x
  # +ID_MDI_WINDOW+::           x
  # +ID_MDI_MENUWINDOW+::       x
  # +ID_MDI_MENUMINIMIZE+::     x
  # +ID_MDI_MENURESTORE+::      x
  # +ID_MDI_MENUCLOSE+::        x
  # +ID_MDI_NEXT+::             x
  # +ID_MDI_PREV+::             x

  class FXWindow < FXDrawable
  
    # This window's parent window [FXWindow]
    attr_reader	:parent

    # This window's owner window [FXWindow]
    attr_reader		:owner

    # The shell window for this window [FXWindow]
    attr_reader		:shell

    # Root window [FXWindow]
    attr_reader		:root

    # Next (sibling) window, if any [FXWindow]
    attr_reader		:next

    # Previous (sibling) window, if any [FXWindow]
    attr_reader		:prev

    # This window's first child window, if any [FXWindow]
    attr_reader		:first

    # This window's last child window, if any [FXWindow]
    attr_reader		:last

    # Currently focused child window, if any [FXWindow]
    attr_reader		:focus

    # Window key [Integer]
    attr_accessor	:key

    # Message target object for this window [FXObject]
    attr_accessor	:target

    # Message identifier for this window [Integer]
    attr_accessor	:selector

    # This window's x-coordinate, in the parent's coordinate system [Integer]
    attr_accessor	:x

    # This window's y-coordinate, in the parent's coordinate system [Integer]
    attr_accessor	:y

    # The accelerator table for this window [FXAccelTable]
    attr_accessor	:accelTable

    # Layout hints for this window [Integer]
    attr_accessor	:layoutHints

    # Number of child windows for this window [Integer]
    attr_reader		:numChildren

    # Default cursor for this window [FXCursor]
    attr_accessor	:defaultCursor

    # Drag cursor for this window [FXCursor]
    attr_accessor	:dragCursor

    # Window background color [FXColor]
    attr_accessor	:backColor

    # Common DND type: Raw octet stream
    def FXWindow.octetType; end

    # Common DND type: Delete request
    def FXWindow.deleteType; end

    # Common DND type: ASCII text request
    def FXWindow.textType; end

    # Common DND type: UTF-8 text request
    def FXWindow.utf8Type; end

    # Common DND type: UTF-16 text request
    def FXWindow.utf16Type; end

    # Common DND type: Color
    def FXWindow.colorType; end

    # Common DND type: URI List
    def FXWindow.urilistType; end

    # Common DND type: Clipboard text type (pre-registered)
    def FXWindow.stringType; end
  
    # Common DND type: Clipboard image type (pre-registered)
    def FXWindow.imageType; end

    # Common DND type name: Raw octet stream
    def FXWindow.octetTypeName() ; end

    # Common DND type name: Delete request
    def FXWindow.deleteTypeName() ; end
    
    # Common DND type name: ASCII text
    def FXWindow.textTypeName() ; end
    
    # Common DND type name: Color
    def FXWindow.colorTypeName() ; end
    
    # Common DND type name: URI List
    def FXWindow.urilistTypeName() ; end
  
    # Common DND type name: UTF-8 text request
    def FXWindow.utf8TypeName() ; end

    # Common DND type name: UTF-16 text request
    def FXWindow.utf16TypeName() ; end

    #
    # Return an initialized FXWindow instance, for a child window.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this window [FXComposite]
    # +opts+::	window options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(p, opts=0, x=0, y=0, width=0, height=0) # :yields: theWindow
    end
  
    #
    # Return an initialized FXWindow instance, for a shell window.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +vis+::	the visual to use for this window [FXVisual]
    #
    def initialize(a, vis) # :yields: theWindow
    end

    #
    # Return an initialized FXWindow instance, for an owned window.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +own+::	the owner window for this window [FXWindow]
    # +opts+::	window options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    #
    def initialize(a, own, opts, x, y, w, h) # :yields: theWindow
    end

    # Return the window width (in pixels).
    def width; end
    
    #
    # Set the window width; and flag the widget as being in need of
    # layout by its parent.  This does not immediately update the server-
    # side representation of the widget.
    #
    def width=(w); end

    # Return the window height (in pixels).
    def height; end

    #
    # Set the window height; and flag the widget as being in need of
    # layout by its parent.  This does not immediately update the server-
    # side representation of the widget.
    #
    def height=(h); end

    # Return the default width of this window
    def defaultWidth(); end
  
    # Return the default height of this window 
    def defaultHeight(); end
  
    # Return width for given height
    def getWidthForHeight(givenHeight); end
  
    # Return height for given width
    def getHeightForWidth(givenWidth); end
  
    #
    # Add this hot key to the closest ancestor's accelerator table.
    #
    def addHotKey(code)
      accel = nil
      win = self
      while win && (accel = win.accelTable).nil?
        win = win.parent
      end
      if accel
        accel.addAccel(code, self, MKUINT(ID_HOTKEY, SEL_KEYPRESS), MKUINT(ID_HOTKEY, SEL_KEYRELEASE))
      end
    end
  
    #
    # Remove this hot key from the closest ancestor's accelerator
    # table.
    #
    def remHotKey(code)
      accel = nil
      win = self
      while win && (accel = win.accelTable).nil?
        win = win.parent
      end
      if accel
        accel.removeAccel(code)
      end
    end
  
    # Return +true+ if this window is a shell window.
    def shell?() ; end
  
    #
    # Return +true+ if specified _window_ is ancestor of this window.
    #
    def childOf?(window) ; end
  
    #
    # Return +true+ if this window contains _child_ in its subtree.
    #
    def containsChild?(child) ; end
  
    # Return the child window at specified coordinates (_x_, _y_)
    def getChildAt(x, y) ; end
  
    # Return the index (starting from zero) of the specified child _window_, 
    # or -1 if the window is not a child of this window.
    def indexOfChild(window) ; end
  
    # Remove specified child window
    def removeChild(child) ; end
  
    # Return the child window at specified index. Raises IndexError if _index_ is out of range.
    def childAtIndex(index) ; end
  
    # Return the common ancestor of window _a_ and window _b_.
    def FXWindow.commonAncestor(a, b); end

    # Return +true+ if sibling _a_ comes before sibling _b_.
    def FXWindow.before?(a, b); end

    # Return +true+ if sibling _a_ comes after sibling _b_.
    def FXWindow.after?(a, b); end

    # Return compose context (an FXComposeContext).
    def composeContext; end
  
    # Create compose context.
    def createComposeContext; end
  
    # Destroy compose context.
    def destroyComposeContext; end

    # Return the cursor position and mouse button-state as a three-element array.
    def cursorPosition() ; end
  
    # Warp the cursor to the new position (_x_, _y_).
    def setCursorPosition(x, y); end
  
    # Return +true+ if this window is able to receive mouse and keyboard events.
    def enabled?() ; end
  
    # Return +true+ if this window is active.
    def active?() ; end
  
    # Return +true+ if this window is a control capable of receiving the focus.
    def canFocus?() ; end
  
    # Return +true+ if this window has the focus.
    def hasFocus?() ; end
    
    # Return +true+ if this window is in the focus chain.
    def inFocusChain? ; end
  
    # Move the focus to this window.
    def setFocus(); end
  
    # Remove the focus from this window.
    def killFocus(); end
  
    # Notification that focus moved to a new child window.
    def changeFocus(child); end

    # This changes the default window which responds to the *Enter*
    # key in a dialog. If _enable_ is +true+, this window becomes the default 
    # window; when _enable_ is +false+, this window will no longer be the
    # default window.  Finally, when _enable_ is +MAYBE+, the default window
    # will revert to the initial default window.
    def setDefault(enable=TRUE) ; end
    
    # Return +true+ if this is the default window.
    def default?() ; end
    
    # Make this window the initial default window.
    def setInitial(enable=true) ; end
    
    # Return +true+ if this is the initial default window.
    def initial?() ; end
  
    # Enable the window to receive mouse and keyboard events.
    def enable(); end
  
    # Disable the window from receiving mouse and keyboard events.
    def disable(); end
  
    # Create all of the server-side resources for this window.
    def create(); end
  
    # Detach the server-side resources for this window.
    def detach(); end
  
    # Destroy the server-side resources for this window.
    def destroy(); end
  
    #
    # Set window shape, where _shape_ is either an FXRegion, FXBitmap or
    # FXIcon instance.
    #
    def setShape(shape); end

    # Clear window shape
    def clearShape(); end

    # Raise this window to the top of the stacking order.
    def raiseWindow(); end
  
    # Lower this window to the bottom of the stacking order.
    def lower(); end
  
    #
    # Move the window immediately, in the parent's coordinate system.
    # Update the server representation as well if the window is realized.
    # Perform layout of the children when necessary.
    #
    def move(x, y) ; end
  
    #
    # Resize the window to the specified width and height immediately,
    # updating the server representation as well, if the window was realized.
    # Perform layout of the children when necessary.
    #
    def resize(w, h) ; end
  
    #
    # Move and resize the window immediately, in the parent's coordinate system.
    # Update the server representation as well if the window is realized.
    # Perform layout of the children when necessary.
    #
    def position(x, y, w, h); end
  
    # Mark this window's layout as dirty
    def recalc(); end
  
    # Perform layout immediately.
    def layout(); end

    # Generate a SEL_UPDATE message for the window and its children.
    def forceRefresh(); end
  
    # Reparent this window under new _father_ window, before _other_ sibling..
    def reparent(father, other); end
  
    # Scroll rectangle (_x_, _y_, _w_, _h_) by a shift of (_dx_, _dy_)
    def scroll(x, y, w, h, dx, dy); end
  
    # Mark the entire window client area dirty.
    def update() ; end

    # Mark the specified rectangle dirty
    def update(x, y, w, h) ; end
  
    # Process any outstanding repaint messages immediately, for the given rectangle
    def repaint(x, y, w, h) ; end
    
    # If marked but not yet painted, paint the entire window
    def repaint() ; end
  
    # Grab the mouse to this window; future mouse events will be
    # reported to this window even while the cursor goes outside of this window
    def grab() ; end
  
    # Release the mouse grab 
    def ungrab(); end
  
    # Return +true+ if the window has been grabbed
    def grabbed?() ; end
  
    # Grab keyboard device
    def grabKeyboard(); end
  
    # Ungrab keyboard device
    def ungrabKeyboard(); end
  
    # Return +true+ if active grab is in effect
    def grabbedKeyboard?() ; end
  
    # Show this window 
    def show(); end
  
    # Hide this window 
    def hide(); end
  
    # Return +true+ if this window is shown.
    def shown?() ; end
    
    alias visible? shown?
  
    # Return +true+ if this window is a composite.
    def composite?() ; end
  
    # Return +true+ if this window is under the cursor
    def underCursor?() ; end
  
    # Return +true+ if this window owns the primary selection
    def hasSelection?() ; end
  
    #
    # Try to acquire the primary selection, given an array of drag types.
    # Returns +true+ on success.
    #
    def acquireSelection(typesArray) ; end
  
    #
    # Release the primary selection. Returns +true+ on success.
    #
    def releaseSelection(); end
  
    # Return +true+ if this window owns the clipboard
    def hasClipboard?() ; end
  
    #
    # Try to acquire the clipboard, given an array of drag types.
    # Returns +true+ on success.
    #
    def acquireClipboard(typesArray) ; end
  
    #
    # Release the clipboard. Returns +true+ on success.
    #
    def releaseClipboard(); end
  
    # Enable this window to receive drops 
    def dropEnable(); end
  
    # Disable this window from receiving drops
    def dropDisable(); end
  
    # Return +true+ if this window is able to receive drops
    def dropEnabled?() ; end
  
    # Return +true+ if a drag operation has been initiated from this window
    def dragging?() ; end
    
    # Initiate a drag operation with an array of previously registered drag types
    def beginDrag(typesArray) ; end
    
    # When dragging, inform the drop target of the new position and
    # the drag action. The _action_ is a constant, one of:
    #
    # +DRAG_REJECT+::	reject all drop actions
    # +DRAG_ACCEPT+::	accept any drop action
    # +DRAG_COPY+::	accept this drop as a copy
    # +DRAG_MOVE+::	accept this drop as a move
    # +DRAG_LINK+::	accept this drop as a link
    # +DRAG_PRIVATE+::	private

    def handleDrag(x, y, action=DRAG_COPY) ; end
    
    #
    # Terminate the drag operation with or without actually dropping the data.
    # Return the action performed by the target.
    #
    def endDrag(drop=true); end
    
    # Return +true+ if this window is the target of a drop
    def dropTarget?() ; end
    
    # When being dragged over, indicate that no further +SEL_DND_MOTION+ messages
    # are required while the cursor is inside the given rectangle
    def setDragRectangle(x, y, w, h, wantUpdates=true); end
    
    # When being dragged over, indicate we want to receive +SEL_DND_MOTION+ messages
    # every time the cursor moves
    def clearDragRectangle();
    
    # When being dragged over, indicate acceptance or rejection of the dragged data.
    # The _action_ is a constant indicating the suggested drag action, one of:
    #
    # +DRAG_REJECT+::	reject all drop actions
    # +DRAG_ACCEPT+::	accept any drop action
    # +DRAG_COPY+::	accept this drop as a copy
    # +DRAG_MOVE+::	accept this drop as a move
    # +DRAG_LINK+::	accept this drop as a link
    # +DRAG_PRIVATE+::	private

    def acceptDrop(action=DRAG_ACCEPT); end
    
    # Returns +DRAG_REJECT+ when the drop target would not accept the drop;
    # otherwise indicates acceptance by returning one of +DRAG_ACCEPT+,
    # +DRAG_COPY+, +DRAG_MOVE+ or +DRAG_LINK+.
    def didAccept() ; end
    
    #
    # Sent by the drop target in response to +SEL_DND_DROP+.  The drag action
    # should be the same as the action the drop target reported to the drag
    # source in reponse to the +SEL_DND_MOTION+ message.
    # This function notifies the drag source that its part of the drop transaction
    # is finished, and that it is free to release any resources involved in the
    # drag operation.
    # Calling #dropFinished is advisable in cases where the drop target needs
    # to perform complex processing on the data received from the drag source,
    # prior to returning from the +SEL_DND_DROP+ message handler.
    #
    def dropFinished(action=DRAG_REJECT); end

    # When being dragged over, inquire the drag types being offered.
    # The _origin_ is a constant indicating the origin of the data, one of
    # +FROM_SELECTION+, +FROM_CLIPBOARD+ or +FROM_DRAGNDROP+.
    # Returns an array of drag types.
    def inquireDNDTypes(origin) ; end
    
    # When being dragged over, return +true+ if we are offered the given drag type.
    # The _origin_ is a constant indicating the origin of the data, one of
    # +FROM_SELECTION+, +FROM_CLIPBOARD+ or +FROM_DRAGNDROP+.
    # The _type_ is a previously registered drag type.
    def offeredDNDType?(origin, type) ; end
    
    # When being dragged over, return the drag action
    def inquireDNDAction() ; end
    
    # Get DND data; the caller becomes the owner of the array.
    # The _origin_ is a constant indicating the origin of the data, one of
    # +FROM_SELECTION+, +FROM_CLIPBOARD+ or +FROM_DRAGNDROP+.
    # The _type_ is a previously registered drag type.
    def getDNDData(origin, type) ; end
    
    # Set DND data; ownership is transferred to the system.
    # The _origin_ is a constant indicating the origin of the data, one of
    # +FROM_SELECTION+, +FROM_CLIPBOARD+ or +FROM_DRAGNDROP+.
    # The _type_ is a previously registered drag type.
    def setDNDData(origin, type, data) ; end
    
    # Return +true+ if this window logically contains the given point (_parentX_, _parentY_).
    def contains?(parentX, parentY) ; end
    
    # Translate coordinates (_fromX_, _fromY_) from _fromWindow_'s coordinate system
    # to this window's coordinate system. Returns a two-element array containing the
    # coordinates in this window's coordinate system.
    def translateCoordinatesFrom(fromWindow, fromX, fromY) ; end
    
    # Translate coordinates (_fromX_, _fromY_) from this window's coordinate system
    # to _toWindow_'s coordinate system. Returns a two-element array containing the
    # coordinates in _toWindow_'s coordinate system.
    def translateCoordinatesTo(toWindow, fromX, fromY) ; end
 
    # Return +true+ if this window does save-unders.
    def doesSaveUnder?() ; end
  
    #
    # Translate message for localization; using the current FXTranslator,
    # an attempt is made to translate the given message into the current
    # language.  An optional hint may be passed to break any ties in case
    # more than one tranlation is possible for the given message text.
    # In addition, the name of the widget is passed as context name so
    # that controls in a single dialog may be grouped together.
    #
    def tr(message, hint=nil); end
  end
end
