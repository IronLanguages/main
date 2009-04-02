module Fox
  #
  # JPEG icon class
  #
  class FXJPGIcon < FXIcon
  
    #
    # Return the suggested file extension for this image type ("jpg").
    #
    def FXJPGIcon.fileExt; end

    #
    # Return the MIME type for this image type
    #
    def FXJPGIcon.mimeType; end
    
    # Return +true+ if JPEG image file format is supported.
    def FXJPGIcon.supported? ; end

    # Image quality setting [Integer]
    attr_accessor :quality

    #
    # Return an initialized FXJPGIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in JPEG file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theJPGIcon
    end
  end
  #
  # Load a JPEG file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), transparency color, image width, image height and quality.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadJPG(store); end
  
  #
  # Save a JPEG image to a stream.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the image data [FXStream]
  # +data+::	the image pixel data [String]
  # +transp+::	transparency color [FXColor]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  # +quality+::	image quality [Integer]
  #
  def Fox.fxsaveJPG(store, data, transp, width, height, quality); end

  #
  # Return +true+ if _store_ (an FXStream instance) contains a JPEG image.
  #
  def Fox.fxcheckJPG(store); end
end

