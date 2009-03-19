module DataMapper

  # = Types
  # Provides means of writing custom types for properties. Each type is based
  # on a ruby primitive and handles its own serialization and materialization,
  # and therefore is responsible for providing those methods.
  #
  # To see complete list of supported types, see documentation for
  # DataMapper::Property::TYPES
  #
  # == Defining new Types
  # To define a new type, subclass DataMapper::Type, pick ruby primitive, and
  # set the options for this type.
  #
  #   class MyType < DataMapper::Type
  #     primitive String
  #     size 10
  #   end
  #
  # Following this, you will be able to use MyType as a type for any given
  # property. If special materialization and serialization is required,
  # override the class methods
  #
  #   class MyType < DataMapper::Type
  #     primitive String
  #     size 10
  #
  #     def self.dump(value, property)
  #       <work some magic>
  #     end
  #
  #     def self.load(value)
  #       <work some magic>
  #     end
  #   end
  class Type
    PROPERTY_OPTIONS = [
      :accessor, :reader, :writer,
      :lazy, :default, :nullable, :key, :serial, :field, :size, :length,
      :format, :index, :unique_index, :check, :ordinal, :auto_validation,
      :validates, :unique, :track, :precision, :scale
    ]

    PROPERTY_OPTION_ALIASES = {
      :size => [ :length ]
    }

    class << self

      def configure(primitive_type, options)
        @_primitive_type = primitive_type
        @_options = options

        def self.inherited(base)
          base.primitive @_primitive_type

          @_options.each do |k, v|
            base.send(k, v)
          end
        end

        self
      end

      # The Ruby primitive type to use as basis for this type. See
      # DataMapper::Property::TYPES for list of types.
      #
      # @param primitive<Class, nil>
      #   The class for the primitive. If nil is passed in, it returns the
      #   current primitive
      #
      # @return <Class> if the <primitive> param is nil, return the current primitive.
      #
      # @api public
      def primitive(primitive = nil)
        return @primitive if primitive.nil?

        # TODO: change Integer to be used internally once most in-the-wild code
        # is updated to use Integer for properties instead of Fixnum, or before
        # DM 1.0, whichever comes first
        if Fixnum == primitive
          warn "#{primitive} properties are deprecated.  Please use Integer instead"
          primitive = Integer
        end

        @primitive = primitive
      end

      # Load DataMapper::Property options
      PROPERTY_OPTIONS.each do |property_option|
        self.class_eval <<-EOS, __FILE__, __LINE__
          def #{property_option}(arg = nil)
            return @#{property_option} if arg.nil?

            @#{property_option} = arg
          end
        EOS
      end

      # Create property aliases
      PROPERTY_OPTION_ALIASES.each do |property_option, aliases|
        aliases.each do |ali|
          self.class_eval <<-EOS, __FILE__, __LINE__
            alias #{ali} #{property_option}
          EOS
        end
      end

      # Gives all the options set on this type
      #
      # @return <Hash> with all options and their values set on this type
      #
      # @api public
      def options
        options = {}
        PROPERTY_OPTIONS.each do |method|
          next if (value = send(method)).nil?
          options[method] = value
        end
        options
      end
    end

    # Stub instance method for dumping
    #
    # @param value<Object, nil>       the value to dump
    # @param property<Property, nil>  the property the type is being used by
    #
    # @return <Object> Dumped object
    #
    # @api public
    def self.dump(value, property)
      value
    end

    # Stub instance method for loading
    #
    # @param value<Object, nil>       the value to serialize
    # @param property<Property, nil>  the property the type is being used by
    #
    # @return <Object> Serialized object. Must be the same type as the Ruby primitive
    #
    # @api public
    def self.load(value, property)
      value
    end

    def self.bind(property)
      # This method should not modify the state of this type class, and
      # should produce no side-effects on the type class. It's just a
      # hook to allow your custom-type to modify the property it's bound to.
    end

  end # class Type

  def self.Type(primitive_type, options = {})
    Class.new(Type).configure(primitive_type, options)
  end

end # module DataMapper
