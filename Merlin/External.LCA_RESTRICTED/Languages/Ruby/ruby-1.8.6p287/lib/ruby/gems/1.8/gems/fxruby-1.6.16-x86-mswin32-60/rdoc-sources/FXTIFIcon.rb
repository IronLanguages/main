module Fox
  #
  # Tagged Image File Format (TIFF) Icon
  #
  class FXTIFIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("tif").
    #
    def FXTIFIcon.fileExt; end

    #
    # Return the MIME type for this image type
    #
    def FXTIFIcon.mimeType; end

    # Return +true+ if TIF image file format is supported.
    def FXTIFIcon.supported? ; end

    # Codec setting [Integer]
    attr_accessor :codec

    #
    # Return an initialized FXTIFIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in TIFF file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theTIFIcon
    end
  end
  
  #
  # Load a TIFF file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), transparency color, width, height and codec setting.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadTIF(store); end

  #
  # Save a TIFF image to a stream.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the image data [FXStream]
  # +data+::	the image pixel data [String]
  # +transp+::	transparency color [FXColor]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  # +codec+::	codec setting [Integer]
  #
  def Fox.fxsaveTIF(store, data, transp, width, height, codec); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a TIFF image.
  #
  def Fox.fxcheckTIF(store); end
end

