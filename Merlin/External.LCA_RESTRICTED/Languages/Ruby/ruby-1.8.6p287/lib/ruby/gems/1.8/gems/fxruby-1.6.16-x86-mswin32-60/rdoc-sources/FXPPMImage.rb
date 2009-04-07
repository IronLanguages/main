module Fox
  #
  # Portable Pixmap (PPM) Image
  #
  class FXPPMImage < FXImage
    #
    # Return the suggested file extension for this image type ("ppm").
    #
    def FXPPMImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXPNGImage.mimeType; end

    #
    # Return an initialized FXPPMImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in PPM file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: thePPMImage
    end
  end
end

