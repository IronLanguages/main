module Fox
  #
  # Portable Network Graphics (PNG) Icon
  #
  class FXPNGIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("png").
    #
    def FXPNGIcon.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXPNGIcon.mimeType; end

    # Return +true+ if PNG image file format is supported.
    def FXPNGIcon.supported? ; end

    #
    # Return an initialized FXPNGIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in PNG file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: thePNGIcon
    end
  end
  
  #
  # Load a PNG file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), the transparency color, the image width and the image height.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadPNG(store); end

  #
  # Save a PNG image to a stream.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the image data [FXStream]
  # +data+::	the image pixel data [String]
  # +transp+::	transparency color [FXColor]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  #
  def Fox.fxsavePNG(store, data, transp, width, height); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a PNG image.
  #
  def Fox.fxcheckPNG(store); end
end

