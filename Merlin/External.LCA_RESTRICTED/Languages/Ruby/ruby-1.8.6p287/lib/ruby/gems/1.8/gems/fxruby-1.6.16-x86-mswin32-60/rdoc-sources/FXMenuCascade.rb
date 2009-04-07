module Fox
  #
  # The cascade menu widget is used to bring up a sub menu from a
  # pull down menu.
  #
  class FXMenuCascade < FXMenuCaption

    # The popup menu [FXPopup]
    attr_accessor :menu

    #
    # Construct a menu cascade responsible for the given popup menu
    #
    def initialize(parent, text, icon=nil, popupMenu=nil, opts=0) # :yields: theMenuCascade
    end
  end
end

