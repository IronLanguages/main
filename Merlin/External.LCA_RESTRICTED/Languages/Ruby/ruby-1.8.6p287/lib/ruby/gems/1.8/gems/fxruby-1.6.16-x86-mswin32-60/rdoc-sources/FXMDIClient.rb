module Fox
  #
  # The MDI client window manages a number of MDI child windows in a multiple-document
  # interface (MDI) application. MDI child windows usually receive messages from the GUI controls
  # by delegation via the MDI client.  This is accomplished by making the MDI client window
  # the target for most GUI controls.  The MDI client filters out messages intented for itself,
  # and delegates the remaining messages to its currently active MDI child, if any.
  # If you use the auto-gray or auto-hide feature available in some GUI controls, these
  # controls can be automatically grayed out or hidden when there is no active MDI child.
  # When delegating messages via MDI client to MDI child windows of different types, care
  # should be taken that message ID's do not overlap, so that all message ID's only map to
  # the intented handlers no matter which MDI child window type is active.
  # The MDI client sends a SEL_CHANGED message to its target when the active MDI child is
  # switched, with the void  # pointer refering to the new MDI child.
  # A MDI Window selection dialog can be brought up through the ID_MDI_OVER_X messages;
  # a menu button connected to the MDI client with the ID_MDI_OVER_X message will be
  # automatically grayed out if there are less than X MDI child windows.
  #
  # === Events
  #
  # The following messages are sent by FXMDIClient to its target:
  #
  # +SEL_CHANGED+::
  #   sent when the active child changes; the message data is a reference to the new active child window (or +nil+ if there is none)
  #
  class FXMDIClient < FXComposite
  
    # Active MDI child window, or +nil+ if none [FXMDIChild].
    attr_accessor :activeChild
  
    # Cascade offset X [Integer]
    attr_accessor :cascadeX
  
    # Cascade offset Y [Integer]
    attr_accessor :cascadeY

    # Construct MDI Client window
    def initialize(p, opts=0, x=0, y=0, width=0, height=0) # :yields: theMDIClient
    end

    #
    # Pass message to all MDI windows, stopping when one of
    # the MDI windows fails to handle the message.
    #
    def forallWindows(sender, sel, ptr); end
  
    #
    # Pass message once to all MDI windows with the same document,
    # stopping when one of the MDI windows fails to handle the message.
    #
    def forallDocuments(sender, sel, ptr); end

    #
    # Pass message to all MDI Child windows whose target is _document_,
    # stopping when one of the MDI windows fails to handle the message.
    #
    def forallDocWindows(document, sender, sel, ptr); end
  
    #
    # Set active MDI child window for this MDI client to _child_.
    #
    def setActiveChild(child=nil, notify=true); end
  end
end

