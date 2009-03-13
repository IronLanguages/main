module Fox
  #
  # The FXSwitcher layout manager automatically arranges its child
  # windows such that one of them is placed on top; all other
  # child windows are hidden.
  # Switcher provides a convenient method to conserve screen
  # real-estate by arranging several GUI panels to appear in the 
  # same space, depending on context.
  # Switcher ignores all layout hints from its children; all
  # children are stretched according to the switcher layout
  # managers own size.
  # When the +SWITCHER_HCOLLAPSE+ or +SWITCHER_VCOLLAPSE+ options
  # are used, the switcher's default size is based on the width or
  # height of the current child, instead of the maximum width
  # or height of all of the children.
  #
  # === Events
  #
  # The following messages are sent by FXSwitcher to its target:
  #
  # +SEL_COMMAND+::
  #   sent whenever the current (topmost) child window changes;
  #   the message data is an integer indicating the new current window's index.
  #
  # === Switcher options
  #
  # +SWITCHER_HCOLLAPSE+::	Collapse horizontally to width of current child
  # +SWITCHER_VCOLLAPSE+::	Collapse vertically to height of current child
  #
  # === Message identifiers
  #
  # +ID_OPEN_FIRST+::	x
  # +ID_OPEN_SECOND+::	x
  # +ID_OPEN_THIRD+::	x
  # +ID_OPEN_FOURTH+::	x
  # +ID_OPEN_FIFTH+::	x
  # +ID_OPEN_SIXTH+::	x
  # +ID_OPEN_SEVENTH+::	x
  # +ID_OPEN_EIGHTH+::	x
  # +ID_OPEN_NINETH+::	x
  # +ID_OPEN_TENTH+::	x
  # +ID_OPEN_LAST+::	x
  #
  class FXSwitcher < FXPacker
    # Current child window's index [Integer]
    attr_accessor :current

    # Switcher style flags [Integer]
    attr_accessor :switcherStyle

    #
    # Return an initialized FXSwitcher instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this switcher [FXComposite]
    # +opts+::	switcher options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING) # :yields: theSwitcher
    end
  
    #
    # Raise the child window at _index_ to the top of the stack.
    # If _notify_ is +true+, a +SEL_COMMAND+ message is sent to the switcher's message target
    # Raises IndexError if _index_ is out of bounds.
    #
    def setCurrent(index, notify=false); end
  end
end

