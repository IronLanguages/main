module DataMapper
  module Types
    class Enum < DataMapper::Type(Integer)
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
        enum = Class.new(Enum)
        enum.flag_map = {}

        flags.each_with_index do |flag, i|
          enum.flag_map[i + 1] = flag
        end

        enum
      end

      def self.[](*flags)
        new(*flags)
      end

      def self.load(value, property)
        self.flag_map[value]
      end

      def self.dump(value, property)
        case value
          when Array then value.collect { |v| self.dump(v, property) }
          else            self.flag_map.invert[value]
        end
      end

      def self.typecast(value, property)
        # Attempt to typecast using the class of the first item in the map.
        return value if value.nil?
        case self.flag_map[1]
          when Symbol then value.to_sym
          when String then value.to_s
          when Fixnum then value.to_i
          else             value
        end
      end
    end # class Enum
  end # module Types

  if defined?(Validate)
    module Validate
      module AutoValidate
        alias :orig_auto_generate_validations :auto_generate_validations
        def auto_generate_validations(property)
          orig_auto_generate_validations(property)
          return unless property.options[:auto_validation]

          if property.type.ancestors.include?(Types::Enum)
            validates_within property.name, options_with_message({:set => property.type.flag_map.values}, property, :within)
          end
        end
      end
    end
  end
end # module DataMapper
