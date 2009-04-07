module Fox
  #
  # Microsoft Bitmap image.
  #
  class FXBMPImage < FXImage
    #
    # Return the suggested file extension for this image type ("bmp").
    #
    def FXBMPImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXBMPImage.mimeType; end
    
    #
    # Return an initialized FXBMPImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in BMP file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1)	# :yields: theBMPImage
    end
  end
end
