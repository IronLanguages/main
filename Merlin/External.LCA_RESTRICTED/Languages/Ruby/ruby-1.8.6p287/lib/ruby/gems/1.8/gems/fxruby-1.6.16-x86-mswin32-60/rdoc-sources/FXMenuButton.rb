module Fox
  #
  # An FXMenuButton posts a popup menu when clicked.
  # There are many ways to control the placement where the popup will appear;
  # first, the popup may be placed on either of the four sides relative to the
  # menu button; this is controlled by the flags +MENUBUTTON_DOWN+, etc.
  # Next, there are several attachment modes; the popup's left/bottom edge may
  # attach to the menu button's left/top edge, or the popup's right/top edge may
  # attach to the menu button's right/bottom edge, or both. 
  # Also, the popup may appear centered relative to the menu button.
  # Finally, a small offset may be specified to displace the location of the
  # popup by a few pixels so as to account for borders and so on. 
  # Normally, the menu button shows an arrow pointing to the direction where
  # the popup is set to appear; this can be turned off by passing the option
  # +MENUBUTTON_NOARROWS+.
  #
  # === Events
  #
  # The following messages are sent by FXMenuButton to its target:
  #
  # +SEL_KEYPRESS+::		sent when a key goes down; the message data is an FXEvent instance.
  # +SEL_KEYRELEASE+::		sent when a key goes up; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONPRESS+::	sent when the left mouse button goes down; the message data is an FXEvent instance.
  # +SEL_LEFTBUTTONRELEASE+::	sent when the left mouse button goes up; the message data is an FXEvent instance.
  #
  # === Menu Button Style Flags
  #
  # Any combination of the following flags can be assigned as the menu
  # button style flags.
  #
  # +MENUBUTTON_AUTOGRAY+::		Automatically gray out when no target
  # +MENUBUTTON_AUTOHIDE+::		Automatically hide when no target
  # +MENUBUTTON_TOOLBAR+::		Toolbar style
  # +MENUBUTTON_NOARROWS+::		Do not show arrows
  #
  # === Menu Button Popup Style
  #
  # Any one of the following options can be assigned as the menu
  # button's popup style.
  #
  # +MENUBUTTON_DOWN+::			Popup window appears below menu button
  # +MENUBUTTON_UP+::			Popup window appears above menu button
  # +MENUBUTTON_LEFT+::			Popup window to the left of the menu button
  # +MENUBUTTON_RIGHT+::		Popup window to the right of the menu button
  #
  # === Menu Button Attachment
  #
  # Any combination of the following flags can be assigned as the menu
  # button's attachment flags.
  #
  # +MENUBUTTON_ATTACH_LEFT+::		Popup attaches to the left side of the menu button
  # +MENUBUTTON_ATTACH_TOP+::		Popup attaches to the top of the menu button
  # +MENUBUTTON_ATTACH_RIGHT+::		Popup attaches to the right side of the menu button
  # +MENUBUTTON_ATTACH_BOTTOM+::	Popup attaches to the bottom of the menu button
  # +MENUBUTTON_ATTACH_CENTER+::	Popup attaches to the center of the menu button
  # +MENUBUTTON_ATTACH_BOTH+::		Popup attaches to both sides of the menu button
  #
  class FXMenuButton < FXLabel

    # The popup menu [FXPopup]
    attr_accessor :menu
    
    # X-offset where menu pops up relative to button [Integer]
    attr_accessor :xOffset
    
    # Y-offset where menu pops up relative to button [Integer]
    attr_accessor :yOffset
    
    # Menu button style [Integer]
    attr_accessor :buttonStyle
    
    # Popup style [Integer]
    attr_accessor :popupStyle
    
    # Attachment [Integer]
    attr_accessor :attachment

    #
    # Constructor
    #
    def initialize(parent, text, icon=nil, popupMenu=nil, opts=JUSTIFY_NORMAL|ICON_BEFORE_TEXT|MENUBUTTON_DOWN, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING) # :yields: theMenuButton
  end
end

