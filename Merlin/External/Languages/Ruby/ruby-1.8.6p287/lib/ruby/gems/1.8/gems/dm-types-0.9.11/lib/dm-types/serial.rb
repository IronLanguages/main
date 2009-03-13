module DataMapper
  module Types
    class Serial < DataMapper::Type
      primitive Integer
      serial true
    end # class Serial
  end # module Types
end # module DataMapper
