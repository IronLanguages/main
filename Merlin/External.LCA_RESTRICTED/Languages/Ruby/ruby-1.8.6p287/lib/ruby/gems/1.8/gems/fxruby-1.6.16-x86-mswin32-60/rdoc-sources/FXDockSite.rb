module Fox
  #
  # The dock site widget is a widget where dock bars can be docked.
  # Dock site widgets are typically embedded inside the main window, placed
  # against those sides where docking of toolbars is to be allowed.
  # Dock bars placed inside a dock site are laid out in horizontal or vertical bands
  # called _galleys_. A toolbar with the +LAYOUT_DOCK_SAME+ hint is preferentially placed
  # on the same galley as its previous sibling. A dock bar with the +LAYOUT_DOCK_NEXT+ is
  # always placed on the next galley.
  # Each galley will have at least one dock bar shown in it. Several dock bars
  # may be placed side-by-side inside one galley, unless there is insufficient
  # room. If there is insufficient room to place another dock bar, that dock bar
  # will be moved to the next galley, even though its +LAYOUT_DOCK_NEXT+ option
  # is not set. This implies that when the main window is resized, and more room
  # becomes available, it will jump back to its preferred galley.
  # Within a galley, dock bars will be placed from left to right, at the given
  # x and y coordinates, with the constraints that the dock bar will stay within
  # the galley, and do not overlap each other. It is possible to use +LAYOUT_FILL_X+
  # and/or +LAYOUT_FILL_Y+ to stretch a toolbar to the available space on its galley.
  # The galleys are oriented horizontally if the dock site is placed inside
  # a top level window using +LAYOUT_SIDE_TOP+ or +LAYOUT_SIDE_BOTTOM+, and
  # vertically oriented if placed with +LAYOUT_SIDE_LEFT+ or +LAYOUT_SIDE_RIGHT+.
  #
  # === Dock Site Options
  #
  # +DOCKSITE_WRAP+::		Dockbars are wrapped to another galley when not enough space on current galley
  # +DOCKSITE_NO_WRAP+::	Never wrap dockbars to another galley even if not enough space
  #
  class FXDockSite < FXPacker
    #
    # Construct a toolbar dock layout manager. Passing +LAYOUT_SIDE_TOP+ or +LAYOUT_SIDE_BOTTOM+
    # causes the toolbar dock to be oriented horizontally. Passing +LAYOUT_SIDE_LEFT+ or
    # +LAYOUT_SIDE_RIGHT+ causes it to be oriented vertically.
    #
    def initialize(p, opts=0, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0, hSpacing=0, vSpacing=0) # :yields: theDockSite
    end

    #
    # Move tool bar, changing its options to suit the new position.
    # Used by the toolbar dragging to rearrange the toolbars inside the
    # toolbar dock.
    #
    # ==== Parameters:
    #
    # +bar+::		a reference to the dockbar that's being dragged [FXDockBar]
    # +barx+::		current x-coordinate of the dockbar [Integer]
    # +bary+		current y-coordinate of the dockbar [Integer]
    #
    def moveToolBar(bar, barx, bary); end

    #
    # The dock site is notified that the given bar has been added
    # logically before the given window, and is to placed on a new
    # galley all by itself.  The default implementation adjusts
    # the layout options of the bars accordingly.
    #
    # ==== Parameters:
    #
    # +bar+::		a reference to the newly docked dockbar [FXDockBar]
    # +before+::	a reference to the window that the dockbar was added before [FXWindow]
    #
    def dockToolBar(bar, before); end

    #
    # The dock site is informed that the given bar has been docked
    # at the given coordinates.  The default implementation determines
    # where to insert the newly docked bar and adjusts the layout
    # options of the bars accordingly.
    #
    # ==== Parameters:
    #
    # +bar+::		a reference to the newly docked dockbar [FXDockBar][FXDockBar]
    # +barx+::		x-coordinate of the docking position [Integer]
    # +bary+::		y-coordinate of the docking position [Integer]
    #
    def dockToolBar(bar, barx, bary); end

    #
    # The dock site is informed that the given bar has been removed.
    # In the default implementation, the dock site fixes the layout
    # options of the remaining bars so they stay in the same place
    # if possible.
    #
    # ==== Parameters:
    #
    # +bar+::	a reference to the removed dockbar [FXDockBar]
    #
    def undockToolBar(bar); end
    
    #
    # If _wrap_ is +true+, allow the wrapping of dockbars (i.e. set the
    # +DOCKSITE_WRAP+ option.)
    #
    def wrapGalleys=(wrap); end
    
    #
    # Return +true+ if the +DOCKSITE_WRAP+ option is set, indicating that
    # dockbars will be wrapped to another galley if there's not enough space on
    # current galley.
    #
    def wrapGalleys?; end
  end
end

