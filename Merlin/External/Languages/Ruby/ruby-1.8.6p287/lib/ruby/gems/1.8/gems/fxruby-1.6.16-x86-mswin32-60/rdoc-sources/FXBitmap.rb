module Fox
  #
  # A bitmap is a rectangular array of pixels.
  # It supports two representations of these pixels: a client-side pixel buffer,
  # and a server-side pixmap which is stored in an organization directly compatible
  # with the screen, for fast drawing onto the device. The server-side representation
  # is not directly accessible from the current process at it lives in the process
  # of the X server or GDI (on Microsoft Windows).
  # The client-side pixel array is of size height x (width+7)/8 bytes; in other
  # words, 8 pixels packed into a single byte, starting with bit zero on the left.
  # 
  # === Image rendering hints
  #
  # +BITMAP_KEEP+::	Keep pixel data in client
  # +BITMAP_OWNED+::	Pixel data is owned by image
  # +BITMAP_SHMI+::	Using shared memory image
  # +BITMAP_SHMP+::	Using shared memory pixmap
  #
  class FXBitmap < FXDrawable
  
    alias data getData

    #
    # Return an initialized FXBitmap instance.
    # If a client-side pixel buffer (the _pixels_ argument) has been specified,
    # the bitmap does not own that pixel buffer unless the +BITMAP_OWNED+ flag
    # is set. If the +BITMAP_OWNED+ flag _is_ set, but a +nil+ value for _pixels_
    # is passed in, a pixel buffer will be automatically created and will be
    # owned by the bitmap. The flags +BITMAP_SHMI+ and +BITMAP_SHMP+ may be
    # specified for large bitmaps to instruct FXBitmap#render to use shared
    # memory to communicate with the server.
    #
    def initialize(app, pixels=nil, opts=0, width=1, height=1) # :yields: theBitmap
    end

    #
    # Populate the bitmap with new pixel data of the same size; it will assume
    # ownership of the pixel data if the +BITMAP_OWNED+ option is passed in the _opts_.
    # The server-side representation of the image, if it exists, is not updated;
    # to update ther server-side representation, call #render.
    #
    def setData(pix, opts=0); end

    #
    # Populate the bitmap with new pixel data of a new size; it will assume ownership
    # of the pixel data if the +BITMAP_OWNED+ option is passed in the _opts_. The size of the server-
    # side representation of the image, if it exists, is adjusted but the contents are
    # not updated; to update the server-side representation, call #render.
    #
    def setData(pix, opts, w, h); end

    # Return the pixel data.
    def getData(); end
    
    # Return the option flags.
    def options; end
    
    # Set the options.
    def options=(opts); end

    # Retrieve pixels from the server-side bitmap.
    def restore; end

    # Render the server-side representation of the bitmap from the client-side pixels.
    def render() ; end
    
    #
    # Release the client-side pixels buffer and free it if it was owned.
    # If it is not owned, the image just forgets about the buffer.
    #
    def release(); end

    #
    # Resize both client-side and server-side representations (if any) to the
    # given width and height.  The new representations typically contain garbage
    # after this operation and need to be re-filled.
    #
    def resize(w, h); end

    # Save pixel data only
    def savePixels(stream); end
    
    # Load pixel data from a stream
    def loadPixels(stream); end

    # Get pixel state (either +true+ or +false+) at (_x_, _y_)
    def getPixel(x, y) ; end

    # Change pixel at (_x_, _y_), where _color_ is either +true+ or +false+.
    def setPixel(x, y, color) ; end
    
    #
    # Rescale pixels image to the specified width and height; this calls
    # #resize to adjust the client and server side representations.
    #
    def scale(w, h); end
    
    # Mirror the bitmap horizontally and/or vertically
    def mirror(horizontal, vertical); end
    
    # Rotate bitmap by _degrees_ degrees (counter-clockwise)
    def rotate(degrees); end
    
    #
    # Crop bitmap to given rectangle; this calls #resize to adjust the client
    # and server side representations.  The new bitmap may be smaller or larger
    # than the old one; blank areas are filled with _color_. There must be at
    # least one pixel of overlap between the old and the new bitmap.
    #
    def crop(x, y, w, h, color=false); end
    
    # Fill bitmap with uniform value
    def fill(color); end
  end
end
