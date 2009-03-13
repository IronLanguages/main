module Fox
  #
  # The menu caption is a widget which can be used as a caption
  # above a number of menu commands in a menu.
  #
  # === Menu caption options
  #
  # +MENU_AUTOGRAY+::	Automatically gray out when not updated
  # +MENU_AUTOHIDE+::	Automatically hide button when not updated
  #
  class FXMenuCaption < FXWindow

    # The text for this menu [String]
    attr_accessor :text
    
    # The icon for this menu [FXIcon]
    attr_accessor :icon
    
    # The text font [FXFont]
    attr_accessor :font
    
    # Text color [FXColor]
    attr_accessor :textColor
    
    # Selection background color [FXColor]
    attr_accessor :selBackColor
    
    # Selection text color [FXColor]
    attr_accessor :selTextColor
    
    # Highlight color [FXColor]
    attr_accessor :hiliteColor
    
    # Shadow color [FXColor]
    attr_accessor :shadowColor
    
    # Status line help text for this menu [String]
    attr_accessor :helpText
    
    # Tool tip message for this menu [String]
    attr_accessor :tipText

    #
    # Construct a new menu caption
    #
    def initialize(parent, text, icon=nil, opts=0) # :yields: theMenuCaption
    end
    
    # Return the menu caption's text
    def to_s; text; end
  end
end

