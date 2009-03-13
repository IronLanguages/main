module Fox
  #
  # PCX graphics file
  #
  class FXPCXImage < FXImage
    #
    # Return the suggested file extension for this image type ("pcx").
    #
    def FXPCXImage.fileExt; end
    
    #
    # Return the MIME type for this image type
    #
    def FXPCXImage.mimeType; end

    #
    # Return an initialized FXPCXImage instance.
    #
    # ==== Parameters:
    #
    # +a+::	an application instance [FXApp]
    # +pix+::	a memory buffer formatted in PCX file format [String]
    # +opts+::	options [Integer]
    # +width+::	width [Integer]
    # +height+::	height [Integer]
    #
    def initialize(a, pix=nil, opts=0, width=1, height=1) # :yields: thePCXImage
    end
  end
end

