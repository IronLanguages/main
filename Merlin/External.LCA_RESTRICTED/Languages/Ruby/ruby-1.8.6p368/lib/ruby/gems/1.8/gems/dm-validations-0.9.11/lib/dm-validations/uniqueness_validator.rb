module DataMapper
  module Validate

    ##
    #
    # @author Guy van den Berg
    # @since  0.9
    class UniquenessValidator < GenericValidator
      include Extlib::Assertions

      def initialize(field_name, options = {})
        assert_kind_of 'scope', options[:scope], Array, Symbol if options.has_key?(:scope)
        super
        @field_name, @options = field_name, options
      end

      def call(target)
        scope = Array(@options[:scope])

        return true if @options[:allow_nil] && target.send(field_name).nil?

        repository_name = target.repository.name

        opts = {
          :fields    => target.model.key,
          field_name => target.validation_property_value(field_name)
        }

        scope.each do |item|
          if target.model.properties(repository_name).has_property?(item)
            opts[item] = target.validation_property_value(item)
          elsif target.model.relationships(repository_name).has_key?(item)
            target.validation_association_keys(item).each do |key|
              opts[key] = target.validation_property_value(key)
            end
          end
        end

        resource = repository(repository_name) { target.model.first(opts) }

        return true if resource.nil?

        # is target and found resource identic? same instance... but not ==
        return true if !target.new_record? && resource.repository.name == repository_name && resource.model == target.model && resource.key == target.key

        error_message = @options[:message] || ValidationErrors.default_error_message(:taken, field_name)
        add_error(target, error_message, field_name)

        return false
      end
    end # class UniquenessValidator

    module ValidatesIsUnique

      # Validate the uniqueness of a field
      #
      def validates_is_unique(*fields)
        opts = opts_from_validator_args(fields)
        add_validator_to_context(opts, fields, DataMapper::Validate::UniquenessValidator)
      end

    end # module ValidatesIsUnique
  end # module Validate
end # module DataMapper
