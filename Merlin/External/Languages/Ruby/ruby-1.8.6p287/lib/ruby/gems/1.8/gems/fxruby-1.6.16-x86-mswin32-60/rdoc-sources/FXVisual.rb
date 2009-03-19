module Fox
  #
  # An FXVisual instance describes the pixel format for a drawable (i.e. an FXDrawable instance).
  #
  # === Construction options for FXVisual
  #
  # +VISUAL_DEFAULT+::		Default visual
  # +VISUAL_MONOCHROME+::	Must be monochrome visual
  # +VISUAL_BEST+::		Best (deepest) visual
  # +VISUAL_INDEXCOLOR+::	Palette visual
  # +VISUAL_GRAYSCALE+::	Gray scale visual
  # +VISUAL_TRUECOLOR+::	Must be true color visual
  # +VISUAL_OWNCOLORMAP+::	Allocate private colormap
  # +VISUAL_DOUBLEBUFFER+::	Double-buffered [FXGLVisual]
  # +VISUAL_STEREO+::		Stereo [FXGLVisual]
  # +VISUAL_NOACCEL+::		No hardware acceleration [for broken h/w]
  # +VISUAL_SWAP_COPY+::	Buffer swap by copying (for FXGLVisual)
  #
  # === Visual type
  #
  # +VISUALTYPE_UNKNOWN+::	Undetermined visual type
  # +VISUALTYPE_MONO+::		Visual for drawing into 1-bpp surfaces
  # +VISUALTYPE_TRUE+::		True color
  # +VISUALTYPE_INDEX+::	Index [palette] color
  # +VISUALTYPE_GRAY+::		Gray scale
  #
  class FXVisual < FXId
    # Visual construction flags [Integer]
    attr_reader :flags

    # Visual depth, i.e. number of significant bits in color representation [Integer]
    attr_reader :depth

    # Number of colors [Integer]
    attr_reader :numColors

    # Number of bits of red [Integer]
    attr_reader :numRed

    # Number of bits of green [Integer]
    attr_reader :numGreen

    # Number of bits of blue [Integer]
    attr_reader :numBlue

    # Maximum number of colors [Integer]
    attr_accessor :maxColors
    
    #
    # The visual type, one of +VISUALTYPE_MONO+, +VISUALTYPE_TRUE+
    # +VISUALTYPE_INDEX+ or +VISUALTYPE_GRAY+. The visual type
    # may also be +VISUALTYPE_UNKNOWN+ before the visual is actually
    # created.
    #
    attr_reader :visualType

    #
    # Return an initialized FXVisual instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +flgs+::	visual construction flags [Integer]
    # +d+::	requested visual depth, in bits [Integer]
    #
    def initialize(a, flgs, d=32); end
  
    #
    # Get device pixel value for color value _clr_.
    #
    def getPixel(clr); end
  
    #
    # Get color value for device pixel value _pix_.
    #
    def getColor(pix); end
  end
end

