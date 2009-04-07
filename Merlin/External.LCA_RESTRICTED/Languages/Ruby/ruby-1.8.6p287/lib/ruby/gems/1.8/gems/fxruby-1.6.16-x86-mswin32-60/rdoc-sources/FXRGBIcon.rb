module Fox
  #
  # Iris RGB Icon
  #
  class FXRGBIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("rgb").
    #
    def FXRGBIcon.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXRGBIcon.mimeType; end

    #
    # Return an initialized FXRGBIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in IRIS RGB file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theRGBIcon
    end
  end
  
  #
  # Load a RGB file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), the transparency color, the image width and the image height.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadRGB(store); end

  #
  # Save a RGB image to a stream.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the file data [FXStream]
  # +data+::	the image pixel data [String]
  # +transp+::	transparency color [FXColor]
  # +opts+::	options [Integer]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  #
  def Fox.fxsaveRGB(store, data, transp, width, height); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a RGB image.
  #
  def Fox.fxcheckRGB(store); end
end

