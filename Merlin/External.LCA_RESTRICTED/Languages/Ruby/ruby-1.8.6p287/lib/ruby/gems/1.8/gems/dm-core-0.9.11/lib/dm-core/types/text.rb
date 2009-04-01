# FIXME: can we alias this to the class Text if it isn't already defined?
module DataMapper
  module Types
    class Text < DataMapper::Type
      primitive String
      size 65535
      lazy true
    end # class Text
  end # module Types
end # module DataMapper
