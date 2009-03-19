module Fox
  #
  # X Bitmap (XBM) image
  #
  class FXXBMImage < FXImage
    #
    # Return the suggested file extension for this image type ("xbm").
    #
    def FXXBMImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXXBMImage.mimeType; end

    #
    # Return an initialized FXXBMImage instance.
    #
    # ==== Parameters:
    #
    # +a+::		an application instance [FXApp]
    # +pixels+::	a memory buffer formatted in XBM file format [String]
    # +mask+::		a memory buffer formatted in XBM file format [String]
    # +opts+::		options [Integer]
    # +width+::		width [Integer]
    # +height+::		height [Integer]
    #
    def initialize(a, pixels=nil, mask=nil, opts=0, width=1, height=1) # :yields: theXBMImage
    end
  end
end

