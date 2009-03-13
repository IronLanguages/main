module Fox
  #
  # A GL context is an object representing the OpenGL state information.
  # Multiple GL context may share display lists to conserve memory.
  # When drawing multiple windows, it may be advantageous to share not only
  # display lists, but also GL contexts.  Since the GL context is created
  # for a certain frame-buffer configuration, sharing of GL contexts is
  # only possible if the windows sharing the GL context all have the same
  # GL visual.
  # However, display lists may be shared between different GL contexts.
  #
  class FXGLContext < FXId
  
    # The visual [FXGLVisual]
    attr_reader :visual

    # Construct an OpenGL context.
    # If _other_ is a reference to an existing FXGLContext, this context will
    # share display lists with that other context.
    # Otherwise, this context will use its own private display list.
    def initialize(app, visual, other=nil) # :yields: theGLContext
    end
  
    # Return +true+ if it is sharing display lists.
    def shared?; end
    
    # Make this OpenGL context current prior to performing OpenGL commands.
    def begin(drawable); end
    
    # Make this OpenGL context non-current.
    def end(); end
    
    # Swap front and back buffer
    def swapBuffers(); end
    
    # Copy part of backbuffer to front buffer [Mesa]
    def swapSubBuffers(x, y, w, h); end
  end
end

