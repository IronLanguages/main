module Fox
  # GIF Image class
  class FXGIFImage < FXImage
    #
    # Return the suggested file extension for this image type ("bmp").
    #
    def FXGIFImage.fileExt; end

    #
    # Return the suggested MIME type for this image type.
    #
    def FXGIFImage.mimeType; end

    #
    # Return an initialized FXGIFImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in GIF file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theGIFImage
    end
  end
end

