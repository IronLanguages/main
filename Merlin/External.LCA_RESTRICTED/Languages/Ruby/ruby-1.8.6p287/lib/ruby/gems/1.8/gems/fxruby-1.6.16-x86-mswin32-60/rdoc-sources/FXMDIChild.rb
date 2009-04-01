module Fox
  #
  # The MDI child window contains the application work area in a Multiple Document
  # Interface application.  GUI Controls are connected to the MDI child via delegation
  # through the MDI client, which forwards messages it receives to the active MDI child.
  # The MDI child itself tries to further delegate messages to its single content window,
  # and if not handled there, to its target object.
  # When the MDI child is maximized, it sends a SEL_MAXIMIZE message; when the MDI
  # child is minimized, it sends a SEL_MINIMIZE message.  When it is restored, it
  # sends a SEL_RESTORE message to its target.  The MDI child also notifies its
  # target when it becomes the active MDI child, via the SEL_SELECTED message.
  # The void* in the SEL_SELECTED message refers to the previously active MDI child,
  # if any.  When an MDI child ceases to be the active one, a SEL_DESELECTED message
  # is sent.  The void* in the SEL_DESELECTED message refers to the newly activated
  # MDI child, if any.  Thus, interception of SEL_SELECTED and SEL_DESELECTED allows
  # the target object to determine whether the user switched between MDI windows of
  # the same document (target) or between MDI windows belonging to the same document.
  # When the MDI child is closed, it sends a SEL_CLOSE message to its target.
  # The target has an opportunity to object to the closing; if the MDI child should
  # not be closed, it should return 1 (objection). If the MDI child should be closed,
  # the target can either just return 0 or simply not handle the SEL_CLOSE message.
  # The SEL_UPDATE message can be used to modify the MDI child's title (via
  # ID_SETSTRINGVALUE), and window icon (via ID_SETICONVALUE).
  #
  # === Events
  #
  # The following messages are sent by FXMDIChild to its target:
  #
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONPRESS+::	sent when the middle mouse button goes down; the message data is an FXEvent instance.
  # +SEL_MIDDLEBUTTONRELEASE+::	sent when the middle mouse button goes up; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONPRESS+::	sent when the right mouse button goes down; the message data is an FXEvent instance.
  # +SEL_RIGHTBUTTONRELEASE+::	sent when the right mouse button goes up; the message data is an FXEvent instance.
  # +SEL_SELECTED+::
  #   sent when the window is selected; the message data is a reference to the MDI client's previous active
  #   child window, or +nil+ if there was no active child window.
  # +SEL_DESELECTED+::
  #   sent when the window is deselected; the message data is a reference to the MDI client's new active child window,
  #   or +nil+ if there is no active child window.
  # +SEL_MAXIMIZE+::		sent when the window is maximized
  # +SEL_MINIMIZE+::		sent when the window is minimized
  # +SEL_RESTORE+::		sent when the window is restored to its normal size and position
  # +SEL_CLOSE+::
  #   sent when the user is trying to close this window. The message handler for this message should
  #   return 1 (or true) if the target objects to closing the window; otherwise it should just return false (or zero).
  # +SEL_DELETE+::		sent immediately before this window is destroyed
  #
  # === MDI Child Window styles
  #
  # +MDI_NORMAL+::	Normal display mode
  # +MDI_MAXIMIZED+::	Window appears maximized
  # +MDI_MINIMIZED+::	Window is iconified or minimized
  # +MDI_TRACKING+::	Track continuously during dragging
  #
  class FXMDIChild < FXComposite

    # Normal (restored) position x-coordinate [Integer]
    attr_accessor :normalX

    # Normal (restored) position y-coordinate [Integer]
    attr_accessor :normalY

    # Normal (restored) width [Integer]
    attr_accessor :normalWidth

    # Normal (restored) height [Integer]
    attr_accessor :normalHeight

    # Iconified position x-coordinate [Integer]
    attr_accessor :iconX

    # Iconified position y-coordinate [Integer]
    attr_accessor :iconY

    # Iconified width [Integer]
    attr_accessor :iconWidth

    # Iconified height [Integer]
    attr_accessor :iconHeight
    
    # Content window [FXWindow]
    attr_reader :contentWindow
    
    # Window title [String]
    attr_accessor :title

    # Highlight color [FXColor]
    attr_accessor :hiliteColor

    # Shadow color [FXColor]
    attr_accessor :shadowColor

    # Base color [FXColor]
    attr_accessor :baseColor

    # Border color [FXColor]
    attr_accessor :borderColor

    # Title color [FXColor]
    attr_accessor :titleColor

    # Title background color [FXColor]
    attr_accessor :titleBackColor
    
    # Window icon [FXIcon]
    attr_accessor :icon
    
    # Window menu [FXPopup]
    attr_accessor :menu
    
    # Title font [FXFont]
    attr_accessor :font

    # Construct MDI Child window with given name and icon
    def initialize(p, name, ic=nil, pup=nil, opts=0, x=0, y=0, width=0, height=0) # :yields: theMDIChild
    end
  
    #
    # Minimize this window.
    # If _notify_ is +true+, ...
    #
    def minimize(notify=false); end

    #
    # Maximize this window.
    # If _notify_ is +true+, ...
    #
    def maximize(notify=false); end

    #
    # Restore this window to its normal position and size.
    # If _notify_ is +true+, ...
    #
    def restore(notify=false); end

    # Return +true+ if maximized
    def maximized? ; end
    
    # Return +true+ if minimized
    def minimized? ; end
    
    # Set tracking instead of just outline
    def setTracking(tracking); end
    
    alias tracking= setTracking
    
    # Return +true+ if tracking, +false+ otherwise.
    def getTracking(); end
    
    alias isTracking? getTracking
    alias tracking    getTracking

  end
end
