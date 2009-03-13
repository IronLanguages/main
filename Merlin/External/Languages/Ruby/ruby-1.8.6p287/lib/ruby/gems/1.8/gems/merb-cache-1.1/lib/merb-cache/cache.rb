module Merb
  # A convinient way to get at Merb::Cache
  def self.cache
    Merb::Cache
  end

  module Cache

    def self.setup(&blk)
      if Merb::BootLoader.finished?(Merb::BootLoader::BeforeAppLoads)
        instance_eval(&blk) unless blk.nil?
      else
        Merb::BootLoader.before_app_loads do
          instance_eval(&blk) unless blk.nil?
        end
      end
    end

    # autoload is used so that gem dependencies can be required only when needed by
    # adding the require statement in the store file.
    autoload :AbstractStore,    "merb-cache" / "stores" / "fundamental" / "abstract_store"
    autoload :FileStore,        "merb-cache" / "stores" / "fundamental" / "file_store"
    autoload :MemcachedStore,   "merb-cache" / "stores" / "fundamental" / "memcached_store"
    
    autoload :AbstractStrategyStore,  "merb-cache" / "stores" / "strategy" / "abstract_strategy_store"
    autoload :ActionStore,            "merb-cache" / "stores" / "strategy" / "action_store"
    autoload :AdhocStore,             "merb-cache" / "stores" / "strategy" / "adhoc_store"
    autoload :GzipStore,              "merb-cache" / "stores" / "strategy" / "gzip_store"
    autoload :PageStore,              "merb-cache" / "stores" / "strategy" / "page_store"
    autoload :SHA1Store,              "merb-cache" / "stores" / "strategy" / "sha1_store"

    
    class << self
      attr_accessor :stores
    end

    self.stores = {}

    # Cache store lookup
    # name<Symbol> : The name of a registered store
    # Returns<Nil AbstractStore> : A thread-safe copy of the store
    def self.[](*names)
      if names.size == 1
        Thread.current[:'merb-cache'] ||= {}
        (Thread.current[:'merb-cache'][names.first] ||= stores[names.first].clone)
      else
        AdhocStore[*names]
      end
    rescue TypeError
      raise(StoreNotFound, "Could not find the :#{names.first} store")
    end

    # Clones the cache stores for the current thread
    def self.clone_stores
      @stores.inject({}) {|h, (k, s)| h[k] = s.clone; h}
    end

    # Registers the cache store name with a type & options
    # name<Symbol> : An optional symbol to give the cache.  :default is used if no name is given.
    # klass<Class> : A store type.
    # opts<Hash> : A hash to pass through to the store for configuration.
    def self.register(name, klass = nil, opts = {})
      klass, opts = nil, klass if klass.is_a? Hash
      name, klass = default_store_name, name if klass.nil?

      raise StoreExists, "#{name} store already setup" if @stores.has_key?(name)

      @stores[name] = (AdhocStore === klass) ? klass : klass.new(opts)
    end
    
    # Checks to see if a given store exists already.
    def self.exists?(name)
      return true if self[name]
    rescue StoreNotFound
      return false
    end

    # Default store name is :default.
    def self.default_store_name
      :default
    end

    class NotSupportedError < Exception; end
    
    class StoreExists < Exception; end

    # Raised when requested store cannot be found on the list of registered.
    class StoreNotFound < Exception; end
  end #Cache
end #Merb
