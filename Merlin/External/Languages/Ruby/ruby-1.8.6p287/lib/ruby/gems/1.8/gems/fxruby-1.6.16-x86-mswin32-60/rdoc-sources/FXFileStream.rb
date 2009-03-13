module Fox
  # File Store Definition
  class FXFileStream < FXStream

    attr_reader :position

    #
    # Return an initialized FXFileStream instance.
    #
    def initialize(cont=nil) # :yields: theFileStream
    end
  
    #
    # Open binary data file stream; allocate a buffer of the given _size_
    # for the file I/O; the buffer must be at least 16 bytes. Returns
    # +true+ on success, +false+ on failure.
    #
    # ==== Parameters:
    #
    # +filename+::	name of the file to open [String]
    # +save_or_load+::	access mode, either +FXStreamSave+ or +FXStreamLoad+ [Integer]
    # +size+::		buffer size [Integer]
    #
    def open(filename, save_or_load, size=8192); end
  end
end

