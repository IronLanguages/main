require 'set'

module DataMapper
  module Resource
    include Assertions

    ##
    #
    # Appends a module for inclusion into the model class after
    # DataMapper::Resource.
    #
    # This is a useful way to extend DataMapper::Resource while still retaining
    # a self.included method.
    #
    # @param [Module] inclusion the module that is to be appended to the module
    #   after DataMapper::Resource
    #
    # @return [TrueClass, FalseClass] whether or not the inclusions have been
    #   successfully appended to the list
    # @return <TrueClass, FalseClass>
    #-
    # @api public
    def self.append_inclusions(*inclusions)
      extra_inclusions.concat inclusions
      true
    end

    def self.extra_inclusions
      @extra_inclusions ||= []
    end

    # When Resource is included in a class this method makes sure
    # it gets all the methods
    #
    # -
    # @api private
    def self.included(model)
      model.extend Model
      model.extend ClassMethods if defined?(ClassMethods)
      model.const_set('Resource', self) unless model.const_defined?('Resource')
      extra_inclusions.each { |inclusion| model.send(:include, inclusion) }
      descendants << model
      class << model
        @_valid_model = false
        attr_reader :_valid_model
      end
    end

    # Return all classes that include the DataMapper::Resource module
    #
    # ==== Returns
    # Set:: a set containing the including classes
    #
    # ==== Example
    #
    #   Class Foo
    #     include DataMapper::Resource
    #   end
    #
    #   DataMapper::Resource.descendants.to_a.first == Foo
    #
    # -
    # @api semipublic
    def self.descendants
      @descendants ||= Set.new
    end

    # +---------------
    # Instance methods

    attr_writer :collection

    alias model class

    # returns the value of the attribute. Do not read from instance variables directly,
    # but use this method. This method handels the lazy loading the attribute and returning
    # of defaults if nessesary.
    #
    # ==== Parameters
    # name<Symbol>:: name attribute to lookup
    #
    # ==== Returns
    # <Types>:: the value stored at that given attribute, nil if none, and default if necessary
    #
    # ==== Example
    #
    #   Class Foo
    #     include DataMapper::Resource
    #
    #     property :first_name, String
    #     property :last_name, String
    #
    #     def full_name
    #       "#{attribute_get(:first_name)} #{attribute_get(:last_name)}"
    #     end
    #
    #     # using the shorter syntax
    #     def name_for_address_book
    #       "#{last_name}, #{first_name}"
    #     end
    #   end
    #
    # -
    # @api semipublic
    def attribute_get(name)
      properties[name].get(self)
    end

    # sets the value of the attribute and marks the attribute as dirty
    # if it has been changed so that it may be saved. Do not set from
    # instance variables directly, but use this method. This method
    # handels the lazy loading the property and returning of defaults
    # if nessesary.
    #
    # ==== Parameters
    # name<Symbol>:: name attribute to set
    # value<Type>:: value to store at that location
    #
    # ==== Returns
    # <Types>:: the value stored at that given attribute, nil if none, and default if necessary
    #
    # ==== Example
    #
    #   Class Foo
    #     include DataMapper::Resource
    #
    #     property :first_name, String
    #     property :last_name, String
    #
    #     def full_name(name)
    #       name = name.split(' ')
    #       attribute_set(:first_name, name[0])
    #       attribute_set(:last_name, name[1])
    #     end
    #
    #     # using the shorter syntax
    #     def name_from_address_book(name)
    #       name = name.split(', ')
    #       first_name = name[1]
    #       last_name = name[0]
    #     end
    #   end
    #
    # -
    # @api semipublic
    def attribute_set(name, value)
      properties[name].set(self, value)
    end

    # Compares if its the same object or if attributes are equal
    #
    # The comparaison is
    #  * false if object not from same repository
    #  * false if object has no all same properties
    #
    #
    # ==== Parameters
    # other<Object>:: Object to compare to
    #
    # ==== Returns
    # <True>:: the outcome of the comparison as a boolean
    #
    # -
    # @api public
    def eql?(other)
      return true if equal?(other)

      # two instances for different models cannot be equivalent
      return false unless other.kind_of?(model)

      # two instances with different keys cannot be equivalent
      return false if key != other.key

      # neither object has changed since loaded, so they are equivalent
      return true if repository == other.repository && !dirty? && !other.dirty?

      # get all the loaded and non-loaded properties that are not keys,
      # since the key comparison was performed earlier
      loaded, not_loaded = properties.select { |p| !p.key? }.partition do |property|
        attribute_loaded?(property.name) && other.attribute_loaded?(property.name)
      end

      # check all loaded properties, and then all unloaded properties
      (loaded + not_loaded).all? { |p| p.get(self) == p.get(other) }
    end

    alias == eql?

    # Computes a hash for the resource
    #
    # ==== Returns
    # <Integer>:: the hash value of the resource
    #
    # -
    # @api public
    def hash
      model.hash + key.hash
    end

    # Inspection of the class name and the attributes
    #
    # ==== Returns
    # <String>:: with the class name, attributes with their values
    #
    # ==== Example
    #
    # >> Foo.new
    # => #<Foo name=nil updated_at=nil created_at=nil id=nil>
    #
    # -
    # @api public
    def inspect
      attrs = []

      properties.each do |property|
        value = if !attribute_loaded?(property.name) && !new_record?
          '<not loaded>'
        else
          send(property.getter).inspect
        end

        attrs << "#{property.name}=#{value}"
      end

      "#<#{model.name} #{attrs * ' '}>"
    end

    # TODO docs
    def pretty_print(pp)
      pp.group(1, "#<#{model.name}", ">") do
        pp.breakable
        pp.seplist(attributes.to_a) do |k_v|
          pp.text k_v[0].to_s
          pp.text " = "
          pp.pp k_v[1]
        end
      end
    end

    ##
    #
    # ==== Returns
    # <Repository>:: the respository this resource belongs to in the context of a collection OR in the class's context
    #
    # @api public
    def repository
      @repository || model.repository
    end

    # default id method to return the resource id when there is a
    # single key, and the model was defined with a primary key named
    # something other than id
    #
    # ==== Returns
    # <Array[Key], Key> key or keys
    #
    # --
    # @api public
    def id
      key = self.key
      key.first if key.size == 1
    end

    def key
      key_properties.map do |property|
        original_values[property.name] || property.get!(self)
      end
    end

    def readonly!
      @readonly = true
    end

    def readonly?
      @readonly == true
    end

    # save the instance to the data-store
    #
    # ==== Returns
    # <True, False>:: results of the save
    #
    # @see DataMapper::Repository#save
    #
    # --
    # #public
    def save(context = :default)
      # Takes a context, but does nothing with it. This is to maintain the
      # same API through out all of dm-more. dm-validations requires a
      # context to be passed

      associations_saved = false
      child_associations.each { |a| associations_saved |= a.save }

      saved = new_record? ? create : update

      if saved
        original_values.clear
      end

      parent_associations.each { |a| associations_saved |= a.save }

      # We should return true if the model (or any of its associations)
      # were saved.
      (saved | associations_saved) == true
    end

    # destroy the instance, remove it from the repository
    #
    # ==== Returns
    # <True, False>:: results of the destruction
    #
    # --
    # @api public
    def destroy
      return false if new_record?
      return false unless repository.delete(to_query)

      @new_record = true
      repository.identity_map(model).delete(key)
      original_values.clear

      properties.each do |property|
        # We'll set the original value to nil as if we had a new record
        original_values[property.name] = nil if attribute_loaded?(property.name)
      end

      true
    end

    # Checks if the attribute has been loaded
    #
    # ==== Example
    #
    #   class Foo
    #     include DataMapper::Resource
    #     property :name, String
    #     property :description, Text, :lazy => false
    #   end
    #
    #   Foo.new.attribute_loaded?(:description) # will return false
    #
    # --
    # @api public
    def attribute_loaded?(name)
      instance_variable_defined?(properties[name].instance_variable_name)
    end

    # fetches all the names of the attributes that have been loaded,
    # even if they are lazy but have been called
    #
    # ==== Returns
    # Array[<Symbol>]:: names of attributes that have been loaded
    #
    # ==== Example
    #
    #   class Foo
    #     include DataMapper::Resource
    #     property :name, String
    #     property :description, Text, :lazy => false
    #   end
    #
    #   Foo.new.loaded_attributes # returns [:name]
    #
    # --
    # @api public
    def loaded_attributes
      properties.map{|p| p.name if attribute_loaded?(p.name)}.compact
    end

    # set of original values of properties
    #
    # ==== Returns
    # Hash:: original values of properties
    #
    # --
    # @api public
    def original_values
      @original_values ||= {}
    end

    # Hash of attributes that have been marked dirty
    #
    # ==== Returns
    # Hash:: attributes that have been marked dirty
    #
    # --
    # @api private
    def dirty_attributes
      dirty_attributes = {}
      properties       = self.properties

      original_values.each do |name, old_value|
        property  = properties[name]
        new_value = property.get!(self)

        dirty = case property.track
        when :hash then old_value != new_value.hash
        else
          property.value(old_value) != property.value(new_value)
        end

        if dirty
          property.hash
          dirty_attributes[property] = property.value(new_value)
        end
      end

      dirty_attributes
    end

    # Checks if the class is dirty
    #
    # ==== Returns
    # True:: returns if class is dirty
    #
    # --
    # @api public
    def dirty?
      dirty_attributes.any?
    end

    # Checks if the attribute is dirty
    #
    # ==== Parameters
    # name<Symbol>:: name of attribute
    #
    # ==== Returns
    # True:: returns if attribute is dirty
    #
    # --
    # @api public
    def attribute_dirty?(name)
      dirty_attributes.has_key?(properties[name])
    end

    def collection
      @collection ||= if query = to_query
        Collection.new(query) { |c| c << self }
      end
    end

    # Reload association and all child association
    #
    # ==== Returns
    # self:: returns the class itself
    #
    # --
    # @api public
    def reload
      unless new_record?
        reload_attributes(*loaded_attributes)
        (parent_associations + child_associations).each { |association| association.reload }
      end

      self
    end

    # Reload specific attributes
    #
    # ==== Parameters
    # *attributes<Array[<Symbol>]>:: name of attribute
    #
    # ==== Returns
    # self:: returns the class itself
    #
    # --
    # @api public
    def reload_attributes(*attributes)
      unless attributes.empty? || new_record?
        collection.reload(:fields => attributes)
      end

      self
    end

    # Checks if the model has been saved
    #
    # ==== Returns
    # True:: status if the model is new
    #
    # --
    # @api public
    def new_record?
      !defined?(@new_record) || @new_record
    end

    # all the attributes of the model
    #
    # ==== Returns
    # Hash[<Symbol>]:: All the (non)-lazy attributes
    #
    # --
    # @api public
    def attributes
      properties.map do |p|
        [p.name, send(p.getter)] if p.reader_visibility == :public
      end.compact.to_hash
    end

    # Mass assign of attributes
    #
    # ==== Parameters
    # value_hash <Hash[<Symbol>]>::
    #
    # --
    # @api public
    def attributes=(values_hash)
      values_hash.each do |name, value|
        name = name.to_s.sub(/\?\z/, '')

        if self.class.public_method_defined?(setter = "#{name}=")
          send(setter, value)
        else
          raise ArgumentError, "The property '#{name}' is not a public property."
        end
      end
    end

    # Updates attributes and saves model
    #
    # ==== Parameters
    # attributes<Hash> Attributes to be updated
    # keys<Symbol, String, Array> keys of Hash to update (others won't be updated)
    #
    # ==== Returns
    # <TrueClass, FalseClass> if model got saved or not
    #
    #-
    # @api public
    def update_attributes(hash, *update_only)
      unless hash.is_a?(Hash)
        raise ArgumentError, "Expecting the first parameter of " +
          "update_attributes to be a hash; got #{hash.inspect}"
      end
      loop_thru = update_only.empty? ? hash.keys : update_only
      loop_thru.each { |attr|  send("#{attr}=", hash[attr]) }
      save
    end

    # TODO: add docs
    def to_query(query = {})
      model.to_query(repository, key, query) unless new_record?
    end

    # TODO: add docs
    # @api private
    def _dump(*)
      ivars = {}

      # dump all the loaded properties
      properties.each do |property|
        next unless attribute_loaded?(property.name)
        ivars[property.instance_variable_name] = property.get!(self)
      end

      # dump ivars used internally
      %w[ @new_record @original_values @readonly @repository ].each do |name|
        ivars[name] = instance_variable_get(name)
      end

      Marshal.dump(ivars)
    end

    protected

    def properties
      model.properties(repository.name)
    end

    def key_properties
      model.key(repository.name)
    end

    def relationships
      model.relationships(repository.name)
    end

    # Needs to be a protected method so that it is hookable
    def create
      # Can't create a resource that is not dirty and doesn't have serial keys
      return false if new_record? && !dirty? && !model.key.any? { |p| p.serial? }
      # set defaults for new resource
      properties.each do |property|
        next if attribute_loaded?(property.name)
        property.set(self, property.default_for(self))
      end

      return false unless repository.create([ self ]) == 1

      @repository = repository
      @new_record = false

      repository.identity_map(model).set(key, self)

      true
    end

    # Needs to be a protected method so that it is hookable
    def update
      dirty_attributes = self.dirty_attributes
      return true if dirty_attributes.empty?
      repository.update(dirty_attributes, to_query) == 1
    end

    private

    def initialize(attributes = {}) # :nodoc:
      assert_valid_model
      self.attributes = attributes
    end

    def assert_valid_model # :nodoc:
      return if self.class._valid_model
      properties = self.properties

      if properties.empty? && relationships.empty?
        raise IncompleteResourceError, "#{model.name} must have at least one property or relationship to be initialized."
      end

      if properties.key.empty?
        raise IncompleteResourceError, "#{model.name} must have a key."
      end

      self.class.instance_variable_set("@_valid_model", true)
    end

    # TODO document
    # @api semipublic
    def attribute_get!(name)
      properties[name].get!(self)
    end

    # TODO document
    # @api semipublic
    def attribute_set!(name, value)
      properties[name].set!(self, value)
    end

    def lazy_load(name)
      reload_attributes(*properties.lazy_load_context(name) - loaded_attributes)
    end

    def child_associations
      @child_associations ||= []
    end

    def parent_associations
      @parent_associations ||= []
    end

    # TODO: move to dm-more/dm-transactions
    module Transaction
      # Produce a new Transaction for the class of this Resource
      #
      # ==== Returns
      # <DataMapper::Adapters::Transaction>::
      #   a new DataMapper::Adapters::Transaction with all DataMapper::Repositories
      #   of the class of this DataMapper::Resource added.
      #-
      # @api public
      #
      # TODO: move to dm-more/dm-transactions
      def transaction
        model.transaction { |*block_args| yield(*block_args) }
      end
    end # module Transaction

    include Transaction
  end # module Resource
end # module DataMapper
