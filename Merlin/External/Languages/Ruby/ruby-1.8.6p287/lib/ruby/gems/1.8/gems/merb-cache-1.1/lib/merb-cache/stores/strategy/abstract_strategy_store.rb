# -*- coding: undecided -*-
module Merb::Cache
  # A strategy store wraps one or
  # more fundamental stores, acting as a middle man between caching
  # requests.
  #
  # For example, if you need to save memory on your Memcache server,
  # you could wrap your MemcachedStore with a GzipStore. This would
  # automatically compress the cached data when put into the cache, and
  # decompress it on the way out. You can even wrap strategy caches
  # with other strategy caches. If your key was comprised of sensitive
  # information, like a SSN, you might want to encrypt the key before
  # storage. Wrapping your GzipStore in a SHA1Store would take
  # care of that for you.
  #
  # The AbstractStore class defines 9 methods as the API:
  # 
  # writable?(key, parameters = {}, conditions = {})
  # exists?(key, parameters = {})
  # read(key, parameters = {})
  # write(key, data = nil, parameters = {}, conditions = {})
  # write_all(key, data = nil, parameters = {}, conditions = {})
  # fetch(key, parameters = {}, conditions = {}, &blk)
  # delete(key, parameters = {})
  # delete_all
  # delete_all!
  # 
  # AbstractStrategyStore implements all of these with the exception
  # of delete_all. If a strategy store can guarantee that calling
  # delete_all on it’s wrapped store(s) will only delete entries
  # populated by the strategy store, it may define the safe
  # version of delete_all. However, this is usually not the
  # case, hence delete_all is not part of the
  # public API for AbstractStrategyStore.
  class AbstractStrategyStore < AbstractStore
    # START: interface for creating strategy stores.  This should/might change.
    def self.contextualize(*stores)
        Class.new(self) do
          cattr_accessor :contextualized_stores

          self.contextualized_stores = stores
        end
      end

      class << self
        alias_method :[], :contextualize
      end

      attr_accessor :stores

      def initialize(config = {})
        @stores = contextualized_stores.map do |cs|
          case cs
          when Symbol
            Merb::Cache[cs]
          when Class
            cs.new(config)
          end
        end
      end

    # END: interface for creating strategy stores.

    attr_accessor :stores

    # determines if the store is able to persist data identified by the key & parameters
    # with the given conditions.
    def writable?(key, parameters = {}, conditions = {})
      raise NotImplementedError
    end

    # gets the data from the store identified by the key & parameters.
    # return nil if the entry does not exist.
    def read(key, parameters = {})
      raise NotImplementedError
    end

    # persists the data so that it can be retrieved by the key & parameters.
    # returns nil if it is unable to persist the data.
    # returns true if successful.
    def write(key, data = nil, parameters = {}, conditions = {})
      raise NotImplementedError
    end

    # persists the data to all context stores.
    # returns nil if none of the stores were able to persist the data.
    # returns true if at least one write was successful.
    def write_all(key, data = nil, parameters = {}, conditions = {})
      raise NotImplementedError
    end

    # tries to read the data from the store.  If that fails, it calls
    # the block parameter and persists the result.
    def fetch(key, parameters = {}, conditions = {}, &blk)
      raise NotImplementedError
    end

    # returns true/false/nil based on if data identified by the key & parameters
    # is persisted in the store.
    def exists?(key, parameters = {})
      raise NotImplementedError
    end

    # deletes the entry for the key & parameter from the store.
    def delete(key, parameters = {})
      raise NotImplementedError
    end

    # deletes all entries for the key & parameters for the store.
    # considered dangerous because strategy stores which call delete_all!
    # on their context stores could delete other store's entrees.
    def delete_all!
      raise NotImplementedError
    end

    def clone
      twin = super
      twin.stores = self.stores.map {|s| s.clone}
      twin
    end
    
  end
end
