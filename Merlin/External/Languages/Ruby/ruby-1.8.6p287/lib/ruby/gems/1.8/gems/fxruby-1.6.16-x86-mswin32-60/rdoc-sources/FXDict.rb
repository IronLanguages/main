module Fox
  #
  # The dictionary class maintains a fast-access hash table of entities
  # indexed by a character string.  
  # It is typically used to map strings to pointers; however, overloading
  # the #createData and #deleteData members allows any type of data to
  # be indexed by strings.
  #
  class FXDict < FXObject
  
    # Total number of entries in the table [Integer]
    attr_reader :length

    # Position of first filled slot, or >= total [Integer]
    attr_reader :first

    # Position of last filled slot, or -1 [Integer]
    attr_reader :last

    alias size length
    
    #
    # Construct an empty dictionary.
    #
    def initialize ; end
    
    #
    # Return key at position _pos_.
    #
    def key(pos) ; end

    #
    # Return mark flag of entry at position _pos_.
    #
    def marked?(pos) ; end

    #
    # Return position of next filled slot after _pos_ in the hash table,
    # or a value greater than or equal to total if no filled 
    # slot was found.
    #
    def next(pos) ; end

    #
    # Return position of previous filled slot before _pos_ in the hash table,
    # or a -1 if no filled slot was found.
    #
    def prev(pos) ; end

    #
    # Clear all entries
    #
    def clear() ; end

    #
    # Iterate over the keys in this dictionary.
    #
    def each_key # :yields: key
    end

    #
    # Returns a new array populated with the keys from this dictionary.
    #
    def keys() ; end

    #
    # Returns +true+ if the given _key_ is present.
    #
    def has_key?(key) ; end
    
    alias include? has_key?
    alias member?  has_key?
    
    #
    # Returns +true+ if this dictionary contains no key-value pairs.
    #
    def empty?() ; end
  end
end
