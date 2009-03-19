module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class AbsentFieldValidator < GenericValidator

      def initialize(field_name, options={})
        super
        @field_name, @options = field_name, options
      end

      def call(target)
        return true if target.send(field_name).blank?

        error_message = @options[:message] || ValidationErrors.default_error_message(:absent, field_name)
        add_error(target, error_message, field_name)

        return false
      end
    end # class AbsentFieldValidator

    module ValidatesAbsent

      ##
      # Validates that the specified attribute is "blank" via the attribute's
      # #blank? method.
      #
      # @note
      #   dm-core's support lib adds the #blank? method to many classes,
      # @see lib/dm-core/support/blank.rb (dm-core) for more information.
      #
      # @example [Usage]
      #   require 'dm-validations'
      #
      #   class Page
      #     include DataMapper::Resource
      #
      #     property :unwanted_attribute, String
      #     property :another_unwanted, String
      #     property :yet_again, String
      #
      #     validates_absent :unwanted_attribute
      #     validates_absent :another_unwanted, :yet_again
      #
      #     # a call to valid? will return false unless
      #     # all three attributes are blank
      #   end
      #
      def validates_absent(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::AbsentFieldValidator)
      end

    end # module ValidatesAbsent
  end # module Validate
end # module DataMapper
