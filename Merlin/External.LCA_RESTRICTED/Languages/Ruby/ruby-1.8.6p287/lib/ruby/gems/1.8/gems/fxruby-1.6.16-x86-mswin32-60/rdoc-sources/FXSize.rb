module Fox
  #
  # Size
  #
  class FXSize
    # Width [Integer]
    attr_accessor :w
    
    # Height [Integer]
    attr_accessor :h

    #
    # Return an uninitialized FXSize instance.
    #
    def initialize; end

    #
    # Return an initialized FXSize instance which is a copy
    # of the input size _s_ (an FXSize instance).
    #
    def initialize(s); end

    #
    # Return an initialized FXSize instance, where _ww_ and
    # _hh_ are the initial width and height.
    #
    def initialize(ww, hh); end
    
    # Return +true+ if width or height is less than or equal to zero.
    def empty?; end
    
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

