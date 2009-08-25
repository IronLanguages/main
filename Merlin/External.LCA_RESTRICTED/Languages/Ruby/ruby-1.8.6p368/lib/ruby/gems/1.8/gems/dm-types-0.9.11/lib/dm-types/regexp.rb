module DataMapper
  module Types
    class Regexp < DataMapper::Type
      primitive String

      def self.load(value, property)
        ::Regexp.new(value) unless value.nil?
      end

      def self.dump(value, property)
        return nil if value.nil?
        value.source
      end

      def self.typecast(value, property)
        value.kind_of?(::Regexp) ? value : load(value, property)
      end
    end
  end
end
