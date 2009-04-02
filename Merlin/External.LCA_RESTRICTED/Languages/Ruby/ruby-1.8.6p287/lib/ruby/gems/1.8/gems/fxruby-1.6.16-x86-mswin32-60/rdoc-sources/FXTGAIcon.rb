module Fox
  #
  # Targa Icon
  #
  class FXTGAIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("tga").
    #
    def FXTGAIcon.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXTGAIcon.mimeType; end

    #
    # Return an initialized FXTGAIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in Targa file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theTGAIcon
    end
  end
  
  #
  # Load a Targa file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), the number of channels (either 3 or 4), the image width and the image height.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadTGA(store); end

  #
  # Save a Targa image to a stream.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::		stream to which to write the image data [FXStream]
  # +data+::		the image pixel data [String]
  # +channels+::	number of channels in the image pixel data: 3 for RGB data, or 4 for RGBA data [Integer]
  # +width+::		width [Integer]
  # +height+::		height [Integer]
  #
  def Fox.fxsaveTGA(store, data, channels, width, height); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a TGA image.
  #
  def Fox.fxcheckTGA(store); end
end

