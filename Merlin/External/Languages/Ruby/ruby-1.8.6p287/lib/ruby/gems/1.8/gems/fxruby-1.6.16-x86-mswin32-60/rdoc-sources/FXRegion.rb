module Fox
  class FXRegion
    # Construct new empty region
    def initialize; end
    
    # Construct new region copied from region _r_ (another FXRegion instance).
    def initialize(r); end
    
    # Construct new region from rectangle _rect_ (an FXRectangle instance)
    def initialize(rect); end
    
    #
    # Construct rectangle region, where (_x_, _y_) are the coordinates of the
    # upper left-hand corner and (_w_, _h_) are its width and height.
    #
    def initialize(x, y, w, h); end
    
    #
    # Construct polygon region from an array of points. Here, _points_ is
    # an array of FXPoint instances.
    #
    def initialize(points, winding=false); end
    
    # Return +true+ if this region is empty
    def empty?; end
    
    # Return +true+ if this region contains the point at (_x_, _y_).
    def contains?(x, y); end
    
    # Return +true+ if this region contains the rectangle whose upper left
    # corner is at (_x_, _y_) and whose width and height are (_w_, _h_).
    def contains?(x, y, w, h); end
    
    # Return the bounding box (an FXRectangle instance) for this region.
    def bounds; end
    
    # Offset this region by (_dx_, _dy_) units, and return a reference to
    # this region.
    def offset!(dx, dy); end
    
    # Return a new FXRegion which is the union of this region and _other_
    # (another FXRegion instance).
    def +(other); end
    
    # Return a new FXRegion which is the intersection of this region and
    # _other_ (another FXRegion instance).
    def *(other); end
    
    # Return a new FXRegion which is the difference of this region and
    # _other_ (another FXRegion instance).
    def -(other); end
    
    # Return a new FXRegion which is the exclusive-or (XOR) of this region
    # with _other_ (another FXRegion instance).
    def ^(other); end
    
    # Return +true+ if this region is equal to _other_ (another FXRegion instance).
    def ==(other); end
    
    # Reset this region to empty.
    def reset; end
  end
end

