module Fox
  #
  # Vertical frame layout manager widget is used to automatically
  # place child-windows vertically from top-to-bottom, or bottom-to-top,
  # depending on the child window's layout hints.
  #
  class FXVerticalFrame < FXPacker
    #
    # Return an initialized FXVerticalFrame instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this vertical frame [FXComposite]
    # +opts+::	frame options [Integer]
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
    def initialize(p, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theVerticalFrame
    end
  end
end
