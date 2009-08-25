require 'digest/sha1'

module Merb::Cache
  # Strategy store that uses SHA1 hex of
  # base cache key and parameters as
  # cache key.
  #
  # It is good for caching of expensive
  # search queries that use multiple
  # parameters passed via query string
  # of request.
  class SHA1Store < AbstractStrategyStore
    def initialize(config = {})
      super(config)
      @map = {}
    end

    def writable?(key, parameters = {}, conditions = {})
      case key
      when String, Numeric, Symbol
        @stores.any? {|c| c.writable?(digest(key, parameters), {}, conditions)}
      else nil
      end
    end

    def read(key, parameters = {})
      @stores.capture_first {|c| c.read(digest(key, parameters))}
    end

    def write(key, data = nil, parameters = {}, conditions = {})
      if writable?(key, parameters, conditions)
        @stores.capture_first {|c| c.write(digest(key, parameters), data, {}, conditions)}
      end
    end

    def write_all(key, data = nil, parameters = {}, conditions = {})
      if writable?(key, parameters, conditions)
        @stores.map {|c| c.write_all(digest(key, parameters), data, {}, conditions)}.all?
      end
    end

    def fetch(key, parameters = {}, conditions = {}, &blk)
      read(key, parameters) || (writable?(key, parameters, conditions) && @stores.capture_first {|c| c.fetch(digest(key, parameters), {}, conditions, &blk)})
    end

    def exists?(key, parameters = {})
      @stores.capture_first {|c| c.exists?(digest(key, parameters))}
    end

    def delete(key, parameters = {})
      @stores.map {|c| c.delete(digest(key, parameters))}.any?
    end

    def delete_all!
      @stores.map {|c| c.delete_all! }.all?
    end

    def digest(key, parameters = {})
      @map[[key, parameters]] ||= Digest::SHA1.hexdigest("#{key}#{parameters.to_sha2}")
    end
  end
end
