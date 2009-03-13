# FIXME: can we alias this to the class Text if it isn't already defined?
module DataMapper
  module Types
    class Serial < DataMapper::Type
      primitive Integer
      serial true
    end # class Text
  end # module Types
end # module DataMapper
