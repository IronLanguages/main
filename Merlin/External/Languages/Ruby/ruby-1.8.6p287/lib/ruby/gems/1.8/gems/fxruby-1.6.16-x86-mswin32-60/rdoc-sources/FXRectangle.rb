module Fox
  #
  # Rectangle
  #
  class FXRectangle

    #
    # Upper left corner's x-coordinate [Integer]
    #
    attr_accessor :x
    
    # Upper left corner's y-coordinate [Integer]
    attr_accessor :y
    
    # Rectangle width [Integer]
    attr_accessor :w
    
    # Rectangle height [Integer]
    attr_accessor :h
  
    #
    # Return an uninitialized FXRectangle instance.
    #
    def initialize; end
    
    #
    # Return an initialized FXRectangle instance.
    #
    # ==== Parameters:
    #
    # +xx+::	upper left corner's initial x-coordinate [Integer]
    # +yy+::	upper left corner's initial y-coordinate [Integer]
    # +ww+::	initial width [Integer]
    # +hh+::	initial height [Integer]
    #
    def initialize(xx, yy, ww, hh); end
    
    #
    # Return an initialized FXRectangle instance.
    #
    # ==== Parameters:
    #
    # +p+::	upper left corner's initial position [FXPoint]
    # +s+::	initial size [FXSize]
    #
    def initialize(p, s); end

    #
    # Return an initialized FXRectangle instance.
    #
    # ==== Parameters:
    #
    # +topleft+::	upper left corner's initial position [FXPoint]
    # +bottomright+::	bottom right corner's initial position [FXPoint]
    #
    def initialize(topleft, bottomright); end
  
    #
    # Return true if _p_ (an FXPoint instance) is contained within this rectangle.
    #
    def contains?(p); end
    
    #
    # Return true if the point at (_xx_, _yy_) is contained within this rectangle.
    #
    def contains?(xx, yy); end
  
    #
    # Return true if _r_ (another FXRectangle instance) is properly contained within
    # this rectangle.
    #
    def contains?(r); end
  
    #
    # Shift each of the rectangle's corners by the amount _p_ (an FXPoint
    # instance) and return a reference to the rectangle.
    #
    def move!(p); end
    
    #
    # Shift each of the rectangle's corners by the amount (_dx_, _dy_)
    # and return a reference to the rectangle.
    #
    def move!(dx, dy); end

    #
    # Grow the rectangle by some amount and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +margin+::	number of units to grow on each side [Integer]
    #
    def grow!(margin); end

    #
    # Grow the rectangle by some amount and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +hormargin+::	number of units to grow on the left and right sides [Integer]
    # +vermargin+::	number of units to grow on the top and bottom sides [Integer]
    #
    def grow!(hormargin, vermargin); end

    #
    # Grow the rectangle by some amount and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +leftmargin+::	number of units to grow on the left side [Integer]
    # +rightmargin+::	number of units to grow on the right side [Integer]
    # +topmargin+::	number of units to grow on the top side [Integer]
    # +bottommargin+::	number of units to grow on the bottom side [Integer]
    #
    def grow!(leftmargin, rightmargin, topmargin, bottommargin); end
  
    #
    # Shrink the rectangle by _margin_ units, and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +margin+::	number of units to shrink on each side [Integer]
    #
    def shrink!(margin); end

    #
    # Shrink the rectangle by some amount, and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +hormargin+::	number of units to shrink on the left and right sides [Integer]
    # +vermargin+::	number of units to shrink on the top and bottom sides [Integer]
    #
    def shrink!(hormargin, vermargin); end

    #
    # Shrink the rectangle by some amount, and return a reference to the rectangle.
    #
    # ==== Parameters:
    #
    # +leftmargin+::	number of units to shrink on the left side [Integer]
    # +rightmargin+::	number of units to shrink on the right side [Integer]
    # +topmargin+::	number of units to shrink on the top side [Integer]
    # +bottommargin+::	number of units to shrink on the bottom side [Integer]
    #
    def shrink!(leftmargin, rightmargin, topmargin, bottommargin); end
  end
end

