module Fox
  #
  # Visual describes pixel format of a drawable
  #
  class FXGLVisual < FXVisual
  
    # Requested number of bits of red (default is 8) [Integer]
    attr_accessor :redSize
    
    # Requested number of bits of green (default is 8) [Integer]
    attr_accessor :greenSize
    
    # Requested number of bits of blue (default is 8) [Integer]
    attr_accessor :blueSize
    
    # Requested number of bits of alpha (default is zero) [Integer]
    attr_accessor :alphaSize
    
    # Requested depth size (default is 16) [Integer]
    attr_accessor :depthSize
    
    # Requested stencil size (default is zero) [Integer]
    attr_accessor :stencilSize
    
    # Requested number of bits of red for the accumulation buffer (default is zero) [Integer]
    attr_accessor :accumRedSize
    
    # Requested number of bits of green for the accumulation buffer (default is zero) [Integer]
    attr_accessor :accumGreenSize
    
    # Requested number of bits of blue for the accumulation buffer (default is zero) [Integer]
    attr_accessor :accumBlueSize
    
    # Requested number of bits of alpha for the accumulation buffer (default is zero) [Integer]
    attr_accessor :accumAlphaSize
    
    # Actual number of bits of red [Integer]
    attr_reader :actualRedSize

    # Actual number of bits of green [Integer]
    attr_reader :actualGreenSize

    # Actual number of bits of blue [Integer]
    attr_reader :actualBlueSize

    # Actual number of bits of alpha [Integer]
    attr_reader :actualAlphaSize

    # Actual depth size [Integer]
    attr_reader :actualDepthSize

    # Actual stencil size [Integer]
    attr_reader :actualStencilSize

    # Actual number of bits of red for the accumulation buffer [Integer]
    attr_reader :actualAccumRedSize

    # Actual number of bits of green for the accumulation buffer [Integer]
    attr_reader :actualAccumGreenSize

    # Actual number of bits of blue for the accumulation buffer [Integer]
    attr_reader :actualAccumBlueSize

    # Actual number of bits of alpha for the accumulation buffer [Integer]
    attr_reader :actualAccumAlphaSize

    #
    # Construct default visual
    #
    def initialize(app, flags) # :yields: theGLVisual

    #
    # Test if OpenGL is possible, and what level is supported.
    # Because of remote display capability, the display server may
    # support a different level of OpenGL than the client; it may
    # even support no OpenGL at all!  This function returns the lesser
    # of the client support level and the display server support level.
    #
    def FXGLVisual.supported?(app); end

    # Return +true+ if double-buffered
    def doubleBuffered?; end

    # Return +true+ if stereo
    def stereo?; end
    
    # Return +true+ if hardware-accelerated
    def accelerated?; end
    
    # Return +true+ if this visual "swaps" by copying instead of flipping buffers.
    def bufferSwapCopy?; end
  end
end

