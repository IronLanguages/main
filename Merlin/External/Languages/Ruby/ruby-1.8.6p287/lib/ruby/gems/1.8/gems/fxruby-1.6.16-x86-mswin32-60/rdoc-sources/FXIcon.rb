module Fox
  #
  # An Icon is an image with two additional server-side resources: a shape
  # bitmap, which is used to mask those pixels where the background should
  # be preserved during the drawing, and a etch bitmap, which is used to
  # draw the icon when it is disabled.
  #
  class FXIcon < FXImage

    # Transparency color [FXColor]
    attr_accessor :transparentColor

    #
    # Create an icon with an initial pixel buffer _pix_, a transparent color _clr_,
    # and _opts_ as in FXImage.  The transparent color is used to determine which
    # pixel values are transparent, i.e. need to be masked out in the absence of
    # a true alpha channel.
    # If the flag +IMAGE_OPAQUE+ is passed, the shape and etch bitmaps are generated
    # as if the image is fully opaque, even if it has an alpha channel or transparancy
    # color.  The flag +IMAGE_ALPHACOLOR+ is used to force a specific alpha color instead
    # of the alpha channel obtained from the image file.
    # Specifying +IMAGE_ALPHAGUESS+ causes FXIcon to obtain the alpha color from the background
    # color of the image; it has the same effect as +IMAGE_ALPHACOLOR+ in the sense that
    # the icon will be transparent for those colors matching the alpha color.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	initial pixel buffer [String]
    # +clr+::	transparency color [FXColor]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(app, pix=nil, clr=0, opts=0, width=1, height=1) # :yields: theIcon
    end
  end
end
