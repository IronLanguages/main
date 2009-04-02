begin
  require 'opengl'
rescue LoadError
  # Can't use FXGLGroup since it depends on Ruby/OpenGL
end

module Fox
  #
  # A group of OpenGL objects
  #
  class FXGLGroup < FXGLObject

    include Enumerable

    FLT_MAX =  1.0e+20
    FLT_MIN = -1.0e+20
    
    #
    # Returns an initialized FXGLGroup instance
    #
    def initialize
      super
      @list = []
    end
    
    #
    # Return number of objects in this group.
    #
    def size
      @list.size
    end
    
    #
    # Return child at position _pos_.
    #
    def [](pos)
      @list[pos]
    end
    
    #
    # Set child at position _pos_ to _obj_.
    #
    def []=(pos, obj)
      @list[pos] = obj
    end

    #
    # Iterate over child objects
    #
    def each_child # :yields: childObject
      @list.each { |child| yield child }
      self
    end

    alias each each_child

    #
    # Return bounding box for this group (an FXRangef instance)
    #
    def bounds
      box = nil
      if @list.empty?
        box = FXRangef.new(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
      else
        box = FXRangef.new(FLT_MAX, -FLT_MAX, FLT_MAX, -FLT_MAX, FLT_MAX, -FLT_MAX)
        @list.each { |obj| box.include!(obj.bounds) }
      end
      box
    end
    
    #
    # Draw this group into _viewer_ (an FXGLViewer instance).
    #
    def draw(viewer)
      @list.each { |obj| obj.draw(viewer) }
    end
    
    #
    # Perform hit test in _viewer_ (an FXGLViewer instance).
    #
    def hit(viewer)
#     GL.PushName(0xffffffff)
      GL.PushName(1000000)
      @list.each_with_index do |obj, i|
        GL.LoadName(i)
	obj.hit(viewer)
      end
      GL.PopName
    end
    
    #
    # Identify object by means of _path_.
    #
    def identify(path)
      objIndex = path.shift
      @list[objIndex].identify(path)
    end

    #
    # Return +true+ if group can be dragged.
    #
    def canDrag
      true
    end
    
    #
    # Drag group object around in _viewer_ (an FXGLViewer instance),
    # from (_fx_, _fy_) to (_tx_, _ty_).
    #
    def drag(viewer, fx, fy, tx, ty)
      @list.each { |obj| obj.drag(viewer, fx, fy, tx, ty) }
    end
    
    #
    # Insert child object (_obj_) at position _pos_.
    #
    def insert(pos, obj)
      raise NotImplementedError
    end
    
    #
    # Prepend child object (_obj_).
    #
    def prepend(obj)
      @list.unshift(obj)
    end
    
    #
    # Append child object
    #
    def append(obj)
      @list << obj
    end
    
    alias <<	append
    
    #
    # Replace child object at position _pos_ with _obj_.
    #
    def replace(pos, obj)
      @list[pos] = obj
    end
    
    #
    # If _obj_ is a reference to an FXGLObject in this group, remove the
    # child object from the list. If _obj_ is an integer, remove the child
    # object at that position from the list.
    #
    def remove(obj)
      if obj.is_a? FXGLObject
        @list.delete(obj)
      else
        @list.delete_at(obj)
      end
    end
    
    alias erase remove
  
    #
    # Remove all children from this group.
    #
    def clear
      @list.clear
    end
  end
end
