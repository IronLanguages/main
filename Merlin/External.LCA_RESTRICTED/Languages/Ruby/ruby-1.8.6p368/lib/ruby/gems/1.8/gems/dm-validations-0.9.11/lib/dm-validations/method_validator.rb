module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class MethodValidator < GenericValidator

      def initialize(field_name, options={})
        super
        @field_name, @options = field_name, options.clone
        @options[:method] = @field_name unless @options.has_key?(:method)
      end

      def call(target)
        result, message = target.send(@options[:method])
        add_error(target, message, field_name) unless result
        result
      end

      def ==(other)
        @options[:method] == other.instance_variable_get(:@options)[:method] && super
      end
    end # class MethodValidator

    module ValidatesWithMethod

      ##
      # Validate using the given method. The method given needs to return:
      # [result::<Boolean>, Error Message::<String>]
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Page
      #     include DataMapper::Resource
      #
      #     property :zip_code, String
      #
      #     validates_with_method :in_the_right_location?
      #
      #     def in_the_right_location?
      #       if @zip_code == "94301"
      #         return true
      #       else
      #         return [false, "You're in the wrong zip code"]
      #       end
      #     end
      #
      #     # A call to valid? will return false and
      #     # populate the object's errors with "You're in the
      #     # wrong zip code" unless zip_code == "94301"
      #
      #     # You can also specify field:
      #
      #     validates_with_method :zip_code, :in_the_right_location?
      #
      #     # it will add returned error message to :zip_code field
      #
      def validates_with_method(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::MethodValidator)
      end

    end # module ValidatesWithMethod
  end # module Validate
end # module DataMapper
