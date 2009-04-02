module Fox
  #
  # FXGLCanvas is an area drawn by another object.
  #
  class FXGLCanvas < FXCanvas
    #
    # Construct an OpenGL-capable canvas, with its own private display list.
    #
    def initialize(parent, vis, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theGLCanvas
    end
  
    #
    # Construct an OpenGL-capable canvas that shares its display
    # list with another GL canvas.  This canvas becomes a member
    # of a display list share group.  All members of the display
    # list share group have to have the same visual.
    #
    def initialize(parent, vis, sharegroup, target=nil, selector=0, opts=0, x=0, y=0, width=0, height=0) # :yields: theGLCanvas
    end

    # Return +true+ if sharing display lists.
    def shared? ; end

    #
    # Return an integer handle to the underlying OpenGL context.
    # On Unix/Linux systems, this is the GLX rendering context
    # returned by a call to glXCreateContext(). On Microsoft Windows
    # systems, it is the value returns by wglCreateContext().
    #
    def context; end

    # Make OpenGL context current prior to performing OpenGL commands
    def makeCurrent(); end
  
    # Make OpenGL context non-current
    def makeNonCurrent(); end

    # Return +true+ if this canvas' context is the current context.
    def current? ; end

    # Swap front and back buffer
    def swapBuffers(); end
  end
end

