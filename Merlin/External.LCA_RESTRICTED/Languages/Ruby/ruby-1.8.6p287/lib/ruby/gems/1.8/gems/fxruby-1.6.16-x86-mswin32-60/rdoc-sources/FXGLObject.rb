module Fox
  #
  # Basic OpenGL object.
  #
  class FXGLObject < FXObject
    #
    # Return an initialized FXGLObject instance.
    #
    def initialize # :yields: theGLObject
    end

    #
    # Return a new object which is a copy (i.e. a "clone") of this one.
    #
    def copy; end
    
    #
    # Return a bounding box (an FXRangef instance) for this object.
    #
    def bounds; end
    
    #
    # Draw this object in a viewer.
    #
    # ==== Parameters:
    #
    # +viewer+::	the viewer window into which we're drawing [FXGLViewer]
    #
    def draw(viewer); end
    
    #
    # Draw this object for hit-testing purposes.
    #
    # ==== Parameters:
    #
    # +viewer+::	the viewer window into which we're drawing [FXGLViewer]
    #
    def hit(viewer); end
    
    #
    # Return +true+ if this object can be dragged around.
    #
    def canDrag; end
    
    #
    # Return +true+ if this object can be deleted from the scene.
    #
    def canDelete; end
    
    #
    # Drag this object from one position to another. Returns +true+
    # if the drag was successful.
    #
    # ==== Parameters:
    #
    # +viewer+::	the viewer window in which we're dragging [FXGLViewer]
    # +fx+::		x-coordinate for position we're dragging from [Integer]
    # +fy+::		y-coordinate for position we're dragging from [Integer]
    # +tx+::		x-coordinate for position we're dragging to [Integer]
    # +ty+::		y-coordinate for position we're dragging to [Integer]
    #
    def drag(viewer, fx, fy, tx, ty); end

    #
    # Identify sub-object given path, where _path_ is a list of integer
    # names pushed onto the stack during hit testing.
    #
    # ==== Parameters:
    #
    # +path+::	an array of integers [Array]
    #
    def identify(path); end
  end
end

