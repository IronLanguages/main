module Fox
  #
  # Tagged Image File Format (TIFF) Image
  #
  class FXTIFImage < FXImage
    #
    # Return the suggested file extension for this image type ("tif").
    #
    def FXTIFImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXTIFImage.mimeType; end

    # Return +true+ if TIF image file format is supported.
    def FXTIFImage.supported? ; end

    # Codec setting [Integer]
    attr_accessor :codec
    
    #
    # Return an initialized FXTIFImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in TIF file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theTIFImage
    end
  end
end

