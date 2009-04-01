module Fox
  #
  # A toolbar tab is used to collapse or uncollapse a sibling
  # widget. The sibling affected is the widget immediately following
  # the toolbar tab or, if the toolbar tab is the last widget in the list,
  # the widget immediately preceding the toolbar tab.
  # Typically, the toolbar tab is paired with just one sibling widget
  # inside a paired container, e.g.
  #
  #     FXHorizontalFrame.new(...) do |p|
  #       FXToolBarTab.new(p)
  #       FXLabel.new(p, "Hideable label", nil, LAYOUT_FILL_X)
  #     end
  #
  # === Events
  #
  # The following messages are sent by FXToolBarTab to its target:
  #
  # +SEL_KEYPRESS+::	Sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::	Sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_COMMAND+::	Sent after the toolbar tab is collapsed (or uncollapsed). The message data indicates the new collapsed state (i.e. it's +true+ if the toolbar tab is now collapsed, +false+ if it is now uncollapsed).
  #
  # === Toolbar tab styles
  #
  # +TOOLBARTAB_HORIZONTAL+::		Default is for horizontal toolbar
  # +TOOLBARTAB_VERTICAL+::		For vertical toolbar
  #
  # === Message identifiers
  # 
  # +ID_COLLAPSE+::					Collapse the toolbar tab
  # +ID_UNCOLLAPSE+::				Uncollapse the toolbar tab
  #
  class FXToolBarTab < FXFrame

    # The tab style [Integer]
    attr_accessor :tabStyle

    # The active color [FXColor]
    attr_accessor :activeColor
    
    # Tooltip message [String]
    attr_accessor :tipText

    #
    # Return an initialized FXToolBarTab instance.
    #
    # ==== Parameters:
    #
    # +p+::		the parent window for this toolbar tab [FXWindow]
    # +target+::	the message target [FXObject]
    # +selector+::	the message identifier [Integer]
    # +opts+::	the options [Integer]
    # +x+::		x-coordinate of window upper left corner [Integer]
    # +y+::		y-coordinate of window upper left corner [Integer]
    # +width+::		window width [Integer]
    # +height+::		window height [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=FRAME_RAISED, x=0, y=0, width=0, height=0) # :yield: theToolBarTab
    end

    #
    # Collapse (if _fold_ is +true+) or uncollapse the toolbar.
    # If _notify_ is +true+, a +SEL_COMMAND+ message is sent to the toolbar
    # tab's message target after the toolbar tab is collapsed (or uncollapsed).
    #
    def collapse(fold, notify=false); end
  
    #
    # Return +true+ if the toolbar is collapsed, +false+ otherwise.
    #
    def collapsed?; end
  end
end

