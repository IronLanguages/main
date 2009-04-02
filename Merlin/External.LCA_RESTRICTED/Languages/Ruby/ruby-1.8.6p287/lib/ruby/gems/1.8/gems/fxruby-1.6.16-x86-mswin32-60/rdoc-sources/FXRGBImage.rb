module Fox
  #
  # Iris RGB Image
  #
  class FXRGBImage < FXImage
    #
    # Return the suggested file extension for this image type ("rgb").
    #
    def FXRGBImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXRGBImage.mimeType; end

    #
    # Return an initialized FXRGBImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in IRIS RGB file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theRGBImage
    end
  end
end

