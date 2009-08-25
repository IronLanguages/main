require 'rubygems'

gem 'addressable', '~>2.0.2'
require 'addressable/uri'

module DataMapper
  module Types
    class URI < DataMapper::Type
      primitive String

      def self.load(value, property)
        Addressable::URI.parse(value)
      end

      def self.dump(value, property)
        return nil if value.nil?
        value.to_s
      end

      def self.typecast(value, property)
        if value.kind_of?(Addressable::URI)
          value
        elsif value.nil?
          load(nil, property)
        else
          load(value.to_s, property)
        end
      end
    end # class URI
  end # module Types
end # module DataMapper
