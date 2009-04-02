module Fox
  #
  # Represents a range in 3-D space.
  #
  class FXRangef
  
    # Lower corner of range [FXVec3f]
    attr_accessor :lower

    # Upper corner of range [FXVec3f]
    attr_accessor :upper

    # Default constructor
    def initialize(xlo=0.0, xhi=0.0, ylo=0.0, yhi=0.0, zlo=0.0, zhi=0.0) ; end

    # Initialize this FXRangef instance from another instance
    def initialize(bounds); end

    # Initialize box to fully contain the given sphere (an FXSpheref instance)
    def initialize(sphere); end

    # Return the width of the box
    def width; end

    # Return the height of the box
    def height; end

    # Return the depth of the box
    def depth; end

    # Return the length of the longest side of the box
    def longest; end

    # Return the length of the shortest side of the box
    def shortest; end

    # Return the length of the diagonal of the box
    def diameter; end

    # Return the radius of the box.
    def radius; end

    # Return the diagonal vector for the box (an FXVec3f instance).
    def diagonal; end

    # Return the center of the box (a point, as an FXVec3f instance).
    def center; end

    # Returns +true+ if this is an empty range (i.e. if any of the side
    # lengths is zero or less).
    def empty?() ; end

    # Returns +true+ if this range contains the point (_x_, _y_, _z_).
    def contains?(x, y, z) ; end

    # Returns +true+ if this range contains the point _p_ (an FXVec3f instance)
    def contains?(p) ; end

    # Returns +true+ if this box properly contains _aRange_ (another FXRangef
    # instance)
    def contains?(aRange) ; end

    # Returns +true+ if this box properly contains _aSphere_ (an FXSpheref
    # instance)
    def contains?(aSphere) ; end

    #
    # Include the given range or point into this range and return a reference
    # to self. Valid forms are:
    #
    #     range.include!(aRange)  -> range
    #     range.include!(x, y, z) -> range
    #     range.include!(vec)     -> range
    #     range.include!(sphere)  -> range
    #
    def include!(*args) ; end

    # Intersect box with a plane <em>ax+by+cz+w</em>; returns -1, 0 or 1.
    def intersect(plane); end

    # Return true if the ray from _u_ to _v_ (both FXVec3f instances
    # representing the ray endpoints) intersects this box.
    def intersects?(u, v) ; end

    # Returns +true+ if any part of this range overlaps the _other_ range.
    def overlaps?(other) ; end

    # Return the _c_th corner of this box (an FXVec3f instance).
    # Raises IndexError if _c_ is less than zero or greater than 7.
    def corner(c); end

    # Return a new FXRangef instance which is the union of this box and
    # another box.
    def union(other); end

    # Return a new FXRangef instance which is the intersection of this box
    # and another box.
    def intersection(other); end
  end
end
