module DataMapper
  class Repository
    include Assertions

    @adapters = {}

    ##
    #
    # @return <Adapter> the adapters registered for this repository
    def self.adapters
      @adapters
    end

    def self.context
      Thread.current[:dm_repository_contexts] ||= []
    end

    def self.default_name
      :default
    end

    attr_reader :name

    def adapter
      # Make adapter instantiation lazy so we can defer repository setup until it's actually
      # needed. Do not remove this code.
      @adapter ||= begin
        raise ArgumentError, "Adapter not set: #{@name}. Did you forget to setup?" \
          unless self.class.adapters.has_key?(@name)

        self.class.adapters[@name]
      end
    end

    def identity_map(model)
      @identity_maps[model] ||= IdentityMap.new
    end

    # TODO: spec this
    def scope
      Repository.context << self

      begin
        return yield(self)
      ensure
        Repository.context.pop
      end
    end

    def create(resources)
      adapter.create(resources)
    end

    ##
    # retrieve a collection of results of a query
    #
    # @param <Query> query composition of the query to perform
    # @return <DataMapper::Collection> result set of the query
    # @see DataMapper::Query
    def read_many(query)
      adapter.read_many(query)
    end

    ##
    # retrieve a resource instance by a query
    #
    # @param <Query> query composition of the query to perform
    # @return <DataMapper::Resource> the first retrieved instance which matches the query
    # @return <NilClass> no object could be found which matches that query
    # @see DataMapper::Query
    def read_one(query)
      adapter.read_one(query)
    end

    def update(attributes, query)
      adapter.update(attributes, query)
    end

    def delete(query)
      adapter.delete(query)
    end

    def eql?(other)
      return true if super
      name == other.name
    end

    alias == eql?

    def to_s
      "#<DataMapper::Repository:#{@name}>"
    end

    def _dump(*)
      name.to_s
    end

    def self._load(marshalled)
      new(marshalled.to_sym)
    end

    private

    def initialize(name)
      assert_kind_of 'name', name, Symbol

      @name          = name
      @identity_maps = {}
    end

    # TODO: move to dm-more/dm-migrations
    module Migration
      # TODO: move to dm-more/dm-migrations
      def map(*args)
        type_map.map(*args)
      end

      # TODO: move to dm-more/dm-migrations
      def type_map
        @type_map ||= TypeMap.new(adapter.class.type_map)
      end

      ##
      #
      # @return <True, False> whether or not the data-store exists for this repo
      #
      # TODO: move to dm-more/dm-migrations
      def storage_exists?(storage_name)
        adapter.storage_exists?(storage_name)
      end

      # TODO: move to dm-more/dm-migrations
      def migrate!
        Migrator.migrate(name)
      end

      # TODO: move to dm-more/dm-migrations
      def auto_migrate!
        AutoMigrator.auto_migrate(name)
      end

      # TODO: move to dm-more/dm-migrations
      def auto_upgrade!
        AutoMigrator.auto_upgrade(name)
      end
    end

    include Migration

    # TODO: move to dm-more/dm-transactions
    module Transaction
      ##
      # Produce a new Transaction for this Repository
      #
      #
      # @return <DataMapper::Adapters::Transaction> a new Transaction (in state
      #   :none) that can be used to execute code #with_transaction
      #
      # TODO: move to dm-more/dm-transactions
      def transaction
        DataMapper::Transaction.new(self)
      end
    end

    include Transaction
  end # class Repository
end # module DataMapper
