module Fox
  #
  # An FXStringDict (string dictionary) object maps one string to another
  # string. The inserted strings are copied when they're inserted.
  #
  class FXStringDict < FXDict
    #
    # Return an initialized FXStringDict instance.
    #
    def initialize; end
  
    #
    # Insert a new string indexed by key, with given mark flag.
    #
    def insert(key, value, mrk=false); end
  
    #
    # Replace or insert a new string indexed by key, unless given mark is lower than the existing mark.
    #
    def replace(key, value, mrk=false); end
  
    #
    # Remove entry indexed by key.
    #
    def remove(key); end
  
    #
    # Return the entry indexed by _key_, or nil if the key does not exist.
    #
    def find(key); end
  
    #
    # Return the string at integer position _pos_.
    #
    def data(pos); end
  end
end

