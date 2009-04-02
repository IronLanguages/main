module Fox
  # Canvas, an area drawn by another object
  #
  # === Events
  #
  # The following messages are sent by FXCanvas to its target:
  #
  # +SEL_KEYPRESS+::	sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::	sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_MOTION+::	sent when the mouse moves; the message data is an FXEvent instance.
  # +SEL_PAINT+::	sent when the canvas needs to be redrawn; the message data is an FXEvent instance.

  class FXCanvas < FXWindow
    # Construct new drawing canvas widget
    def initialize(parent, target=nil, selector=0, opts=FRAME_NORMAL, x=0, y=0, width=0, height=0) # :yields: theCanvas
    end
  end
end
