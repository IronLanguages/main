module Fox
  #
  # JPEG Image class
  #
  class FXJPGImage < FXImage
  
    #
    # Return the suggested file extension for this image type ("jpg").
    #
    def FXJPGImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXJPGImage.mimeType; end

    # Return +true+ if JPEG image file format is supported.
    def FXJPGImage.supported? ; end

    # Image quality
    attr_accessor :quality

    #
    # Return an initialized FXJPGImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in JPEG file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theJPGImage
    end
  end
end

