module Fox
  #
  # A stream is a way to serialize data and objects into a byte stream.
  # Each item of data that is saved or loaded from the stream may be byte-swapped,
  # thus allowing little-endian machines to read data produced on big endian ones
  # and vice-versa.
  # Data is serialized exactly as-is.  There are no tags or other markers
  # inserted into the stream; thus, the stream may be used to save or load arbitrary
  # binary data.
  # Objects derived from FXObjects may be serialized also; whenever a reference to an
  # object is serialized, a table is consulted to determine if the same object has
  # been encountered previously; if not, the object is added to the table and then
  # its contents are serialized.  If the object has been encountered before, only a
  # reference to the object is serialized.
  # When loading back a serialized object, new instances are constructed using
  # the default constructor, and subsequently the object's contents are loaded.
  # A special container object may be passed in which is placed in the table
  # as if it had been encountered before; this will cause only references to this
  # object to be saved.  The container object is typically the top-level document
  # object which manages all objects contained by it.  Additional objects may be
  # added using addObject(); these will not be actually saved or loaded.
  #
  # === Stream status codes
  #
  # +FXStreamOK+::		OK
  # +FXStreamEnd+::		Try read past end of stream
  # +FXStreamFull+::		Filled up stream buffer or disk full
  # +FXStreamNoWrite+::		Unable to open for write
  # +FXStreamNoRead+::		Unable to open for read
  # +FXStreamFormat+::		Stream format error
  # +FXStreamUnknown+::		Trying to read unknown class
  # +FXStreamAlloc+::		Alloc failed
  # +FXStreamFailure+::		General failure
  #
  # === Stream data flow direction
  #
  # +FXStreamDead+::		Unopened stream
  # +FXStreamSave+::		Saving stuff to stream
  # +FXStreamLoad+::		Loading stuff from stream
  #
  # === Stream seeking
  #
  # +FXFromStart+::		Seek from start position
  # +FXFromCurrent+::		Seek from current position
  # +FXFromEnd+::		Seek from end position
  #
  class FXStream

    # Stream status [Integer]
    attr_reader :status

    # Stream direction, one of +FXStreamSave+, +FXStreamLoad+ or +FXStreamDead+.
    attr_reader :direction
  
    # Parent object [FXObject]
    attr_reader :container

    # Available buffer space
    attr_accessor :space

    # Stream position (an offset from the beginning of the stream) [Integer]
    attr_accessor :position
  
    #
    # Construct stream with given container object.  The container object
    # is an object that will itself not be saved to or loaded from the stream,
    # but which may be referenced by other objects.  These references will be
    # properly saved and restored.
    #
    # ==== Parameters:
    #
    # +cont+::	the container object, or +nil+ if there is none [FXObject].
    #
    def initialize(cont=nil) # :yields: theStream
    end
  
    #
    # Open stream for reading or for writing.
    # An initial buffer size may be given, which must be at least 16 bytes.
    # If _data_ is not +nil+, it is expected to point to an external data buffer
    # of length _size_; otherwise the stream will use an internally managed buffer.
    # Returns +true+ on success, +false+ otherwise.
    #
    # ==== Parameters:
    #
    # +save_or_load+::	access mode, either +FXStreamSave+ or +FXStreamLoad+ [Integer]
    # +size+::		initial buffer size [Integer]
    # +data+::		external data buffer (if any) [String]
    #
    def open(save_or_load, size=8192, data=nil); end
  
    #
    # Close stream; returns +true+ if OK.
    #
    def close(); end
  
    #
    # Flush buffer
    #
    def flush(); end

    #
    # Get available buffer space
    #
    def getSpace(); end
  
    #
    # Set available buffer space
    #
    def setSpace(sp); end

    #
    # Return +true+ if at end of file or error.
    #
    def eof?; end

    #
    # Set status code, where _err_ is one of the stream status
    # codes listed above.
    #
    def error=(err); end

    # Set the byte-swapped flag to +true+ or +false+.
    def bytesSwapped=(swapBytes); end
    
    # Returns +true+ if bytes are swapped for this stream
    def bytesSwapped?; end
  
    #
    # Set stream to big endian mode if +true+. Byte swapping will
    # be enabled if the machine native byte order is not equal to
    # the desired byte order.
    #
    def bigEndian=(big); end

    #
    # Return +true+ if big endian mode.
    #
    def bigEndian?; end
  end
end

