require 'set'

module DataMapper
  module Model
    ##
    #
    # Extends the model with this module after DataMapper::Resource has been
    # included.
    #
    # This is a useful way to extend DataMapper::Model while
    # still retaining a self.extended method.
    #
    # @param [Module] extensions the module that is to be extend the model after
    #   after DataMapper::Model
    #
    # @return [TrueClass, FalseClass] whether or not the inclusions have been
    #   successfully appended to the list
    #-
    # @api public
    #
    # TODO: Move this do DataMapper::Model when DataMapper::Model is created
    def self.append_extensions(*extensions)
      extra_extensions.concat extensions
      true
    end

    def self.extra_extensions
      @extra_extensions ||= []
    end

    def self.extended(model)
      model.instance_variable_set(:@storage_names, {})
      model.instance_variable_set(:@properties,    {})
      model.instance_variable_set(:@field_naming_conventions, {})
      extra_extensions.each { |extension| model.extend(extension) }
    end

    def inherited(target)
      target.instance_variable_set(:@storage_names,       @storage_names.dup)
      target.instance_variable_set(:@properties,          {})
      target.instance_variable_set(:@base_model,          self.base_model)
      target.instance_variable_set(:@paranoid_properties, @paranoid_properties)
      target.instance_variable_set(:@field_naming_conventions,  @field_naming_conventions.dup)

      if self.respond_to?(:validators)
        @validations.contexts.each do |context, validators|
          validators.each { |validator| target.validators.context(context) << validator }
        end
      end

      @properties.each do |repository_name,properties|
        repository(repository_name) do
          properties.each do |property|
            next if target.properties(repository_name).has_property?(property.name)
            target.property(property.name, property.type, property.options.dup)
          end
        end
      end

      if @relationships
        duped_relationships = {}
        @relationships.each do |repository_name,relationships|
          relationships.each do |name, relationship|
            dup = relationship.dup
            dup.instance_variable_set(:@child_model, target) if dup.instance_variable_get(:@child_model) == self
            dup.instance_variable_set(:@parent_model, target) if dup.instance_variable_get(:@parent_model) == self
            duped_relationships[repository_name] ||= {}
            duped_relationships[repository_name][name] = dup
          end
        end
        target.instance_variable_set(:@relationships, duped_relationships)
      end
    end

    def self.new(storage_name, &block)
      model = Class.new
      model.send(:include, Resource)
      model.class_eval <<-EOS, __FILE__, __LINE__
        def self.default_storage_name
          #{Extlib::Inflection.classify(storage_name).inspect}
        end
      EOS
      model.instance_eval(&block) if block_given?
      model
    end

    def base_model
      @base_model ||= self
    end

    def repository_name
      Repository.context.any? ? Repository.context.last.name : default_repository_name
    end

    ##
    # Get the repository with a given name, or the default one for the current
    # context, or the default one for this class.
    #
    # @param name<Symbol>   the name of the repository wanted
    # @param block<Block>   block to execute with the fetched repository as parameter
    #
    # @return <Object, DataMapper::Respository> whatever the block returns,
    #   if given a block, otherwise the requested repository.
    #-
    # @api public
    def repository(name = nil)
      #
      # There has been a couple of different strategies here, but me (zond) and dkubb are at least
      # united in the concept of explicitness over implicitness. That is - the explicit wish of the
      # caller (+name+) should be given more priority than the implicit wish of the caller (Repository.context.last).
      #
      if block_given?
        DataMapper.repository(name || repository_name) { |*block_args| yield(*block_args) }
      else
        DataMapper.repository(name || repository_name)
      end
    end

    ##
    # the name of the storage recepticle for this resource.  IE. table name, for database stores
    #
    # @return <String> the storage name (IE table name, for database stores) associated with this resource in the given repository
    def storage_name(repository_name = default_repository_name)
      @storage_names[repository_name] ||= repository(repository_name).adapter.resource_naming_convention.call(base_model.send(:default_storage_name))
    end

    ##
    # the names of the storage recepticles for this resource across all repositories
    #
    # @return <Hash(Symbol => String)> All available names of storage recepticles
    def storage_names
      @storage_names
    end

    ##
    # The field naming conventions for this resource across all repositories.
    #
    # @return <String> The naming convention for the given repository
    def field_naming_convention(repository_name = default_storage_name)
      @field_naming_conventions[repository_name] ||= repository(repository_name).adapter.field_naming_convention
    end

    ##
    # defines a property on the resource
    #
    # @param <Symbol> name the name for which to call this property
    # @param <Type> type the type to define this property ass
    # @param <Hash(Symbol => String)> options a hash of available options
    # @see DataMapper::Property
    def property(name, type, options = {})
      property = Property.new(self, name, type, options)

      create_property_getter(property)
      create_property_setter(property)

      properties(repository_name)[property.name] = property
      @_valid_relations = false

      # Add property to the other mappings as well if this is for the default
      # repository.
      if repository_name == default_repository_name
        @properties.each_pair do |repository_name, properties|
          next if repository_name == default_repository_name
          properties << property unless properties.has_property?(property.name)
        end
      end

      # Add the property to the lazy_loads set for this resources repository
      # only.
      # TODO Is this right or should we add the lazy contexts to all
      # repositories?
      if property.lazy?
        context = options.fetch(:lazy, :default)
        context = :default if context == true

        Array(context).each do |item|
          properties(repository_name).lazy_context(item) << name
        end
      end

      # add the property to the child classes only if the property was
      # added after the child classes' properties have been copied from
      # the parent
      if respond_to?(:descendants)
        descendants.each do |model|
          next if model.properties(repository_name).has_property?(name)
          model.property(name, type, options)
        end
      end

      property
    end

    def repositories
      [ repository ].to_set + @properties.keys.collect { |repository_name| DataMapper.repository(repository_name) }
    end

    def properties(repository_name = default_repository_name)
      # We need to check whether all relations are already set up.
      # If this isn't the case, we try to reload them here
      if !@_valid_relations && respond_to?(:many_to_one_relationships)
        @_valid_relations = true
        begin
          many_to_one_relationships.each do |r|
            r.child_key
          end
        rescue NameError
          # Apparently not all relations are loaded,
          # so we will try again later on
          @_valid_relations = false
        end
      end
      @properties[repository_name] ||= repository_name == Repository.default_name ? PropertySet.new : properties(Repository.default_name).dup
    end

    def eager_properties(repository_name = default_repository_name)
      properties(repository_name).defaults
    end

    # @api private
    def properties_with_subclasses(repository_name = default_repository_name)
      properties = PropertySet.new
      ([ self ].to_set + (respond_to?(:descendants) ? descendants : [])).each do |model|
        model.relationships(repository_name).each_value { |relationship| relationship.child_key }
        model.many_to_one_relationships.each do |relationship| relationship.child_key end
        model.properties(repository_name).each do |property|
          properties << property unless properties.has_property?(property.name)
        end
      end
      properties
    end

    def key(repository_name = default_repository_name)
      properties(repository_name).key
    end

    def inheritance_property(repository_name = default_repository_name)
      @properties[repository_name].detect { |property| property.type == DataMapper::Types::Discriminator }
    end

    def default_order(repository_name = default_repository_name)
      @default_order ||= {}
      @default_order[repository_name] ||= key(repository_name).map { |property| Query::Direction.new(property) }
    end

    def get(*key)
      key = typecast_key(key)
      repository.identity_map(self).get(key) || first(to_query(repository, key))
    end

    def get!(*key)
      get(*key) || raise(ObjectNotFoundError, "Could not find #{self.name} with key #{key.inspect}")
    end

    def all(query = {})
      query = scoped_query(query)
      query.repository.read_many(query)
    end

    def first(*args)
      query = args.last.respond_to?(:merge) ? args.pop : {}
      query = scoped_query(query.merge(:limit => args.first || 1))

      if args.any?
        query.repository.read_many(query)
      else
        query.repository.read_one(query)
      end
    end

    def [](*key)
      warn("#{name}[] is deprecated. Use #{name}.get! instead.")
      get!(*key)
    end

    def first_or_create(query, attributes = {})
      first(query) || begin
        resource = allocate
        query = query.dup

        properties(repository_name).key.each do |property|
          if value = query.delete(property.name)
            resource.send("#{property.name}=", value)
          end
        end

        resource.attributes = query.merge(attributes)
        resource.save
        resource
      end
    end

    ##
    # Create an instance of Resource with the given attributes
    #
    # @param <Hash(Symbol => Object)> attributes hash of attributes to set
    def create(attributes = {})
      resource = new(attributes)
      resource.save
      resource
    end

    ##
    # This method is deprecated, and will be removed from dm-core.
    #
    def create!(attributes = {})
      warn("Model#create! is deprecated. It is moving to dm-validations, and will be used to create a record without validations")
      resource = create(attributes)
      raise PersistenceError, "Resource not saved: :new_record => #{resource.new_record?}, :dirty_attributes => #{resource.dirty_attributes.inspect}" if resource.new_record?
      resource
    end

    ##
    # Copy a set of records from one repository to another.
    #
    # @param [String] source
    #   The name of the Repository the resources should be copied _from_
    # @param [String] destination
    #   The name of the Repository the resources should be copied _to_
    # @param [Hash] query
    #   The conditions with which to find the records to copy. These
    #   conditions are merged with Model.query
    #
    # @return [DataMapper::Collection]
    #   A Collection of the Resource instances created in the operation
    #
    # @api public
    def copy(source, destination, query = {})

      # get the list of properties that exist in the source and destination
      destination_properties = properties(destination)
      fields = query[:fields] ||= properties(source).select { |p| destination_properties.has_property?(p.name) }

      repository(destination) do
        all(query.merge(:repository => repository(source))).map do |resource|
          create(fields.map { |p| [ p.name, p.get(resource) ] }.to_hash)
        end
      end
    end

    # @api private
    # TODO: spec this
    def load(values, query)
      repository = query.repository
      model      = self

      if inheritance_property_index = query.inheritance_property_index
        model = values.at(inheritance_property_index) || model
      end

      key_values = nil
      identity_map = nil

      if key_property_indexes = query.key_property_indexes(repository)
        key_values   = values.values_at(*key_property_indexes)
        identity_map = repository.identity_map(model)

        if resource = identity_map.get(key_values)
          return resource unless query.reload?
        else
          resource = model.allocate
          resource.instance_variable_set(:@repository, repository)
        end
      else
        resource = model.allocate
        resource.readonly!
      end

      resource.instance_variable_set(:@new_record, false)

      query.fields.zip(values) do |property,value|
        value = property.custom? ? property.type.load(value, property) : property.typecast(value)
        property.set!(resource, value)

        if track = property.track
          case track
            when :hash
              resource.original_values[property.name] = value.dup.hash unless resource.original_values.has_key?(property.name) rescue value.hash
            when :load
              resource.original_values[property.name] = value unless resource.original_values.has_key?(property.name)
          end
        end
      end

      if key_values && identity_map
        identity_map.set(key_values, resource)
      end

      resource
    end

    # TODO: spec this
    def to_query(repository, key, query = {})
      conditions = Hash[ *self.key(repository.name).zip(key).flatten ]
      Query.new(repository, self, query.merge(conditions))
    end

    # TODO: add docs
    # @api private
    def _load(marshalled)
      resource = allocate
      Marshal.load(marshalled).each { |kv| resource.instance_variable_set(*kv) }
      resource
    end

    def typecast_key(key)
      self.key(repository_name).zip(key).map { |k, v| k.typecast(v) }
    end

    def default_repository_name
      Repository.default_name
    end

    def paranoid_properties
      @paranoid_properties ||= {}
      @paranoid_properties
    end

    private

    def default_storage_name
      self.name
    end

    def scoped_query(query = self.query)
      assert_kind_of 'query', query, Query, Hash

      return self.query if query == self.query

      query = if query.kind_of?(Hash)
        Query.new(query.has_key?(:repository) ? query.delete(:repository) : self.repository, self, query)
      else
        query
      end

      if self.query
        self.query.merge(query)
      else
        merge_with_default_scope(query)
      end
    end

    def set_paranoid_property(name, &block)
      self.paranoid_properties[name] = block
    end

    # defines the getter for the property
    def create_property_getter(property)
      class_eval <<-EOS, __FILE__, __LINE__
        #{property.reader_visibility}
        def #{property.getter}
          attribute_get(#{property.name.inspect})
        end
      EOS

      if property.primitive == TrueClass && !instance_methods.map { |m| m.to_s }.include?(property.name.to_s)
        class_eval <<-EOS, __FILE__, __LINE__
          #{property.reader_visibility}
          alias #{property.name} #{property.getter}
        EOS
      end
    end

    # defines the setter for the property
    def create_property_setter(property)
      unless instance_methods.map { |m| m.to_s }.include?("#{property.name}=")
        class_eval <<-EOS, __FILE__, __LINE__
          #{property.writer_visibility}
          def #{property.name}=(value)
            attribute_set(#{property.name.inspect}, value)
          end
        EOS
      end
    end

    def relationships(*args)
      # DO NOT REMOVE!
      # method_missing depends on these existing. Without this stub,
      # a missing module can cause misleading recursive errors.
      raise NotImplementedError.new
    end

    def method_missing(method, *args, &block)
      if relationship = self.relationships(repository_name)[method]
        klass = self == relationship.child_model ? relationship.parent_model : relationship.child_model
        return DataMapper::Query::Path.new(repository, [ relationship ], klass)
      end

      property_set = properties(repository_name)
      if property_set.has_property?(method)
        return property_set[method]
      end

      super
    end

    # TODO: move to dm-more/dm-transactions
    module Transaction
      #
      # Produce a new Transaction for this Resource class
      #
      # @return <DataMapper::Adapters::Transaction
      #   a new DataMapper::Adapters::Transaction with all DataMapper::Repositories
      #   of the class of this DataMapper::Resource added.
      #-
      # @api public
      #
      # TODO: move to dm-more/dm-transactions
      def transaction
        DataMapper::Transaction.new(self) { |block_args| yield(*block_args) }
      end
    end # module Transaction

    include Transaction

    # TODO: move to dm-more/dm-migrations
    module Migration
      # TODO: move to dm-more/dm-migrations
      def storage_exists?(repository_name = default_repository_name)
        repository(repository_name).storage_exists?(storage_name(repository_name))
      end
    end # module Migration

    include Migration
  end # module Model
end # module DataMapper
