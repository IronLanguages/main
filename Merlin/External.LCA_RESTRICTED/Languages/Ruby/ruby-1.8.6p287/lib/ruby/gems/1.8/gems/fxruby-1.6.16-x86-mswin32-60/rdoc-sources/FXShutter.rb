module Fox
  #
  # A Shutter Item is a panel which is embedded inside a Shutter Widget.
  # It can contain other user interface widgets which can be added under
  # the content widget.  The content widget is itself embedded in a scroll
  # window to allow unlimited room for all the contents.
  #
  # === Message identifiers
  #
  # +ID_SHUTTERITEM_BUTTON+::	x
  #
  class FXShutterItem < FXVerticalFrame
    #
    # The button for this shutter item [FXButton]
    #
    attr_reader :button
    
    # The contents for this shutter item [FXVerticalFrame]
    attr_reader :content
    
    # Status line help text for this shutter item [String]
    attr_accessor :helpText

    # Tool tip message for this shutter item [String]
    attr_accessor :tipText

    #
    # Return an initialized FXShutterItem instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent shutter for this shutter item [FXShutter]
    # +text+::	the text, if any [String]
    # +icon+::	the icon, if any [FXIcon]
    # +opts+::	options [Integer]
    # +x+::	initial x-position, when the +LAYOUT_FIX_X+ layout hint is in effect [Integer]
    # +y+::	initial y-position, when the +LAYOUT_FIX_Y+ layout hint is in effect [Integer]
    # +width+::	initial width, when the +LAYOUT_FIX_WIDTH+ layout hint is in effect [Integer]
    # +height+::	initial height, when the +LAYOUT_FIX_HEIGHT+ layout hint is in effect [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    # +hSpacing+::	horizontal spacing between widgets, in pixels [Integer]
    # +vSpacing+::	vertical spacing between widgets, in pixels [Integer]
    #
    def initialize(p, text="", icon=nil, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theShutterItem
    end
  end

  #
  # The Shutter widget provides a set of foldable sub panels.  Each subpanel
  # consists of a Shutter Item which contains a button and some contents.
  # A sub panel can be unfolded by pressing on that panel's button.
  #
  # === Events
  #
  # The following messages are sent by FXShutter to its target:
  #
  # +SEL_COMMAND+::
  #   sent whenever a new shutter item is opened; the message data is an integer
  #   indicating the new currently displayed shutter item.
  #
  # === Message identifiers
  #
  # +ID_SHUTTER_TIMEOUT+::	x
  # +ID_OPEN_SHUTTERITEM+::	x
  # +ID_OPEN_FIRST+::		x
  # +ID_OPEN_LAST+::		x
  #
  class FXShutter < FXVerticalFrame
  
    #
    # The currently displayed shutter item (a zero-based index) [Integer]
    #
    attr_accessor :current

    #
    # Return an initialized FXShutter instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this shutter [FXComposite]
    # +target+::	the message target, if any, for this shutter [FXObject]
    # +selector+::	the message identifier for this shutter [Integer]
    # +opts+::	shutter options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    # +hSpacing+::	horizontal spacing between widgets, in pixels [Integer]
    # +vSpacing+::	vertical spacing between widgets, in pixels [Integer]
    #
    def initialize(p, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theShutter
    end
  end
end

