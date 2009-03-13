module Fox
  #
  # GIF Cursor class.
  #
  class FXGIFCursor < FXCursor
    #
    # Return the suggested file extension for this image type ("gif").
    #
    def FXGIFCursor.fileExt; end

    #
    # Return an initialized FXGIFCursor instance.
    #
    def initialize(a, pix, hx=-1, hy=-1) # :yields: theGIFCursor
    end
  end  
  
  #
  # Save a GIF (Graphics Interchange Format) image to a stream.
  # If _fast_ is +true+, the faster Floyd-Steinberg dither method will be used
  # instead of the slower Wu quantization algorithm.
  # Returns +true+ on success, +false+ on failure.
  #
  # ==== Parameters:
  #
  # +store+::	stream to which to write the image data [FXStream]
  # +data+::	the image pixel data [Array of FXColor]
  # +width+::	width [Integer]
  # +height+::	height [Integer]
  # +fast+::	if +true+, use faster Floyd-Steinberg algorithm [Boolean]
  #
  def Fox.fxsaveGIF(store, data, width, height, fast=true); end
  
  #
  # Load a GIF file from a stream.
  # If successful, returns an array containing the image pixel data (as a
  # String), the transparency color, the image width and the image height.
  # If it fails, the function returns +nil+.
  #
  # ==== Parameters:
  #
  # +store+::	stream from which to read the file data [FXStream]
  #
  def Fox.fxloadGIF(store); end

  #
  # Return +true+ if _store_ (an FXStream instance) contains a GIF image.
  #
  def Fox.fxcheckGIF(store); end
end
