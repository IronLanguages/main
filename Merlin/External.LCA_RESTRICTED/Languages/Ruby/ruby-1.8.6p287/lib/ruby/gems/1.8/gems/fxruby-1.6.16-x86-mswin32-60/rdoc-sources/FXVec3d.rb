module Fox
  class FXVec3d
    
    attr_accessor :x
    attr_accessor :y
    attr_accessor :z
    
    #
    # Return an initialized FXVec3d instance.
    #
    def initialize(xx, yy, zz); end
  
    #
    # Returns the element at _index_, where _index_ is 0, 1 or 2.
    # Raises IndexError if _index_ is out of range.
    #
    def [](index); end

    #
    # Set the element at _index_ to _value_ and return _value_.
    # Raises IndexError if _index_ is out of range.
    #
    def []=(index, value); end

    # Returns a new FXVec3d instance which is the negation of this one.
    def @-(); end

    #
    # Returns a new FXVec3d instance obtained by memberwise
    # addition of the _other_ FXVec3d instance with this
    # one.
    #
    def +(other); end

    #
    # Returns a new FXVec3d instance obtained by memberwise
    # subtraction of the _other_ FXVec3d instance from this
    # one.
    #
    def -(other); end

    #
    # Returns a new FXVec3d instance obtained by memberwise
    # multiplication of this vector's elements by the scalar
    # _n_.
    #
    def *(n); end

    #
    # Returns a new FXVec3d instance obtained by memberwise
    # division of this vector's elements by the scalar
    # _n_.
    # Raises ZeroDivisionError if _n_ is identically zero.
    #
    def /(n); end

    #
    # Returns the dot (scalar) product of this vector and _other_.
    #
    def dot(other); end

    # Return the cross product of this vector and _other_.
    def cross(other); end

    # Return +true+ if this vector is equal to _other_.
    def ==(other); end

    #
    # Return the square of the length of this vector.
    #
    def length2; end
    
    #
    # Return the length (magnitude) of this vector.
    #
    def length; end

    #
    # Clamp the values of this vector between limits _lo_ and _hi_.
    #
    def clamp(lo, hi); end
    
    #
    # Return a new FXVec3d instance which is a normalized version
    # of this one.
    #
    def normalize; end

    #
    # Return a new FXVec3d instance which is the lesser of this
    # vector and _other_.
    #
    def lo(other); end

    #
    # Return a new FXVec3d instance which is the greater of this
    # vector and _other_.
    #
    def hi(other); end
    
    #
    # Compute normal vector from points _a_, _b_ and _c_ (each of which is
    # an FXVec3d instance).
    #
    def FXVec3d.normal(a, b, c); end
    
    #
    # Compute approximate normal from four points _a_, _b_, _c_ and _d_.
    #
    def FXVec3d.normal(a, b, c, d); end
  end
end

