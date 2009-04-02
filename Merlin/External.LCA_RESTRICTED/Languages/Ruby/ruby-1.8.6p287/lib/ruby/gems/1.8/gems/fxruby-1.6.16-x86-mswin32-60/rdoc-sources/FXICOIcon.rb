module Fox
  #
  # ICO icon
  #
  class FXICOIcon < FXIcon
    #
    # Return the suggested file extension for this image type ("ico").
    #
    def FXICOIcon.fileExt; end

    #
    # Return the MIME type for this image type
    #
    def FXICOIcon.mimeType; end

    #
    # Return an initialized FXICOIcon instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in ICO file format [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theICOIcon
    end
  end

  #
  # Load a ICO file from _store_ (an FXStream instance).
  # On success, returns an array whose elements are the image data (a String),
  # transparency color, icon width, icon height, and the icon hotspot
  # x and y coordinates. If the operation fails, this method returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadICO(store); end

  #
  # Save a ICO image to _store_ (an FXStream instance).
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the image data [FXStream]
  # +pixels+::	the image pixel data [String]
  # +transp+::	transparency color [FXColor]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  # +xspot+::	hotspot x-coordinate [Integer]
  # +yspot+::	hotspot y-coordinate [Integer]
  #
  def Fox.fxsaveICO(store, pixels, transp, width, height, xspot=-1, yspot=-1); end
  
  #
  # Return +true+ if _store_ (an FXStream instance) contains a ICO image.
  #
  def Fox.fxcheckICO(store); end
end

