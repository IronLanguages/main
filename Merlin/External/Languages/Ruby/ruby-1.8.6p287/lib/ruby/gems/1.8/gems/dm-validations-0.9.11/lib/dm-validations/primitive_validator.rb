module DataMapper
  module Validate

    ##
    #
    # @author Dirkjan Bussink
    # @since  0.9
    class PrimitiveValidator < GenericValidator

      def initialize(field_name, options={})
        super
        @field_name, @options = field_name, options
      end

      def call(target)
        value = target.validation_property_value(field_name)
        property = target.validation_property(field_name)
        return true if value.nil? || value.kind_of?(property.primitive) || property.primitive == TrueClass && value.kind_of?(FalseClass)

        error_message = @options[:message] || default_error(property)
        add_error(target, error_message, field_name)

        false
      end

      protected

      def default_error(property)
        ValidationErrors.default_error_message(:primitive, field_name, property.primitive)
      end

    end # class PrimitiveValidator

    module ValidatesIsPrimitive

      ##
      # Validates that the specified attribute is of the correct primitive type.
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Person
      #     include DataMapper::Resource
      #
      #     property :birth_date, Date
      #
      #     validates_is_primitive :birth_date
      #
      #     # a call to valid? will return false unless
      #     # the birth_date is something that can be properly
      #     # casted into a Date object.
      #   end
      def validates_is_primitive(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::PrimitiveValidator)
      end

    end # module ValidatesPresent
  end # module Validate
end # module DataMapper
