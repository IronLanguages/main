#require File.dirname(__FILE__) + '/formats/email'

require 'pathname'
require Pathname(__FILE__).dirname.expand_path + 'formats/email'
require Pathname(__FILE__).dirname.expand_path + 'formats/url'

module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class FormatValidator < GenericValidator

      FORMATS = {}
      include DataMapper::Validate::Format::Email
      include DataMapper::Validate::Format::Url

      def initialize(field_name, options = {}, &b)
        super(field_name, options)
        @field_name, @options = field_name, options
        @options[:allow_nil] = false unless @options.has_key?(:allow_nil)
      end

      def call(target)
        value = target.validation_property_value(field_name)
        return true if @options[:allow_nil] && value.nil?

        validation = @options[:as] || @options[:with]

        raise "No such predefined format '#{validation}'" if validation.is_a?(Symbol) && !FORMATS.has_key?(validation)
        validator = validation.is_a?(Symbol) ? FORMATS[validation][0] : validation

        valid = case validator
          when Proc   then validator.call(value)
          when Regexp then value =~ validator
          else
            raise UnknownValidationFormat, "Can't determine how to validate #{target.class}##{field_name} with #{validator.inspect}"
        end

        return true if valid

        error_message = @options[:message] || ValidationErrors.default_error_message(:invalid, field_name)

        field = Extlib::Inflection.humanize(field_name)
        error_message = error_message.call(field, value) if error_message.respond_to?(:call)

        add_error(target, error_message, field_name)

        false
      end

      #class UnknownValidationFormat < StandardError; end

    end # class FormatValidator

    module ValidatesFormat

      ##
      # Validates that the attribute is in the specified format. You may use the
      # :as (or :with, it's an alias) option to specify the pre-defined format
      # that you want to validate against. You may also specify your own format
      # via a Proc or Regexp passed to the the :as or :with options.
      #
      # @option :allow_nil<Boolean>         true/false (default is true)
      # @option :as<Format, Proc, Regexp>   the pre-defined format, Proc or Regexp to validate against
      # @option :with<Format, Proc, Regexp> an alias for :as
      #
      # @details [Pre-defined Formats]
      #   :email_address (format is specified in DataMapper::Validate::Format::Email)
      #   :url (format is specified in DataMapper::Validate::Format::Url)
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Page
      #     include DataMapper::Resource
      #
      #     property :email, String
      #     property :zip_code, String
      #
      #     validates_format :email, :as => :email_address
      #     validates_format :zip_code, :with => /^\d{5}$/
      #
      #     # a call to valid? will return false unless:
      #     # email is formatted like an email address
      #     # and
      #     # zip_code is a string of 5 digits
      #
      def validates_format(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::FormatValidator)
      end

    end # module ValidatesFormat
  end # module Validate
end # module DataMapper
