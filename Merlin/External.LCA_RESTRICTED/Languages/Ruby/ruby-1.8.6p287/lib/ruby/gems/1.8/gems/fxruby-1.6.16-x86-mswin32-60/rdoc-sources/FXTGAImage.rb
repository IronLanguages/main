module Fox
  #
  # Targa Image
  #
  class FXTGAImage < FXImage
    #
    # Return the suggested file extension for this image type ("tga").
    #
    def FXTGAImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXTGAImage.mimeType; end

    #
    # Return an initialized FXTGAImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in Targa file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theTGAImage
    end
  end
end

