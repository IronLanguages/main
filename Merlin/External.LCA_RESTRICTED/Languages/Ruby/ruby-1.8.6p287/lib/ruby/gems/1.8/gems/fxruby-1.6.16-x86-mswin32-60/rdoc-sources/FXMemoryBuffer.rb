module Fox
  class FXMemoryBuffer

    #
    # Return a new FXMemoryBuffer instance, initialized with the
    # provided array of FXColor values.
    #
    # ==== Parameters:
    #
    # +data+::	the initial array of FXColor values.
    #
    def initialize(data); end

    # Return a copy of the pixel buffer, as an array of FXColor values [Array]
    def data; end
    alias to_a data

    # Return the size of the pixel buffer
    def size; end

    # Return the specified element (an FXColor value)
    def [](index); end
    
    # Set the specified element to _clr_.
    def []=(index, clr); end
  end
end

