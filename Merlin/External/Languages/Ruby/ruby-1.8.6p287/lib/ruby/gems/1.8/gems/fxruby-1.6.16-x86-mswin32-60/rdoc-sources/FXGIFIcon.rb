module Fox
  #
  # GIF Icon class.
  #
  class FXGIFIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("bmp").
    #
    def FXGIFIcon.fileExt; end

    #
    # Return the suggested MIME type for this image type
    #
    def FXGIFIcon.mimeType; end

    #
    # Return an initialized FXGIFIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in GIF file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theGIFIcon
    end
  end
end
