#require File.dirname(__FILE__) + '/formats/email'

module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class CustomValidator < GenericValidator

      def initialize(field_name, options = {}, &b)
        #super(field_name, options)
        #@field_name, @options = field_name, options
        #@options[:allow_nil] = false unless @options.has_key?(:allow_nil)
      end

      def call(target)
        #field_value = target.instance_variable_get("@#{@field_name}")
        #return true if @options[:allow_nil] && field_value.nil?

        #validation = (@options[:as] || @options[:with])
        #error_message = nil

        # Figure out what to use as the actual validator.
        # If a symbol is passed to :as, look up
        # the canned validation in FORMATS.
        #
        #validator = if validation.is_a? Symbol
        #  if FORMATS[validation].is_a? Array
        #    error_message = FORMATS[validation][1]
        #    FORMATS[validation][0]
        #  else
        #    FORMATS[validation] || validation
        #  end
        #else
        #  validation
        #end

        #valid = case validator
        #when Proc then validator.call(field_value)
        #when Regexp then validator =~ field_value
        #else raise UnknownValidationFormat, "Can't determine how to validate #{target.class}##{@field_name} with #{validator.inspect}"
        #end

        #unless valid
        #  field = Inflector.humanize(@field_name)
        #  value = field_value
        #
        #  error_message = @options[:message] || error_message || '%s is invalid'.t(field)
        #  error_message = error_message.call(field, value) if Proc === error_message
        #
        #  add_error(target, error_message, @field_name)
        #end

        #return valid
      end

      #class UnknownValidationFormat < StandardError; end

    end # class CustomValidator

    module ValidatesFormatOf

      #def validates_format_of(*fields)
        #opts = opts_from_validator_args(fields)
        #add_validator_to_context(opts, fields, DataMapper::Validate::FormatValidator)
      #end

    end # module ValidatesFormatOf
  end # module Validate
end # module DataMapper
