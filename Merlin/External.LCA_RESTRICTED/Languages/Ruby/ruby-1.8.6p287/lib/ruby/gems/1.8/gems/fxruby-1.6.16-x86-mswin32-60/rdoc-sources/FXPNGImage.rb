module Fox
  #
  # Portable Network Graphics (PNG) Image
  #
  class FXPNGImage < FXImage
    #
    # Return the suggested file extension for this image type ("png").
    #
    def FXPNGImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXPNGImage.mimeType; end

    # Return +true+ if PNG image file format is supported.
    def FXPNGImage.supported? ; end

    #
    # Return an initialized FXPNGImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in PNG file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: thePNGImage
    end
  end
end

