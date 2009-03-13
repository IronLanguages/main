module Fox
  #
  # A tool bar widget can be docked in a dock site; it automatically
  # adjusts its orientation based on the orientation of the dock site,
  # and adjusts the layout options accordingly.
  # See FXDockBar widget for more information on the docking behavior.
  #
  class FXToolBar < FXDockBar

    # Docking side, one of +LAYOUT_SIDE_LEFT+, +LAYOUT_SIDE_RIGHT+, +LAYOUT_SIDE_TOP+ or +LAYOUT_SIDE_BOTTOM+ [Integer]
    attr_accessor :dockingSide

    #
    # Return an initialized, floatable FXToolBar instance.
    # Normally, the tool bar is docked under window _p_.
    # When floated, the tool bar can be docked under window _q_, which is
    # typically an FXToolBarShell window.
    #
    # ==== Parameters:
    #
    # +p+::	the "dry dock" window for this tool bar [FXComposite]
    # +q+::	the "wet dock" window for this tool bar [FXComposite]
    # +opts+::	tool bar options [Integer]
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
    def initialize(p, q, opts=LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theToolBar
    end
  
    #
    # Return an initialized, stationary FXToolBar instance.
    # The tool bar can not be undocked.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this tool bar [FXComposite]
    # +opts+::	tool bar options [Integer]
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
    def initialize(p, opts=LAYOUT_TOP|LAYOUT_LEFT|LAYOUT_FILL_X, x=0, y=0, width=0, height=0, padLeft=3, padRight=3, padTop=2, padBottom=2, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theToolBar
    end
  end
end

