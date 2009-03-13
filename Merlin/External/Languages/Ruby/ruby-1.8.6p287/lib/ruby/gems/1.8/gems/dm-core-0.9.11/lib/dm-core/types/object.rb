require "base64"

module DataMapper
  module Types
    class Object < DataMapper::Type
      primitive String
      size 65535
      lazy true
      track :hash

      def self.typecast(value, property)
        value
      end

      def self.dump(value, property)
        Base64.encode64(Marshal.dump(value))
      end

      def self.load(value, property)
        value.nil? ? nil : Marshal.load(Base64.decode64(value))
      end
    end
  end
end
