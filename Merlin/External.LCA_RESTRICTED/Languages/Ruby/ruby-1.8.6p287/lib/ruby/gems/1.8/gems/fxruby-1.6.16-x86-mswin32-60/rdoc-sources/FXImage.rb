module Fox
  #
  # An Image is a rectangular array of pixels.  It supports two representations
  # of these pixels: a client-side pixel buffer which is stored as an array of
  # FXColor, and a server-side pixmap which is stored in an organization directly
  # compatible with the screen, for fast drawing onto the device.
  # The server-side representation is not directly accessible from the current
  # process as it lives in the process of the X Server or GDI.
  #
  # === Image rendering hints
  #
  # +IMAGE_KEEP+::
  #   Keep pixel data in client. By default, FOX discards of the client-side
  #   pixel data for an image after you call create() for that image. When the
  #   +IMAGE_KEEP+ option is set for FXImage (or one of its subclasses), the
  #   client-side buffer is maintained. You will typically want to set this option
  #   if you intend to do repeated re-rendering of the image after it has been
  #   created.
  # +IMAGE_OWNED+::
  #   If the +IMAGE_OWNED+ option is set, the FXImage object assumes ownership
  #   of its client-side pixel data (if any).
  # +IMAGE_DITHER+::
  #   Dither image to look better
  # +IMAGE_NEAREST+::
  #   Turn off dithering and map to nearest color
  # +IMAGE_OPAQUE+::
  #   Force opaque background
  # +IMAGE_ALPHACOLOR+::
  #   By default, FOX will use the transparency color obtained from the image
  #   file as the transparency (alpha) color. If you pass the +IMAGE_ALPHACOLOR+
  #   flag, the user-specified transparency color will be used instead.
  # +IMAGE_SHMI+::
  #   Using shared memory image
  # +IMAGE_SHMP+::
  #   Using shared memory pixmap
  # +IMAGE_ALPHAGUESS+::
  #   Guess transparency color from corners
  #

  class FXImage < FXDrawable

    # Pixel data [FXMemoryBuffer]
    attr_reader	:data
    
    # Option flags [Integer]
    attr_accessor :options
    
    #
    # Create an image.  If a client-side pixel buffer has been specified,
    # the image does not own the pixel buffer unless the +IMAGE_OWNED+ flag is
    # set.  If the +IMAGE_OWNED+ flag is set but a +nil+ pixel buffer is
    # passed, a pixel buffer will be automatically created and will be owned
    # by the image. The flags +IMAGE_SHMI+ and +IMAGE_SHMP+ may be specified for
    # large images to instruct #render to use shared memory to communicate
    # with the server.
    #
    # ==== Parameters:
    #
    # +a+::		an application instance [FXApp]
    # +pixels+::	pixels [Array of FXColor values]
    # +opts+::		image options [Integer]
    # +width+::		image width [Integer]
    # +height+::		image height [Integer]
    #
    def initialize(a, pixels=nil, opts=0, width=1, height=1) # :yields: theImage
    end

    #
    # Return the color of the pixel at (_x_, _y_).
    #
    # ==== Parameters:
    #
    # +x+::	x-coordinate of the pixel of interest [Integer]
    # +y+::	y-coordinate of the pixel of interest [Integer]
    #
    def getPixel(x, y) ; end

    #
    # Set pixel at (_x_, _y_) to _clr_.
    #
    # ==== Parameters:
    #
    # +x+::	x-coordinate of the pixel of interest [Integer]
    # +y+::	y-coordinate of the pixel of interest [Integer]
    # +clr+::	new color value for this pixel [FXColor]
    #
    def setPixel(x, y, clr) ; end
    
    # Scan the image and return +false+ if it's fully opaque.
    def hasAlpha?; end

    #
    # Restore client-side pixel buffer from image.
    # This operation overwrites any current values for the client-side
    # pixel buffer with values corresponding to the server-side image.
    # If the image data is +nil+ at the time #restore is called, the
    # image will first allocate an (owned) pixel buffer to use for this
    # operation.
    #
    def restore() ; end

    #
    # Render the image from client-side pixel buffer, if there is data
    # and if the image width and height are greater than zero.
    #
    def render() ; end
  
    #
    # Release the client-side pixels buffer, free it if it was owned
    # (i.e. if the +IMAGE_OWNED+ option is set)..
    # If it is not owned, the image just forgets about the buffer.
    #
    def release(); end

    #
    # Rescale pixels image to the specified width and height and then
    # re-render the server-side image from the client-side pixel buffer. Note that this
    # serves a slightly different purpose than the base class resize() method,
    # which simply resizes the client-side pixel data buffer but doesn't
    # transform the image.
    #
    # The optional third argument specifies the _quality_ of the scaling algorithm
    # used. By default, #scale uses a fast (but low quality) nearest-neighbor algorithm
    # for scaling the image to its new size. To use the higher-quality scaling algorithm
    # from FOX 1.0, you should pass in a value of 1 as the third argument to #scale.
    #
    # ==== Parameters:
    #
    # +width+::		new image width, in pixels [Integer]
    # +height+::		new image height, in pixels [Integer]
    # +quality+::	scaling algorithm quality, either 0 or 1 (see above) [Integer]
    #
    def scale(w, h, quality=0) ; end
  
    #
    # Mirror image horizontally and/or vertically and then re-render the
    # server-side image from the client-side pixel buffer.
    #
    # ==== Parameters:
    #
    # +horizontal+::	if +true+, the image will be flipped from left to right [Boolean]
    # +vertical+::	if +true+, the image will be flipped from top to bottom [Boolean]
    #
    def mirror(horizontal, vertical) ; end
  
    #
    # Rotate image counter-clockwise by some number of degrees and then
    # re-render the server-side image from the client-side pixel buffer.
    #
    # ==== Parameters:
    #
    # +degrees+::	number of degrees by which to rotate the image [Integer]
    #
    def rotate(degrees) ; end
  
    #
    # Crop image to given rectangle and then re-render the server-side image
    # from the client-side pixel buffer. This method calls resize() to adjust the client
    # and server side representations.  The new image may be smaller or larger
    # than the old one; blank areas are filled with color. There must be at
    # least one pixel of overlap between the old and the new image.
    #
    # ==== Parameters:
    #
    # +x+::	x-coordinate for top left corner of the clip rectangle [Integer]
    # +y+::	y-coordinate for top left corner of the clip rectangle [Integer]
    # +width+::	width of the clip rectangle [Integer]
    # +height+::	height of the clip rectangle [Integer]
    # +color+::	fill color for blank areas after crop [FXColor]
    #
    def crop(x, y, w, h, color=0) ; end
    
    # Fill image with uniform color.
    def fill(color); end
    
    #
    # Fade an image to a certain color by a certain factor. The _factor_ is
    # some integer value between 0 and 255 inclusive, where a factor of 255 indicates no fading and a factor
    # of zero indicates that the image is completely faded to the fade _color_.
    #
    # ==== Parameters:
    #
    # +color+::   the fade color [FXColor]
    # +factor+::	fading factor [Integer]
    #
    def fade(color, factor=255); end

    #
    # Shear image horizontally and then re-render the server-side image
    # from the client-side pixel buffer. The number of pixels is equal to the
    # _shear_ parameter times 256. The area outside the image is filled
    # with transparent black, unless another _color_ is specified.
    #
    # ==== Parameters:
    #
    # +shear+::   how far to shear [Integer]
    # +color+::	  fill color for areas outside the sheared image [FXColor]
    #
    def xshear(shear, color=0); end

    #
    # Shear image verticallyand then re-render the server-side image
    # from the client-side pixel buffer. The number of pixels is equal to the
    # _shear_ parameter times 256. The area outside the image is filled
    # with transparent black, unless another _color_ is specified.
    #
    # ==== Parameters:
    #
    # +shear+::   how far to shear [Integer]
    # +color+::	  fill color for areas outside the sheared image [FXColor]
    #
    def yshear(shear, color=0); end

    #
    # Fill image using a horizontal gradient.
    #
    # ==== Parameters:
    #
    # +left+::   starting color, for leftmost pixels [FXColor]
    # +right+::  ending color, for rightmost pixels [FXColor]
    #
    def hgradient(left, right); end

    #
    # Fill image using a vertical gradient.
    #
    # ==== Parameters:
    #
    # +top+::      starting color, for topmost pixels [FXColor]
    # +bottom+::	 ending color, for bottommost pixels [FXColor]
    #
    def vgradient(top, bottom); end

    #
    # Fill image using a bilinear gradient.
    #
    # ==== Parameters:
    #
    # +topleft+::      pixel color for top-left corner [FXColor]
    # +topright+::	   pixel color for top-right corner [FXColor]
    # +bottomleft+::   pixel color for bottom-left corner [FXColor]
    # +bottomright+::  pixel color for bottom-right corner [FXColor]
    #
    def gradient(topleft, topright, bottomleft, bottomright); end

    #
    # Blend image over uniform color.
    #
    # ==== Parameters:
    #
    # +color+::	  the blended color [FXColor]
    #
    def blend(color); end
  
    #
    # Save pixel data to a stream.
    #
    # Note that the base class version of
    # #savePixels saves the pixel data as-is, without any image format
    # specific information. For example, if you have a 1024x768 image
    # without an alpha channel (and thus only the red, green and blue
    # channels) the total number of bytes written to the stream will be
    # 1024*768*3*8. The behavior of #savePixels is different for subclasses
    # such as FXPNGImage, where #savePixels will instead save the image
    # data in a specific image file format (i.e. the PNG file format).
    #
    # ==== Parameters:
    #
    # +store+::	opened stream to which to save the pixel data [FXStream]
    #
    def savePixels(store) ; end
  
    #
    # Load pixel data from a stream.
    #
    # Note that the base class version of
    # #loadPixels expects to read the the pixel data in a neutral format
    # (i.e. without any image format specific information). For example, if
    # you have a 1024x768 image without an alpha channel (and thus only the
    # red, green and blue channels), #loadPixels will attempt to read a total
    # of 1024*768*3*8 bytes from the stream. The behavior of #loadPixels is
    # different for subclasses such as FXPNGImage, where #loadPixels will instead
    # expects to read the image data in a specific image file format (i.e. the
    # PNG file format).
    #
    # ==== Parameters:
    #
    # +store+::	opened stream from which to read the pixel data [FXStream]
    #
    def loadPixels(store) ; end
  end
end
