module Fox
  #
  # ICO graphics file
  #
  class FXICOImage < FXImage
    #
    # Return the suggested file extension for this image type ("ico").
    #
    def FXICOImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXICOImage.mimeType; end

    #
    # Return an initialized FXICOImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in ICO file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: theICOImage
    end
  end
end

