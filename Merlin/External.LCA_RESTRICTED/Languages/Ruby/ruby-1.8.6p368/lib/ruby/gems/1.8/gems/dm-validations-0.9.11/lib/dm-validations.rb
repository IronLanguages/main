require 'rubygems'
require 'pathname'

gem 'dm-core', '0.9.11'
require 'dm-core'

dir = Pathname(__FILE__).dirname.expand_path / 'dm-validations'

require dir / 'validation_errors'
require dir / 'contextual_validators'
require dir / 'auto_validate'

require dir / 'generic_validator'
require dir / 'required_field_validator'
require dir / 'primitive_validator'
require dir / 'absent_field_validator'
require dir / 'confirmation_validator'
require dir / 'format_validator'
require dir / 'length_validator'
require dir / 'within_validator'
require dir / 'numeric_validator'
require dir / 'method_validator'
require dir / 'block_validator'
require dir / 'uniqueness_validator'
require dir / 'acceptance_validator'
require dir / 'custom_validator'

require dir / 'support' / 'object'

module DataMapper
  module Validate

    def self.included(model)
      model.class_eval <<-EOS, __FILE__, __LINE__
        if method_defined?(:save)
          before :save, :check_validations
        end

        class << self
          def create(attributes = {}, context = :default)
            resource = new(attributes)
            return resource unless resource.valid?(context)
            resource.save!
            resource
          end

          def create!(attributes = {})
            resource = new(attributes)
            resource.save!
            resource
          end
        end
      EOS
    end

    # Ensures the object is valid for the context provided, and otherwise
    # throws :halt and returns false.
    #
    def check_validations(context = :default)
      throw(:halt, false) unless context.nil? || valid?(context)
    end

    # Calls save with a context of nil, thus skipping validations.
    #
    def save!
      save(nil)
    end

    # Return the ValidationErrors
    #
    def errors
      @errors ||= ValidationErrors.new
    end

    # Mark this resource as validatable. When we validate associations of a
    # resource we can check if they respond to validatable? before trying to
    # recursivly validate them
    #
    def validatable?
      true
    end

    # Alias for valid?(:default)
    #
    def valid_for_default?
      valid?(:default)
    end

    # Check if a resource is valid in a given context
    #
    def valid?(context = :default)
      self.class.validators.execute(context, self)
    end

    # Begin a recursive walk of the model checking validity
    #
    def all_valid?(context = :default)
      recursive_valid?(self, context, true)
    end

    # Do recursive validity checking
    #
    def recursive_valid?(target, context, state)
      valid = state
      target.instance_variables.each do |ivar|
        ivar_value = target.instance_variable_get(ivar)
        if ivar_value.validatable?
          valid = valid && recursive_valid?(ivar_value, context, valid)
        elsif ivar_value.respond_to?(:each)
          ivar_value.each do |item|
            if item.validatable?
              valid = valid && recursive_valid?(item, context, valid)
            end
          end
        end
      end
      return valid && target.valid?
    end


    def validation_property_value(name)
      self.respond_to?(name, true) ? self.send(name) : nil
    end

    # Get the corresponding Resource property, if it exists.
    #
    # Note: DataMapper validations can be used on non-DataMapper resources.
    # In such cases, the return value will be nil.
    def validation_property(field_name)
      if respond_to?(:model) && (properties = model.properties(self.repository.name)) && properties.has_property?(field_name)
        properties[field_name]
      end
    end

    def validation_association_keys(name)
      if model.relationships.has_key?(name)
        result = []
        relation = model.relationships[name]
        relation.child_key.each do |key|
          result << key.name
        end
        return result
      end
      nil
    end

    module ClassMethods
      include DataMapper::Validate::ValidatesPresent
      include DataMapper::Validate::ValidatesAbsent
      include DataMapper::Validate::ValidatesIsConfirmed
      include DataMapper::Validate::ValidatesIsPrimitive
      include DataMapper::Validate::ValidatesIsAccepted
      include DataMapper::Validate::ValidatesFormat
      include DataMapper::Validate::ValidatesLength
      include DataMapper::Validate::ValidatesWithin
      include DataMapper::Validate::ValidatesIsNumber
      include DataMapper::Validate::ValidatesWithMethod
      include DataMapper::Validate::ValidatesWithBlock
      include DataMapper::Validate::ValidatesIsUnique
      include DataMapper::Validate::AutoValidate

      # Return the set of contextual validators or create a new one
      #
      def validators
        @validations ||= ContextualValidators.new
      end

      # Clean up the argument list and return a opts hash, including the
      # merging of any default opts. Set the context to default if none is
      # provided. Also allow :context to be aliased to :on, :when & group
      #
      def opts_from_validator_args(args, defaults = nil)
        opts = args.last.kind_of?(Hash) ? args.pop : {}
        context = :default
        context = opts[:context] if opts.has_key?(:context)
        context = opts.delete(:on) if opts.has_key?(:on)
        context = opts.delete(:when) if opts.has_key?(:when)
        context = opts.delete(:group) if opts.has_key?(:group)
        opts[:context] = context
        opts.mergs!(defaults) unless defaults.nil?
        opts
      end

      # Given a new context create an instance method of
      # valid_for_<context>? which simply calls valid?(context)
      # if it does not already exist
      #
      def create_context_instance_methods(context)
        name = "valid_for_#{context.to_s}?"           # valid_for_signup?
        if !self.instance_methods.include?(name)
          class_eval <<-EOS, __FILE__, __LINE__
            def #{name}                               # def valid_for_signup?
              valid?('#{context.to_s}'.to_sym)        #   valid?('signup'.to_sym)
            end                                       # end
          EOS
        end

        all = "all_valid_for_#{context.to_s}?"        # all_valid_for_signup?
        if !self.instance_methods.include?(all)
          class_eval <<-EOS, __FILE__, __LINE__
            def #{all}                                # def all_valid_for_signup?
              all_valid?('#{context.to_s}'.to_sym)    #   all_valid?('signup'.to_sym)
            end                                       # end
          EOS
        end
      end

      # Create a new validator of the given klazz and push it onto the
      # requested context for each of the attributes in the fields list
      #
      def add_validator_to_context(opts, fields, klazz)
        fields.each do |field|
          validator = klazz.new(field, opts)
          if opts[:context].is_a?(Symbol)
            unless validators.context(opts[:context]).include?(validator)
              validators.context(opts[:context]) << validator
              create_context_instance_methods(opts[:context])
            end
          elsif opts[:context].is_a?(Array)
            opts[:context].each do |c|
              unless validators.context(c).include?(validator)
                validators.context(c) << validator
                create_context_instance_methods(c)
              end
            end
          end
        end
      end
    end # module ClassMethods
  end # module Validate

  Resource.append_inclusions Validate
  Model.append_extensions Validate::ClassMethods
end # module DataMapper
