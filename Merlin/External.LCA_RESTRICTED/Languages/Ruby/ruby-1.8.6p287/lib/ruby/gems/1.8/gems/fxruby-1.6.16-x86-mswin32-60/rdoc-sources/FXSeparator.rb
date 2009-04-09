module Fox
  #
  # A Separator widget is used to draw a horizontal or vertical divider between
  # groups of controls.  It is purely decorative.  The separator may be drawn
  # in various styles as determined by the SEPARATOR_NONE, SEPARATOR_GROOVE,
  # SEPARATOR_RIDGE, and SEPARATOR_LINE options.  Since its derived from Frame,
  # it can also have the frame's border styles.
  #
  # === Separator options
  #
  # +SEPARATOR_NONE+::		Nothing visible
  # +SEPARATOR_GROOVE+::	Etched-in looking groove
  # +SEPARATOR_RIDGE+::		Embossed looking ridge
  # +SEPARATOR_LINE+::		Simple line
  #
  class FXSeparator < FXFrame

    # Separator style, one of SEPARATOR_NONE, SEPARATOR_GROOVE, SEPARATOR_RIDGE or SEPARATOR_LINE [Integer]
    attr_accessor :separatorStyle

    # Return an initialized FXSeparator instance.
    def initialize(p, opts=SEPARATOR_GROOVE|LAYOUT_FILL_X, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=0, padBottom=0) # :yields: theSeparator
    end
  end
    
  #
  # Horizontal separator
  #
  class FXHorizontalSeparator < FXSeparator
    #
    # Return an initialized FXHorizontalSeparator instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this separator [FXComposite]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, opts=SEPARATOR_GROOVE|LAYOUT_FILL_X, x=0, y=0, width=0, height=0, padLeft=1, padRight=1, padTop=0, padBottom=0) # :yields: theHorizontalSeparator
    end
  end

  #
  # Vertical separator
  #
  class FXVerticalSeparator < FXSeparator
    #
    # Return an initialized FXVerticalSeparator instance.
    #
    # ==== Parameters:
    #
    # +p+::	the parent widget for this separator [FXComposite]
    # +opts+::	the options [Integer]
    # +x+::	initial x-position [Integer]
    # +y+::	initial y-position [Integer]
    # +width+::	initial width [Integer]
    # +height+::	initial height [Integer]
    # +padLeft+::	internal padding on the left side, in pixels [Integer]
    # +padRight+::	internal padding on the right side, in pixels [Integer]
    # +padTop+::	internal padding on the top side, in pixels [Integer]
    # +padBottom+::	internal padding on the bottom side, in pixels [Integer]
    #
    def initialize(p, opts=SEPARATOR_GROOVE|LAYOUT_FILL_Y, x=0, y=0, width=0, height=0, padLeft=0, padRight=0, padTop=1, padBottom=1) # :yields: theVerticalSeparator
    end
  end
end

