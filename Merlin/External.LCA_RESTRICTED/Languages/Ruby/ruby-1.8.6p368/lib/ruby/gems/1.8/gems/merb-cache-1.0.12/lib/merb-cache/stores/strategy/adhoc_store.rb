module Merb::Cache
  # General purpose store, use for your own
  # contexts. Since it wraps access to multiple
  # fundamental stores, it's easy to use
  # this strategy store with distributed cache
  # stores like Memcached.
  class AdhocStore < AbstractStrategyStore
    class << self
      alias_method :[], :new
    end

    attr_accessor :stores

    def initialize(*names)
      @stores = names.map {|n| Merb::Cache[n]}
    end

    def writable?(key, parameters = {}, conditions = {})
      @stores.capture_first {|s| s.writable?(key, parameters, conditions)}
    end

    # gets the data from the store identified by the key & parameters.
    # return nil if the entry does not exist.
    def read(key, parameters = {})
      @stores.capture_first {|s| s.read(key, parameters)}
    end

    # persists the data so that it can be retrieved by the key & parameters.
    # returns nil if it is unable to persist the data.
    # returns true if successful.
    def write(key, data = nil, parameters = {}, conditions = {})
      @stores.capture_first {|s| s.write(key, data, parameters, conditions)}
    end

    # persists the data to all context stores.
    # returns nil if none of the stores were able to persist the data.
    # returns true if at least one write was successful.
    def write_all(key, data = nil, parameters = {}, conditions = {})
      @stores.map {|s| s.write_all(key, data, parameters, conditions)}.all?
    end

    # tries to read the data from the store.  If that fails, it calls
    # the block parameter and persists the result.  If it cannot be fetched,
    # the block call is returned.
    def fetch(key, parameters = {}, conditions = {}, &blk)
      read(key, parameters) ||
        @stores.capture_first {|s| s.fetch(key, parameters, conditions, &blk)} ||
        blk.call
    end

    # returns true/false/nil based on if data identified by the key & parameters
    # is persisted in the store.
    def exists?(key, parameters = {})
      @stores.capture_first {|s| s.exists?(key, parameters)}
    end

    # deletes the entry for the key & parameter from the store.
    def delete(key, parameters = {})
      @stores.map {|s| s.delete(key, parameters)}.any?
    end

    # deletes all entries for the key & parameters for the store.
    # considered dangerous because strategy stores which call delete_all!
    # on their context stores could delete other store's entrees.
    def delete_all!
      @stores.map {|s| s.delete_all!}.all?
    end
  end
end
