module DataMapper
  class Collection < LazyArray
    include Assertions

    attr_reader :query

    ##
    # @return [Repository] the repository the collection is
    #   associated with
    #
    # @api public
    def repository
      query.repository
    end

    ##
    # loads the entries for the collection. Used by the
    # adapters to load the instances of the declared
    # model for this collection's query.
    #
    # @api private
    def load(values)
      add(model.load(values, query))
    end

    ##
    # reloads the entries associated with this collection
    #
    # @param [DataMapper::Query] query (optional) additional query
    #   to scope by.  Use this if you want to query a collections result
    #   set
    #
    # @see DataMapper::Collection#all
    #
    # @api public
    def reload(query = {})
      @query = scoped_query(query)
      inheritance_property  = model.base_model.inheritance_property
      fields  = (@key_properties | [inheritance_property]).compact
      @query.update(:fields => @query.fields | fields)
      replace(all(:reload => true))
    end

    ##
    # retrieves an entry out of the collection's entry by key
    #
    # @param [DataMapper::Types::*, ...] key keys which uniquely
    #   identify a resource in the collection
    #
    # @return [DataMapper::Resource, NilClass] the resource which
    #   has the supplied keys
    #
    # @api public
    def get(*key)
      key = model.typecast_key(key)
      if loaded?
        # find indexed resource (create index first if it does not exist)
        each {|r| @cache[r.key] = r } if @cache.empty?
        @cache[key]
      elsif query.limit || query.offset > 0
        # current query is exclusive, find resource within the set

        # TODO: use a subquery to retrieve the collection and then match
        #   it up against the key.  This will require some changes to
        #   how subqueries are generated, since the key may be a
        #   composite key.  In the case of DO adapters, it means subselects
        #   like the form "(a, b) IN(SELECT a,b FROM ...)", which will
        #   require making it so the Query condition key can be a
        #   Property or an Array of Property objects

        # use the brute force approach until subquery lookups work
        lazy_load
        get(*key)
      else
        # current query is all inclusive, lookup using normal approach
        first(model.to_query(repository, key))
      end
    end

    ##
    # retrieves an entry out of the collection's entry by key,
    # raising an exception if the object cannot be found
    #
    # @param [DataMapper::Types::*, ...] key keys which uniquely
    #   identify a resource in the collection
    #
    # @calls DataMapper::Collection#get
    #
    # @raise [ObjectNotFoundError] "Could not find #{model.name} with key #{key.inspect} in collection"
    #
    # @api public
    def get!(*key)
      get(*key) || raise(ObjectNotFoundError, "Could not find #{model.name} with key #{key.inspect} in collection")
    end

    ##
    # Further refines a collection's conditions.  #all provides an
    # interface which simulates a database view.
    #
    # @param [Hash[Symbol, Object], DataMapper::Query] query parameters for
    #   an query within the results of the original query.
    #
    # @return [DataMapper::Collection] a collection whose query is the result
    #   of a merge
    #
    # @api public
    def all(query = {})
      # TODO: this shouldn't be a kicker if scoped_query() is called
      return self if query.kind_of?(Hash) ? query.empty? : query == self.query
      query = scoped_query(query)
      query.repository.read_many(query)
    end

    ##
    # Simulates Array#first by returning the first entry (when
    # there are no arguments), or transforms the collection's query
    # by applying :limit => n when you supply an Integer. If you
    # provide a conditions hash, or a Query object, the internal
    # query is scoped and a new collection is returned
    #
    # @param [Integer, Hash[Symbol, Object], Query] args
    #
    # @return [DataMapper::Resource, DataMapper::Collection] The
    #   first resource in the entries of this collection, or
    #   a new collection whose query has been merged
    #
    # @api public
    def first(*args)
      # TODO: this shouldn't be a kicker if scoped_query() is called
      if loaded?
        if args.empty?
          return super
        elsif args.size == 1 && args.first.kind_of?(Integer)
          limit = args.shift
          return self.class.new(scoped_query(:limit => limit)) { |c| c.replace(super(limit)) }
        end
      end

      query = args.last.respond_to?(:merge) ? args.pop : {}
      query = scoped_query(query.merge(:limit => args.first || 1))

      if args.any?
        query.repository.read_many(query)
      else
        query.repository.read_one(query)
      end
    end

    ##
    # Simulates Array#last by returning the last entry (when
    # there are no arguments), or transforming the collection's
    # query by reversing the declared order, and applying
    # :limit => n when you supply an Integer.  If you
    # supply a conditions hash, or a Query object, the
    # internal query is scoped and a new collection is returned
    #
    # @calls Collection#first
    #
    # @api public
    def last(*args)
      return super if loaded? && args.empty?

      reversed = reverse

      # tell the collection to reverse the order of the
      # results coming out of the adapter
      reversed.query.add_reversed = !query.add_reversed?

      reversed.first(*args)
    end

    ##
    # Simulates Array#at and returns the entry at that index.
    # Also accepts negative indexes and appropriate reverses
    # the order of the query
    #
    # @calls Collection#first
    # @calls Collection#last
    #
    # @api public
    def at(offset)
      return super if loaded?
      offset >= 0 ? first(:offset => offset) : last(:offset => offset.abs - 1)
    end

    ##
    # Simulates Array#slice and returns a new Collection
    # whose query has a new offset or limit according to the
    # arguments provided.
    #
    # If you provide a range, the min is used as the offset
    # and the max minues the offset is used as the limit.
    #
    # @param [Integer, Array(Integer), Range] args the offset,
    # offset and limit, or range indicating offsets and limits
    #
    # @return [DataMapper::Resource, DataMapper::Collection]
    #   The entry which resides at that offset and limit,
    #   or a new Collection object with the set limits and offset
    #
    # @raise [ArgumentError] "arguments may be 1 or 2 Integers,
    #   or 1 Range object, was: #{args.inspect}"
    #
    # @alias []
    #
    # @api public
    def slice(*args)
      return at(args.first) if args.size == 1 && args.first.kind_of?(Integer)

      if args.size == 2 && args.first.kind_of?(Integer) && args.last.kind_of?(Integer)
        offset, limit = args
      elsif args.size == 1 && args.first.kind_of?(Range)
        range  = args.first
        offset = range.first
        limit  = range.last - offset
        limit += 1 unless range.exclude_end?
      else
        raise ArgumentError, "arguments may be 1 or 2 Integers, or 1 Range object, was: #{args.inspect}", caller
      end

      all(:offset => offset, :limit => limit)
    end

    alias [] slice

    ##
    #
    # @return [DataMapper::Collection] a new collection whose
    #   query is sorted in the reverse
    #
    # @see Array#reverse, DataMapper#all, DataMapper::Query#reverse
    #
    # @api public
    def reverse
      all(self.query.reverse)
    end

    ##
    # @see Array#<<
    #
    # @api public
    def <<(resource)
      super
      relate_resource(resource)
      self
    end

    ##
    # @see Array#push
    #
    # @api public
    def push(*resources)
      super
      resources.each { |resource| relate_resource(resource) }
      self
    end

    ##
    # @see Array#unshift
    #
    # @api public
    def unshift(*resources)
      super
      resources.each { |resource| relate_resource(resource) }
      self
    end

    ##
    # @see Array#replace
    #
    # @api public
    def replace(other)
      if loaded?
        each { |resource| orphan_resource(resource) }
      end
      super
      other.each { |resource| relate_resource(resource) }
      self
    end

    ##
    # @see Array#pop
    #
    # @api public
    def pop
      orphan_resource(super)
    end

    ##
    # @see Array#shift
    #
    # @api public
    def shift
      orphan_resource(super)
    end

    ##
    # @see Array#delete
    #
    # @api public
    def delete(resource)
      orphan_resource(super)
    end

    ##
    # @see Array#delete_at
    #
    # @api public
    def delete_at(index)
      orphan_resource(super)
    end

    ##
    # @see Array#clear
    #
    # @api public
    def clear
      if loaded?
        each { |resource| orphan_resource(resource) }
      end
      super
      self
    end

    # builds a new resource and appends it to the collection
    #
    # @param Hash[Symbol => Object] attributes attributes which
    #   the new resource should have.
    #
    # @api public
    def build(attributes = {})
      repository.scope do
        resource = model.new(default_attributes.merge(attributes))
        self << resource
        resource
      end
    end

    ##
    # creates a new resource, saves it, and appends it to the collection
    #
    # @param Hash[Symbol => Object] attributes attributes which
    #   the new resource should have.
    #
    # @api public
    def create(attributes = {})
      repository.scope do
        resource = model.create(default_attributes.merge(attributes))
        self << resource unless resource.new_record?
        resource
      end
    end

    def update(attributes = {}, preload = false)
      raise NotImplementedError, 'update *with* validations has not be written yet, try update!'
    end

    ##
    # batch updates the entries belongs to this collection, and skip
    # validations for all resources.
    #
    # @example Reached the Age of Alchohol Consumption
    #   Person.all(:age.gte => 21).update!(:allow_beer => true)
    #
    # @param attributes Hash[Symbol => Object] attributes to update
    # @param reload [FalseClass, TrueClass] if set to true, collection
    #   will have loaded resources reflect updates.
    #
    # @return [TrueClass, FalseClass]
    #   TrueClass indicates that all entries were affected
    #   FalseClass indicates that some entries were affected
    #
    # @api public
    def update!(attributes = {}, reload = false)
      # TODO: delegate to Model.update
      return true if attributes.empty?

      dirty_attributes = {}

      model.properties(repository.name).slice(*attributes.keys).each do |property|
        dirty_attributes[property] = attributes[property.name] if property
      end

      # this should never be done on update! even if collection is loaded. or?
      # each { |resource| resource.attributes = attributes } if loaded?

      changes = repository.update(dirty_attributes, scoped_query)

      # need to decide if this should be done in update!
      query.update(attributes)

      if identity_map.any? && reload
        reload_query = @key_properties.zip(identity_map.keys.transpose).to_hash
        model.all(reload_query.merge(attributes)).reload(:fields => attributes.keys)
      end

      # this should return true if there are any changes at all. as it skips validations
      # the only way it could be fewer changes is if some resources already was updated.
      # that should not return false? true = 'now all objects have these new values'
      return loaded? ? changes == size : changes > 0
    end

    def destroy
      raise NotImplementedError, 'destroy *with* validations has not be written yet, try destroy!'
    end

    ##
    # batch destroy the entries belongs to this collection, and skip
    # validations for all resources.
    #
    # @example The War On Terror (if only it were this easy)
    #   Person.all(:terrorist => true).destroy() #
    #
    # @return [TrueClass, FalseClass]
    #   TrueClass indicates that all entries were affected
    #   FalseClass indicates that some entries were affected
    #
    # @api public
    def destroy!
      # TODO: delegate to Model.destroy
      if loaded?
        return false unless repository.delete(scoped_query) == size

        each do |resource|
          resource.instance_variable_set(:@new_record, true)
          identity_map.delete(resource.key)
          resource.dirty_attributes.clear

          model.properties(repository.name).each do |property|
            next unless resource.attribute_loaded?(property.name)
            resource.dirty_attributes[property] = property.get(resource)
          end
        end
      else
        return false unless repository.delete(scoped_query) > 0
      end

      clear

      true
    end

    ##
    # @return [DataMapper::PropertySet] The set of properties this
    #   query will be retrieving
    #
    # @api public
    def properties
      PropertySet.new(query.fields)
    end

    ##
    # @return [DataMapper::Relationship] The model's relationships
    #
    # @api public
    def relationships
      model.relationships(repository.name)
    end

    ##
    # default values to use when creating a Resource within the Collection
    #
    # @return [Hash] The default attributes for DataMapper::Collection#create
    #
    # @see DataMapper::Collection#create
    #
    # @api public
    def default_attributes
      default_attributes = {}
      query.conditions.each do |tuple|
        operator, property, bind_value = *tuple

        next unless operator == :eql &&
          property.kind_of?(DataMapper::Property) &&
          ![ Array, Range ].any? { |k| bind_value.kind_of?(k) }
          !@key_properties.include?(property)

        default_attributes[property.name] = bind_value
      end
      default_attributes
    end

    ##
    # check to see if collection can respond to the method
    #
    # @param method [Symbol] method to check in the object
    # @param include_private [FalseClass, TrueClass] if set to true,
    #   collection will check private methods
    #
    # @return [TrueClass, FalseClass]
    #   TrueClass indicates the method can be responded to by the collection
    #   FalseClass indicates the method can not be responded to by the collection
    #
    # @api public
    def respond_to?(method, include_private = false)
      super || model.public_methods(false).map { |m| m.to_s }.include?(method.to_s) || relationships.has_key?(method)
    end

    # TODO: add docs
    # @api private
    def _dump(*)
      Marshal.dump([ query, to_a ])
    end

    # TODO: add docs
    # @api private
    def self._load(marshalled)
      query, array = Marshal.load(marshalled)

      # XXX: IMHO it is a code smell to be forced to use allocate
      # and instance_variable_set to load an object.  You should
      # be able to use a constructor to provide all the info needed
      # to initialize an object.  This should be fixed in the edge
      # branch dkubb/dm-core

      collection = allocate
      collection.instance_variable_set(:@query,          query)
      collection.instance_variable_set(:@array,          array)
      collection.instance_variable_set(:@loaded,         true)
      collection.instance_variable_set(:@key_properties, collection.send(:model).key(collection.repository.name))
      collection.instance_variable_set(:@cache,          {})
      collection
    end

    protected

    ##
    # @api private
    def model
      query.model
    end

    private

    ##
    # @api public
    def initialize(query, &block)
      assert_kind_of 'query', query, Query

      unless block_given?
        # It can be helpful (relationship.rb: 112-13, used for SEL) to have a non-lazy Collection.
        block = lambda { |c| }
      end

      @query          = query
      @key_properties = model.key(repository.name)
      @cache          = {}

      super()

      load_with(&block)
    end

    ##
    # @api private
    def add(resource)
      query.add_reversed? ? unshift(resource) : push(resource)
      resource
    end

    ##
    # @api private
    def relate_resource(resource)
      return unless resource
      resource.collection = self
      @cache[resource.key] = resource
      resource
    end

    ##
    # @api private
    def orphan_resource(resource)
      return unless resource
      resource.collection = nil if resource.collection.object_id == self.object_id
      @cache.delete(resource.key)
      resource
    end

    ##
    # @api private
    def scoped_query(query = self.query)
      assert_kind_of 'query', query, Query, Hash

      query.update(keys) if loaded?

      return self.query if query == self.query

      query = if query.kind_of?(Hash)
        Query.new(query.has_key?(:repository) ? query.delete(:repository) : self.repository, model, query)
      else
        query
      end

      if query.limit || query.offset > 0
        set_relative_position(query)
      end

      self.query.merge(query)
    end

    ##
    # @api private
    def keys
      keys = map {|r| r.key }
      keys.any? ? @key_properties.zip(keys.transpose).to_hash : {}
    end

    ##
    # @api private
    def identity_map
      repository.identity_map(model)
    end

    ##
    # @api private
    def set_relative_position(query)
      return if query == self.query

      if query.offset == 0
        return if !query.limit.nil? && !self.query.limit.nil? && query.limit <= self.query.limit
        return if  query.limit.nil? &&  self.query.limit.nil?
      end

      first_pos = self.query.offset + query.offset
      last_pos  = self.query.offset + self.query.limit if self.query.limit

      if limit = query.limit
        if last_pos.nil? || first_pos + limit < last_pos
          last_pos = first_pos + limit
        end
      end

      if last_pos && first_pos >= last_pos
        raise 'outside range'  # TODO: raise a proper exception object
      end

      query.update(:offset => first_pos)
      query.update(:limit => last_pos - first_pos) if last_pos
    end

    ##
    # @api private
    def method_missing(method, *args, &block)
      if model.public_methods(false).map { |m| m.to_s }.include?(method.to_s)
        model.send(:with_scope, query) do
          model.send(method, *args, &block)
        end
      elsif relationship = relationships[method]
        klass = model == relationship.child_model ? relationship.parent_model : relationship.child_model

        # TODO: when self.query includes an offset/limit use it as a
        # subquery to scope the results rather than a join

        query = Query.new(repository, klass)
        query.conditions.push(*self.query.conditions)
        query.update(relationship.query)
        query.update(args.pop) if args.last.kind_of?(Hash)

        query.update(
          :fields => klass.properties(repository.name).defaults,
          :links  => [ relationship ] + self.query.links
        )

        klass.all(query, &block)
      else
        super
      end
    end
  end # class Collection
end # module DataMapper
