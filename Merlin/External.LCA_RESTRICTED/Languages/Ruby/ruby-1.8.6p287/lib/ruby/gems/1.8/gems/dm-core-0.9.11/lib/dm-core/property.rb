require 'date'
require 'time'
require 'bigdecimal'

module DataMapper

  # :include:QUICKLINKS
  #
  # = Properties
  # Properties for a model are not derived from a database structure, but
  # instead explicitly declared inside your model class definitions. These
  # properties then map (or, if using automigrate, generate) fields in your
  # repository/database.
  #
  # If you are coming to DataMapper from another ORM framework, such as
  # ActiveRecord, this is a fundamental difference in thinking. However, there
  # are several advantages to defining your properties in your models:
  #
  # * information about your model is centralized in one place: rather than
  #   having to dig out migrations, xml or other configuration files.
  # * having information centralized in your models, encourages you and the
  #   developers on your team to take a model-centric view of development.
  # * it provides the ability to use Ruby's access control functions.
  # * and, because DataMapper only cares about properties explicitly defined in
  #   your models, DataMapper plays well with legacy databases, and shares
  #   databases easily with other applications.
  #
  # == Declaring Properties
  # Inside your class, you call the property method for each property you want
  # to add. The only two required arguments are the name and type, everything
  # else is optional.
  #
  #   class Post
  #     include DataMapper::Resource
  #     property :title,   String,    :nullable => false
  #        # Cannot be null
  #     property :publish, TrueClass, :default => false
  #        # Default value for new records is false
  #   end
  #
  # By default, DataMapper supports the following primitive types:
  #
  # * TrueClass, Boolean
  # * String
  # * Text (limit of 65k characters by default)
  # * Float
  # * Integer
  # * BigDecimal
  # * DateTime
  # * Date
  # * Time
  # * Object (marshalled out during serialization)
  # * Class (datastore primitive is the same as String. Used for Inheritance)
  #
  # For more information about available Types, see DataMapper::Type
  #
  # == Limiting Access
  # Property access control is uses the same terminology Ruby does. Properties
  # are public by default, but can also be declared private or protected as
  # needed (via the :accessor option).
  #
  #  class Post
  #   include DataMapper::Resource
  #    property :title,  String, :accessor => :private
  #      # Both reader and writer are private
  #    property :body,   Text,   :accessor => :protected
  #      # Both reader and writer are protected
  #  end
  #
  # Access control is also analogous to Ruby accessors and mutators, and can
  # be declared using :reader and :writer, in addition to :accessor.
  #
  #  class Post
  #    include DataMapper::Resource
  #
  #    property :title, String, :writer => :private
  #      # Only writer is private
  #
  #    property :tags,  String, :reader => :protected
  #      # Only reader is protected
  #  end
  #
  # == Overriding Accessors
  # The accessor for any property can be overridden in the same manner that Ruby
  # class accessors can be.  After the property is defined, just add your custom
  # accessor:
  #
  #  class Post
  #    include DataMapper::Resource
  #    property :title,  String
  #
  #    def title=(new_title)
  #      raise ArgumentError if new_title != 'Luke is Awesome'
  #      @title = new_title
  #    end
  #  end
  #
  # == Lazy Loading
  # By default, some properties are not loaded when an object is fetched in
  # DataMapper. These lazily loaded properties are fetched on demand when their
  # accessor is called for the first time (as it is often unnecessary to
  # instantiate -every- property -every- time an object is loaded).  For
  # instance, DataMapper::Types::Text fields are lazy loading by default,
  # although you can over-ride this behavior if you wish:
  #
  # Example:
  #
  #  class Post
  #    include DataMapper::Resource
  #    property :title,  String                    # Loads normally
  #    property :body,   DataMapper::Types::Text   # Is lazily loaded by default
  #  end
  #
  # If you want to over-ride the lazy loading on any field you can set it to a
  # context or false to disable it with the :lazy option. Contexts allow
  # multipule lazy properties to be loaded at one time. If you set :lazy to
  # true, it is placed in the :default context
  #
  #  class Post
  #    include DataMapper::Resource
  #
  #    property :title,    String
  #      # Loads normally
  #
  #    property :body,     DataMapper::Types::Text, :lazy => false
  #      # The default is now over-ridden
  #
  #    property :comment,  String, lazy => [:detailed]
  #      # Loads in the :detailed context
  #
  #    property :author,   String, lazy => [:summary,:detailed]
  #      # Loads in :summary & :detailed context
  #  end
  #
  # Delaying the request for lazy-loaded attributes even applies to objects
  # accessed through associations. In a sense, DataMapper anticipates that
  # you will likely be iterating over objects in associations and rolls all
  # of the load commands for lazy-loaded properties into one request from
  # the database.
  #
  # Example:
  #
  #   Widget[1].components
  #     # loads when the post object is pulled from database, by default
  #
  #   Widget[1].components.first.body
  #     # loads the values for the body property on all objects in the
  #     # association, rather than just this one.
  #
  #   Widget[1].components.first.comment
  #     # loads both comment and author for all objects in the association
  #     # since they are both in the :detailed context
  #
  # == Keys
  # Properties can be declared as primary or natural keys on a table.
  # You should a property as the primary key of the table:
  #
  # Examples:
  #
  #  property :id,        Serial                    # auto-incrementing key
  #  property :legacy_pk, String, :key => true      # 'natural' key
  #
  # This is roughly equivalent to ActiveRecord's <tt>set_primary_key</tt>,
  # though non-integer data types may be used, thus DataMapper supports natural
  # keys. When a property is declared as a natural key, accessing the object
  # using the indexer syntax <tt>Class[key]</tt> remains valid.
  #
  #   User[1]
  #      # when :id is the primary key on the users table
  #   User['bill']
  #      # when :name is the primary (natural) key on the users table
  #
  # == Indeces
  # You can add indeces for your properties by using the <tt>:index</tt>
  # option. If you use <tt>true</tt> as the option value, the index will be
  # automatically named. If you want to name the index yourself, use a symbol
  # as the value.
  #
  #   property :last_name,  String, :index => true
  #   property :first_name, String, :index => :name
  #
  # You can create multi-column composite indeces by using the same symbol in
  # all the columns belonging to the index. The columns will appear in the
  # index in the order they are declared.
  #
  #   property :last_name,  String, :index => :name
  #   property :first_name, String, :index => :name
  #      # => index on (last_name, first_name)
  #
  # If you want to make the indeces unique, use <tt>:unique_index</tt> instead
  # of <tt>:index</tt>
  #
  # == Inferred Validations
  # If you require the dm-validations plugin, auto-validations will
  # automatically be mixed-in in to your model classes:
  # validation rules that are inferred when properties are declared with
  # specific column restrictions.
  #
  #  class Post
  #    include DataMapper::Resource
  #
  #    property :title, String, :length => 250
  #      # => infers 'validates_length :title,
  #             :minimum => 0, :maximum => 250'
  #
  #    property :title, String, :nullable => false
  #      # => infers 'validates_present :title
  #
  #    property :email, String, :format => :email_address
  #      # => infers 'validates_format :email, :with => :email_address
  #
  #    property :title, String, :length => 255, :nullable => false
  #      # => infers both 'validates_length' as well as
  #      #    'validates_present'
  #      #    better: property :title, String, :length => 1..255
  #
  #  end
  #
  # This functionality is available with the dm-validations gem, part of the
  # dm-more bundle. For more information about validations, check the
  # documentation for dm-validations.
  #
  # == Default Values
  # To set a default for a property, use the <tt>:default</tt> key.  The
  # property will be set to the value associated with that key the first time
  # it is accessed, or when the resource is saved if it hasn't been set with
  # another value already.  This value can be a static value, such as 'hello'
  # but it can also be a proc that will be evaluated when the property is read
  # before its value has been set.  The property is set to the return of the
  # proc.  The proc is passed two values, the resource the property is being set
  # for and the property itself.
  #
  #   property :display_name, String, :default => { |r, p| r.login }
  #
  # Word of warning.  Don't try to read the value of the property you're setting
  # the default for in the proc.  An infinite loop will ensue.
  #
  # == Embedded Values
  # As an alternative to extraneous has_one relationships, consider using an
  # EmbeddedValue.
  #
  # == Misc. Notes
  # * Properties declared as strings will default to a length of 50, rather than
  #   255 (typical max varchar column size).  To overload the default, pass
  #   <tt>:length => 255</tt> or <tt>:length => 0..255</tt>.  Since DataMapper
  #   does not introspect for properties, this means that legacy database tables
  #   may need their <tt>String</tt> columns defined with a <tt>:length</tt> so
  #   that DM does not apply an un-needed length validation, or allow overflow.
  # * You may declare a Property with the data-type of <tt>Class</tt>.
  #   see SingleTableInheritance for more on how to use <tt>Class</tt> columns.
  class Property
    include Assertions

    # NOTE: check is only for psql, so maybe the postgres adapter should
    # define its own property options. currently it will produce a warning tho
    # since PROPERTY_OPTIONS is a constant
    #
    # NOTE: PLEASE update PROPERTY_OPTIONS in DataMapper::Type when updating
    # them here
    PROPERTY_OPTIONS = [
      :accessor, :reader, :writer,
      :lazy, :default, :nullable, :key, :serial, :field, :size, :length,
      :format, :index, :unique_index, :check, :ordinal, :auto_validation,
      :validates, :unique, :track, :precision, :scale
    ]

    # FIXME: can we pull the keys from
    # DataMapper::Adapters::DataObjectsAdapter::TYPES
    # for this?
    TYPES = [
      TrueClass,
      String,
      DataMapper::Types::Text,
      Float,
      Integer,
      BigDecimal,
      DateTime,
      Date,
      Time,
      Object,
      Class,
      DataMapper::Types::Discriminator,
      DataMapper::Types::Serial
    ]

    IMMUTABLE_TYPES = [ TrueClass, Float, Integer, BigDecimal]

    VISIBILITY_OPTIONS = [ :public, :protected, :private ]

    DEFAULT_LENGTH    = 50
    DEFAULT_PRECISION = 10
    DEFAULT_SCALE_BIGDECIMAL = 0
    DEFAULT_SCALE_FLOAT = nil

    attr_reader :primitive, :model, :name, :instance_variable_name,
      :type, :reader_visibility, :writer_visibility, :getter, :options,
      :default, :precision, :scale, :track, :extra_options

    # Supplies the field in the data-store which the property corresponds to
    #
    # @return <String> name of field in data-store
    # -
    # @api semi-public
    def field(repository_name = nil)
      @field || @fields[repository_name] ||= self.model.field_naming_convention(repository_name).call(self)
    end

    def unique
      @unique ||= @options.fetch(:unique, @serial || @key || false)
    end

    def hash
      if @custom && !@bound
        @type.bind(self)
        @bound = true
      end

      return @model.hash + @name.hash
    end

    def eql?(o)
      if o.is_a?(Property)
        return o.model == @model && o.name == @name
      else
        return false
      end
    end

    def length
      @length.is_a?(Range) ? @length.max : @length
    end
    alias size length

    def index
      @index
    end

    def unique_index
      @unique_index
    end

    # Returns whether or not the property is to be lazy-loaded
    #
    # @return <TrueClass, FalseClass> whether or not the property is to be
    #   lazy-loaded
    # -
    # @api public
    def lazy?
      @lazy
    end

    # Returns whether or not the property is a key or a part of a key
    #
    # @return <TrueClass, FalseClass> whether the property is a key or a part of
    #   a key
    #-
    # @api public
    def key?
      @key
    end

    # Returns whether or not the property is "serial" (auto-incrementing)
    #
    # @return <TrueClass, FalseClass> whether or not the property is "serial"
    #-
    # @api public
    def serial?
      @serial
    end

    # Returns whether or not the property can accept 'nil' as it's value
    #
    # @return <TrueClass, FalseClass> whether or not the property can accept 'nil'
    #-
    # @api public
    def nullable?
      @nullable
    end

    def custom?
      @custom
    end

    # Provides a standardized getter method for the property
    #
    # @raise <ArgumentError> "+resource+ should be a DataMapper::Resource, but was ...."
    #-
    # @api private
    def get(resource)
      lazy_load(resource)

      value = get!(resource)

      set_original_value(resource, value)

      # [YK] Why did we previously care whether options[:default] is nil.
      # The default value of nil will be applied either way
      if value.nil? && resource.new_record? && !resource.attribute_loaded?(name)
        value = default_for(resource)
        set(resource, value)
      end

      value
    end

    def get!(resource)
      resource.instance_variable_get(instance_variable_name)
    end

    def set_original_value(resource, val)
      unless resource.original_values.key?(name)
        val = val.try_dup
        val = val.hash if track == :hash
        resource.original_values[name] = val
      end
    end

    # Provides a standardized setter method for the property
    #
    # @raise <ArgumentError> "+resource+ should be a DataMapper::Resource, but was ...."
    #-
    # @api private
    def set(resource, value)
      # [YK] We previously checked for new_record? here, but lazy loading
      # is blocked anyway if we're in a new record by by
      # Resource#reload_attributes. This may eventually be useful for
      # optimizing, but let's (a) benchmark it first, and (b) do
      # whatever refactoring is necessary, which will benefit from the
      # centralize checking
      lazy_load(resource)

      new_value = typecast(value)
      old_value = get!(resource)

      set_original_value(resource, old_value)

      set!(resource, new_value)
    end

    def set!(resource, value)
      resource.instance_variable_set(instance_variable_name, value)
    end

    # Loads lazy columns when get or set is called.
    #-
    # @api private
    def lazy_load(resource)
      # It is faster to bail out at at a new_record? rather than to process
      # which properties would be loaded and then not load them.
      return if resource.new_record? || resource.attribute_loaded?(name)
      # If we're trying to load a lazy property, load it. Otherwise, lazy-load
      # any properties that should be eager-loaded but were not included
      # in the original :fields list
      contexts = lazy? ? name : model.eager_properties(resource.repository.name)
      resource.send(:lazy_load, contexts)
    end

    # typecasts values into a primitive
    #
    # @return <TrueClass, String, Float, Integer, BigDecimal, DateTime, Date, Time
    #   Class> the primitive data-type, defaults to TrueClass
    #-
    # @api private
    def typecast(value)
      return type.typecast(value, self) if type.respond_to?(:typecast)
      return value if value.kind_of?(primitive) || value.nil?
      begin
        if    primitive == TrueClass  then %w[ true 1 t ].include?(value.to_s.downcase)
        elsif primitive == String     then value.to_s
        elsif primitive == Float      then value.to_f
        elsif primitive == Integer
          # The simplest possible implementation, i.e. value.to_i, is not
          # desirable because "junk".to_i gives "0". We want nil instead,
          # because this makes it clear that the typecast failed.
          #
          # After benchmarking, we preferred the current implementation over
          # these two alternatives:
          # * Integer(value) rescue nil
          # * Integer(value_to_s =~ /(\d+)/ ? $1 : value_to_s) rescue nil
          #
          # [YK] The previous implementation used a rescue. Why use a rescue
          # when the list of cases where a valid string other than "0" could
          # produce 0 is known?
          value_to_i = value.to_i
          if value_to_i == 0
            value.to_s =~ /^(0x|0b)?0+/ ? 0 : nil
          else
            value_to_i
          end
        elsif primitive == BigDecimal then BigDecimal(value.to_s)
        elsif primitive == DateTime   then typecast_to_datetime(value)
        elsif primitive == Date       then typecast_to_date(value)
        elsif primitive == Time       then typecast_to_time(value)
        elsif primitive == Class      then self.class.find_const(value)
        else
          value
        end
      rescue
        value
      end
    end

    def default_for(resource)
      @default.respond_to?(:call) ? @default.call(resource, self) : @default
    end

    def value(val)
      custom? ? self.type.dump(val, self) : val
    end

    def inspect
      "#<Property:#{@model}:#{@name}>"
    end

    # TODO: add docs
    # @api private
    def _dump(*)
      Marshal.dump([ repository, model, name ])
    end

    # TODO: add docs
    # @api private
    def self._load(marshalled)
      repository, model, name = Marshal.load(marshalled)
      model.properties(repository.name)[name]
    end

    private

    def initialize(model, name, type, options = {})
      assert_kind_of 'model', model, Model
      assert_kind_of 'name',  name,  Symbol
      assert_kind_of 'type',  type,  Class

      if Fixnum == type
        # It was decided that Integer is a more expressively names class to
        # use instead of Fixnum.  Fixnum only represents smaller numbers,
        # so there was some confusion over whether or not it would also
        # work with Bignum too (it will).  Any Integer, which includes
        # Fixnum and Bignum, can be stored in this property.
        warn "#{type} properties are deprecated.  Please use Integer instead"
        type = Integer
      end

      unless TYPES.include?(type) || (DataMapper::Type > type && TYPES.include?(type.primitive))
        raise ArgumentError, "+type+ was #{type.inspect}, which is not a supported type: #{TYPES * ', '}", caller
      end

      @extra_options = {}
      (options.keys - PROPERTY_OPTIONS).each do |key|
        @extra_options[key] = options.delete(key)
      end

      @model                  = model
      @name                   = name.to_s.sub(/\?$/, '').to_sym
      @type                   = type
      @custom                 = DataMapper::Type > @type
      @options                = @custom ? @type.options.merge(options) : options
      @instance_variable_name = "@#{@name}"

      # TODO: This default should move to a DataMapper::Types::Text
      # Custom-Type and out of Property.
      @primitive = @options.fetch(:primitive, @type.respond_to?(:primitive) ? @type.primitive : @type)

      @getter       = TrueClass == @primitive ? "#{@name}?".to_sym : @name
      @field        = @options.fetch(:field,        nil)
      @serial       = @options.fetch(:serial,       false)
      @key          = @options.fetch(:key,          @serial || false)
      @default      = @options.fetch(:default,      nil)
      @nullable     = @options.fetch(:nullable,     @key == false)
      @index        = @options.fetch(:index,        false)
      @unique_index = @options.fetch(:unique_index, false)
      @lazy         = @options.fetch(:lazy,         @type.respond_to?(:lazy) ? @type.lazy : false) && !@key
      @fields       = {}

      @track = @options.fetch(:track) do
        if @custom && @type.respond_to?(:track) && @type.track
          @type.track
        else
          IMMUTABLE_TYPES.include?(@primitive) ? :set : :get
        end
      end

      # assign attributes per-type
      if String == @primitive || Class == @primitive
        @length = @options.fetch(:length, @options.fetch(:size, DEFAULT_LENGTH))
      elsif BigDecimal == @primitive || Float == @primitive
        @precision = @options.fetch(:precision, DEFAULT_PRECISION)

        default_scale = (Float == @primitive) ? DEFAULT_SCALE_FLOAT : DEFAULT_SCALE_BIGDECIMAL
        @scale     = @options.fetch(:scale, default_scale)
        # @scale     = @options.fetch(:scale, DEFAULT_SCALE_BIGDECIMAL)

        unless @precision > 0
          raise ArgumentError, "precision must be greater than 0, but was #{@precision.inspect}"
        end

        if (BigDecimal == @primitive) || (Float == @primitive && !@scale.nil?)
          unless @scale >= 0
            raise ArgumentError, "scale must be equal to or greater than 0, but was #{@scale.inspect}"
          end

          unless @precision >= @scale
            raise ArgumentError, "precision must be equal to or greater than scale, but was #{@precision.inspect} and scale was #{@scale.inspect}"
          end
        end
      end

      determine_visibility

      @model.auto_generate_validations(self)    if @model.respond_to?(:auto_generate_validations)
      @model.property_serialization_setup(self) if @model.respond_to?(:property_serialization_setup)
    end

    def determine_visibility # :nodoc:
      @reader_visibility = @options[:reader] || @options[:accessor] || :public
      @writer_visibility = @options[:writer] || @options[:accessor] || :public

      unless VISIBILITY_OPTIONS.include?(@reader_visibility) && VISIBILITY_OPTIONS.include?(@writer_visibility)
        raise ArgumentError, 'property visibility must be :public, :protected, or :private', caller(2)
      end
    end

    # Typecasts an arbitrary value to a DateTime
    def typecast_to_datetime(value)
      case value
      when Hash then typecast_hash_to_datetime(value)
      else DateTime.parse(value.to_s)
      end
    end

    # Typecasts an arbitrary value to a Date
    def typecast_to_date(value)
      case value
      when Hash then typecast_hash_to_date(value)
      else Date.parse(value.to_s)
      end
    end

    # Typecasts an arbitrary value to a Time
    def typecast_to_time(value)
      case value
      when Hash then typecast_hash_to_time(value)
      else Time.parse(value.to_s)
      end
    end

    def typecast_hash_to_datetime(hash)
      args = extract_time_args_from_hash(hash, :year, :month, :day, :hour, :min, :sec)
      DateTime.new(*args)
    rescue ArgumentError => e
      t = typecast_hash_to_time(hash)
      DateTime.new(t.year, t.month, t.day, t.hour, t.min, t.sec)
    end

    def typecast_hash_to_date(hash)
      args = extract_time_args_from_hash(hash, :year, :month, :day)
      Date.new(*args)
    rescue ArgumentError
      t = typecast_hash_to_time(hash)
      Date.new(t.year, t.month, t.day)
    end

    def typecast_hash_to_time(hash)
      args = extract_time_args_from_hash(hash, :year, :month, :day, :hour, :min, :sec)
      Time.local(*args)
    end

    # Extracts the given args from the hash. If a value does not exist, it
    # uses the value of Time.now
    def extract_time_args_from_hash(hash, *args)
      now = Time.now
      args.map { |arg| hash[arg] || hash[arg.to_s] || now.send(arg) }
    end
  end # class Property
end # module DataMapper
