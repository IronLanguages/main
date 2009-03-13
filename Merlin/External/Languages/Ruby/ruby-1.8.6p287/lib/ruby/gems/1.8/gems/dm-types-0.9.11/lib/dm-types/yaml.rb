require 'yaml'

module DataMapper
  module Types
    class Yaml < DataMapper::Type
      primitive Text
      lazy true

      def self.load(value, property)
        if value.nil?
          nil
        elsif value.is_a?(String)
          ::YAML.load(value)
        else
          raise ArgumentError.new("+value+ must be nil or a String")
        end
      end

      def self.dump(value, property)
        if value.nil?
          nil
        elsif value.is_a?(String) && value =~ /^---/
          value
        else
          ::YAML.dump(value)
        end
      end

      def self.typecast(value, property)
        # No typecasting; leave values exactly as they're provided.
        value
      end
    end # class Yaml
  end # module Types
end # module DataMapper
