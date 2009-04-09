module Fox
  #
  # The horizontal frame layout manager widget is used to automatically
  # place child-windows horizontally from left-to-right, or right-to-left,
  # depending on the child windows' layout hints.
  #
  class FXHorizontalFrame < FXPacker
    #
    # Return an initialized FXHorizontalFrame instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent window for this horizontal frame [FXComposite]
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
    def initialize(p, opts=0, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theHorizontalFrame
    end
  end
end
