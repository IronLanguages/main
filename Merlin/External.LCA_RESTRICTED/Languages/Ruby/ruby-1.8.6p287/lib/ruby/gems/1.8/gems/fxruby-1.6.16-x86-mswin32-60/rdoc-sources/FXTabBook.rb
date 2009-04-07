module Fox
  #
  # The tab book layout manager arranges pairs of children;
  # the even numbered children (0,2,4,...) are usually tab items,
  # and are placed on the top.  The odd numbered children are
  # usually layout managers, and are placed below; all the odd
  # numbered children are placed on top of each other, similar
  # to the switcher widget.  When the user presses one of the
  # tab items, the tab item is raised above the neighboring tabs,
  # and the corresponding panel is raised to the top.
  # Thus, a tab book can be used to present many GUI controls
  # in a small space by placing several panels on top of each
  # other and using tab items to select the desired panel.
  # When one of the tab items is pressed, the tab book's #setCurrent method
  # is called with _notify+=true. This causes the tab book to send a
  # +SEL_COMMAND+ message to its target.
  #
  class FXTabBook < FXTabBar
    #
    # Return an initialized FXTabBook instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tar book [FXComposite]
    # +target+::	the message target, if any, for this tar book [FXObject]
    # +selector+::	the message identifier for this tab book [Integer]
    # +opts+::	tar book options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=TABBOOK_NORMAL, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING) # :yields: theTabBook
    end
  end
end

