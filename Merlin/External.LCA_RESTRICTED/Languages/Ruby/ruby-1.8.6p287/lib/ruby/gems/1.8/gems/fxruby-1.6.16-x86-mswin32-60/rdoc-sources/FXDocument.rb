module Fox
  #
  # Abstract base class for documents
  #
  # === Message identifiers
  #
  # +ID_TITLE+::	x
  # +ID_FILENAME+::	x
  #
  class FXDocument < FXObject

    # Modified state for the document [Boolean]
    attr_writer :modified
    
    # Document title
    attr_accessor :title
    
    # Document filename
    attr_accessor :filename

    # Return an initialized FXDocument instance
    def initialize # :yields: theDocument
    end
  
    # Return +true+ if document is modified
    def modified?; end
  end
end

