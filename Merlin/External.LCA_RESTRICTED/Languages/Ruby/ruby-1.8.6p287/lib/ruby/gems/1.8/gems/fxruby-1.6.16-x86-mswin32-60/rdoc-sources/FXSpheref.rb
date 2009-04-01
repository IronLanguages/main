module Fox
  #
  # Spherical bounds
  #
  class FXSpheref
    # Sphere center [FXVec3f]
    attr_accessor :center
    
    # Sphere radius [Float]
    attr_accessor :radius
    
    # Default constructor
    def initialize; end
    
    # Copy constructor
    def initialize(otherSphere); end
    
    # Initialize from center and radius
    def initialize(cen, rad=0.0); end
    
    # Initialize from center (_x_, _y_, _z_) and radius (_rad_).
    def initialize(x, y, z, rad=0.0); end
    
    # Initialize sphere to fully contain the given bounding box (an FXRangef instance).
    def initialize(bounds); end
    
    # Return the diameter of this sphere.
    def diameter; end
    
    # Return +true+ if this sphere is empty (i.e. has zero radius).
    def empty?; end
    
    # Return +true+ if this sphere contains the point at (_x_, _y_, _z_).
    def contains?(x, y, z); end
    
    # Return +true+ if this sphere contains the point _p_ (an FXVec3f instance).
    def contains?(p); end
    
    # Return +true+ if this sphere properly contains _box_ (an FXRangef instance).
    def contains?(box); end
    
    # Return +true+ if this sphere properly contains _sphere_ (another FXSpheref instance).
    def contains?(sphere); end
    
    # Include the point _p_ (an FXVec3f instance) and return a reference to self.
    def include!(p); end
    
    # Expand radius to include point and return a reference to self.
    def includeInRadius!(x, y, z); end

    # Expand radius to include point (an FXVec3f instance) and return a reference to self.
    def includeInRadius!(p); end

    # Include the range _box_ (an FXRangef instance) into this sphere and return self.
    def include!(box); end
    
    # Expand radius to include box (an FXRangef instance) and return a reference to self.
    def includeInRadius!(box);

    # Include the sphere _sphere_ (an FXSpheref instance) into this sphere and return self.
    def include!(sphere); end
    
    # Expand radius to include sphere (an FXSpheref instance) and return self.
    def includeInRadius!(sphere); end
    
    # Intersect this sphere with the plane <em>ax+by+cz+w</em> and return -1, 0 or +1.
    # Here, _plane_ is an FXVec4f instance describing the plane.
    def intersect(plane); end
    
    # Return +true+ if this sphere intersects the ray between points _u_ and _v_
    # (both FXVec3f instances).
    def intersects?(u, v); end
    
    # Return +true+ if this sphere overlaps with _box_ (an FXRangef instance).
    def overlaps?(box); end
    
    # Return +true+ if this sphere overlaps with another sphere.
    def overlaps?(sphere); end
  end
end
