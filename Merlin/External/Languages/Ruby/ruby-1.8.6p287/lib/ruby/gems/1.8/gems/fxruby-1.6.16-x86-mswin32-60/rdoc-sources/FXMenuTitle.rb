module Fox
  #
  # A menu title is a child of a menu bar which is responsible
  # for popping up a pulldown menu.
  #
  # === Events
  #
  # The following messages are sent by FXMenuTitle to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  class FXMenuTitle < FXMenuCaption

    # The popup menu [FXPopup]
    attr_accessor :menu

    #
    # Constructor
    #
    def initialize(parent, text, icon=nil, popupMenu=nil, opts=0) # :yields: theMenuTitle
    end
  end
end

