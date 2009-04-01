module Fox
  #
  # The BMP Icon class is a convenience class for working with icons in the
  # Microsoft Bitmap (.bmp) graphics file format.  This makes it possible to
  # use resources created with Windows development tools inside FOX without
  # need for graphics file format translators.  The bitmap loaded handles
  # 1, 4, and 8 bit paletted bitmaps, 16 and 24 bit RGB bitmaps, and
  # 32 bit RGBA bitmaps.
  #
  class FXBMPIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("bmp").
    #
    def FXBMPIcon.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXBMPIcon.mimeType; end
    
    #
    # Return an initialized FXBMPIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in BMP file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=FXRGB(192,192,192), opts=0, width=1, height=1) # :yields: theBMPIcon
    end
  end

  #
  # Load a BMP file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), the transparency color, the image width and the image height.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadBMP(store); end

  #
  # Save a BMP image to a stream.
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
  def Fox.fxsaveBMP(store, data, transp, width, height); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a BMP image.
  #
  def Fox.fxcheckBMP(store); end
end
