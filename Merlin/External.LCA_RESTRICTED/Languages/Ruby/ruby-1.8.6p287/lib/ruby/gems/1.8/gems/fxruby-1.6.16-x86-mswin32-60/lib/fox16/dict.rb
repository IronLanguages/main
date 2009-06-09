#
# Adds some methods similar to those for Ruby's Hash class
# to FOX's FXDict class.
#
module Fox
  class FXDict
    #
    # Returns a new array populated with the keys from this dictionary.
    #
    def keys
      ary = []
      each_key { |k| ary << k }
      ary
    end

    #
    # Iterate over the keys in this dictionary.
    #
    def each_key
      pos = first
      while pos < self.getTotalSize()
        yield key(pos)
        pos = self.next(pos)
      end
    end
    
    #
    # Returns +true+ if this dictionary contains no key-value pairs.
    #
    def empty?
      self.size == 0
    end
  end
end

