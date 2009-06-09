module Fox
  #
  # A FXMemoryStream is a stream that reads from (or writes to) a buffer of bytes in memory.
  # That buffer may "owned" by either the application code or by the stream object itself.
  # In the latter case, the stream object will dispose of the buffer contents when the stream
  # is closed.
  #
  class FXMemoryStream < FXStream

    attr_reader :position

    #
    # Construct a new memory stream with given container object.
    # The container object is an object that will itself not be
    # saved to or loaded from the stream, but which may be referenced
    # by other objects. These references will be properly saved and restored.
    #
    # ==== Parameters:
    #
    # +cont+::	the container object, or +nil+ if there is none [FXObject].
    #
    def initialize(cont=nil) # :yields: theMemoryStream
    end
  
    #
    # Open memory stream for reading or writing.
    # Returns +true+ if successful, +false+ otherwise.
    #
    # ==== Parameters:
    #
    # +save_or_load+:: access mode, either +FXStreamSave+ or +FXStreamLoad+ [Integer]
    # +data+::         memory buffer to be used for the stream, or +nil+ if the stream object should allocate its own buffer [String]
    #
    def open(save_or_load, data); end
    
    #
    # Take buffer away from stream, thus transferring ownership of the buffer
    # from the stream object to the caller.
    # Returns a string containing the buffer contents.
    #
    def takeBuffer; end
    
    #
    # Give buffer (a string) to this stream, thus transferring ownership of
    # the buffer from the caller to the stream object.
    #
    def giveBuffer(buffer); end
  end
end

