module Fox
  #
  # X Pixmap (XPM) Image
  #
  class FXXPMImage < FXImage
    #
    # Return the suggested file extension for this image type ("xpm").
    #
    def FXXPMImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXXPMImage.mimeType; end

    #
    # Return an initialized FXXPMImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in XPM file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theXPMImage
    end
  end
end

