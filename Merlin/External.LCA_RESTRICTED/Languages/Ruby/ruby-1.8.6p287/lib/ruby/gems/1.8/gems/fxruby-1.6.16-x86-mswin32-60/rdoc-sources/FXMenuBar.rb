module Fox
  #
  # Menu bar
  #
  class FXMenuBar < FXToolBar
    #
    # Construct a floatable menubar.
    # Normally, the menubar is docked under window _p_.
    # When floated, the menubar can be docked under window _q_, which is
    # typically an FXToolBarShell window.
    #
    def initialize(p, q, opts=LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theMenuBar
    end
  
    #
    # Construct a non-floatable menubar.
    # The menubar can not be undocked.
    #
    def initialize(p, opts, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theMenuBar
    end
  end
end

