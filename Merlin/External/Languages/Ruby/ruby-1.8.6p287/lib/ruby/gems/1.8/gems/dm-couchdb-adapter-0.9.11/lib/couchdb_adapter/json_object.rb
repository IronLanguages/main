require 'json'

# Non-lazy objects that serialize to/from JSON, for use with couchdb
module DataMapper
  module Types
    class JsonObject < DataMapper::Type
      primitive String
      size 65535

      def self.load(value, property)
        value.nil? ? nil : value
      end

      def self.dump(value, property)
        value.nil? ? nil : value
      end

      def self.typecast(value, property)
        value
      end
    end # class JsonObject
  end # module Types
end # module DataMapper
