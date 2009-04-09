module DataMapper
  module Types
    class Flag < DataMapper::Type(Integer)
      def self.inherited(target)
        target.instance_variable_set("@primitive", self.primitive)
      end

      def self.flag_map
        @flag_map
      end

      def self.flag_map=(value)
        @flag_map = value
      end

      def self.new(*flags)
        type = Class.new(Flag)
        type.flag_map = {}

        flags.each_with_index do |flag, i|
          type.flag_map[2 ** i] = flag
        end

        type
      end

      def self.[](*flags)
        new(*flags)
      end

      def self.load(value, property)
        begin
          matches = []

          return [] if value.nil? || (value <= 0)
          0.upto((Math.log(value) / Math.log(2)).ceil) do |i|
            pow = 2 ** i
            matches << flag_map[pow] if value & pow == pow
          end

          matches.compact
        rescue TypeError, Errno::EDOM
          []
        end
      end

      def self.dump(value, property)
        return if value.nil?
        flags = value.is_a?(Array) ? value : [value]
        flags.map!{ |f| f.to_sym }
        flag_map.invert.values_at(*flags.flatten).compact.inject(0) { |sum, i| sum + i }
      end

      def self.typecast(value, property)
        case value
        when nil   then nil
        when Array then value.map {|v| v.to_sym}
        else value.to_sym
        end
      end
    end # class Flag
  end # module Types
end # module DataMapper
