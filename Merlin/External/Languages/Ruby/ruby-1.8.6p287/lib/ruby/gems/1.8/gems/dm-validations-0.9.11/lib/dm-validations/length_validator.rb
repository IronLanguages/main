module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class LengthValidator < GenericValidator

      def initialize(field_name, options)
        super
        @field_name = field_name
        @options = options

        @min = options[:minimum] || options[:min]
        @max = options[:maximum] || options[:max]
        @equal = options[:is] || options[:equals]
        @range = options[:within] || options[:in]

        @validation_method ||= :range if @range
        @validation_method ||= :min if @min && @max.nil?
        @validation_method ||= :max if @max && @min.nil?
        @validation_method ||= :equals unless @equal.nil?
      end

      def call(target)
        field_value = target.validation_property_value(field_name)
        return true if @options[:allow_nil] && field_value.nil?

        field_value = '' if field_value.nil?

        # XXX: HACK seems hacky to do this on every validation, probably should
        #      do this elsewhere?
        field = Extlib::Inflection.humanize(field_name)
        min = @range ? @range.min : @min
        max = @range ? @range.max : @max
        equal = @equal

        case @validation_method
        when :range then
          unless valid = @range.include?(field_value.size)
            error_message = ValidationErrors.default_error_message(:length_between, field, min, max)
          end
        when :min then
          unless valid = field_value.size >= min
            error_message = ValidationErrors.default_error_message(:too_short, field, min)
          end
        when :max then
          unless valid = field_value.size <= max
            error_message = ValidationErrors.default_error_message(:too_long, field, max)
          end
        when :equals then
          unless valid = field_value.size == equal
            error_message = ValidationErrors.default_error_message(:wrong_length, field, equal)
          end
        end

        error_message = @options[:message] || error_message

        add_error(target, error_message, field_name) unless valid

        return valid
      end

    end # class LengthValidator

    module ValidatesLength

      # Validates that the length of the attribute is equal to, less than,
      # greater than or within a certain range (depending upon the options
      # you specify).
      #
      # @option :allow_nil<Boolean> true/false (default is true)
      # @option :minimum    ensures that the attribute's length is greater than
      #   or equal to the supplied value
      # @option :min        alias for :minimum
      # @option :maximum    ensures the attribute's length is less than or equal
      #   to the supplied value
      # @option :max        alias for :maximum
      # @option :equals     ensures the attribute's length is equal to the
      #   supplied value
      # @option :is         alias for :equals
      # @option :in<Range>  given a Range, ensures that the attributes length is
      #   include?'ed in the Range
      # @option :within<Range>  alias for :in
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Page
      #     include DataMapper::Resource
      #
      #     property high, Integer
      #     property low, Integer
      #     property just_right, Integer
      #
      #     validates_length :high, :min => 100000000000
      #     validates_length :low, :equals => 0
      #     validates_length :just_right, :within => 1..10
      #
      #     # a call to valid? will return false unless:
      #     # high is greater than or equal to 100000000000
      #     # low is equal to 0
      #     # just_right is between 1 and 10 (inclusive of both 1 and 10)
      #
      def validates_length(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::LengthValidator)
      end

    end # module ValidatesLength
  end # module Validate
end # module DataMapper
