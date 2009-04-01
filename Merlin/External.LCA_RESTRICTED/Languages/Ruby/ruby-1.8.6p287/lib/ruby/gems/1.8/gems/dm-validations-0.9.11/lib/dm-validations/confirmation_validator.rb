module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class ConfirmationValidator < GenericValidator

      def initialize(field_name, options = {})
        super
        @options = options
        @field_name, @confirm_field_name = field_name, (options[:confirm] || "#{field_name}_confirmation").to_sym
        @options[:allow_nil] = true unless @options.has_key?(:allow_nil)
      end

      def call(target)
        unless valid?(target)
          error_message = @options[:message] || ValidationErrors.default_error_message(:confirmation, field_name)
          add_error(target, error_message, field_name)
          return false
        end

        return true
      end

      def valid?(target)
        field_value = target.send(field_name)
        return true if @options[:allow_nil] && field_value.nil?
        return false if !@options[:allow_nil] && field_value.nil?

        if target.class.properties.has_property?(field_name)
          return true unless target.attribute_dirty?(field_name)
        end

        confirm_value = target.instance_variable_get("@#{@confirm_field_name}")
        field_value == confirm_value
      end

    end # class ConfirmationValidator

    module ValidatesIsConfirmed

      ##
      # Validates that the given attribute is confirmed by another attribute.
      # A common use case scenario is when you require a user to confirm their
      # password, for which you use both password and password_confirmation
      # attributes.
      #
      # @option :allow_nil<Boolean> true/false (default is true)
      # @option :confirm<Symbol>    the attribute that you want to validate
      #                             against (default is firstattr_confirmation)
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Page
      #     include DataMapper::Resource
      #
      #     property :password, String
      #     property :email, String
      #     attr_accessor :password_confirmation
      #     attr_accessor :email_repeated
      #
      #     validates_is_confirmed :password
      #     validates_is_confirmed :email, :confirm => :email_repeated
      #
      #     # a call to valid? will return false unless:
      #     # password == password_confirmation
      #     # and
      #     # email == email_repeated
      #
      def validates_is_confirmed(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::ConfirmationValidator)
      end

    end # module ValidatesIsConfirmed
  end # module Validate
end # module DataMapper
