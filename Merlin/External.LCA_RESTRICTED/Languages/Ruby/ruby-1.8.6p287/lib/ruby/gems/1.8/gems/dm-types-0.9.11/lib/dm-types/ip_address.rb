require 'ipaddr'

module DataMapper
  module Types
    class IPAddress < DataMapper::Type
      primitive String

      def self.load(value, property)
        if value.nil?
          nil
        elsif value.is_a?(String) && !value.empty?
          IPAddr.new(value)
        elsif value.is_a?(String) && value.empty?
          IPAddr.new("0.0.0.0")
        else
          raise ArgumentError.new("+value+ must be nil or a String")
        end
      end

      def self.dump(value, property)
        return nil if value.nil?
        value.to_s
      end

      def self.typecast(value, property)
        value.kind_of?(IPAddr) ? value : load(value, property)
      end
    end # class IPAddress
  end # module Types
end # module DataMapper
