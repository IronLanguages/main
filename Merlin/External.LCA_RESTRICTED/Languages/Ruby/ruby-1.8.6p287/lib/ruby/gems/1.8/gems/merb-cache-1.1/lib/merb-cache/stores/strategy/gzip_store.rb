# Pinched from Tobias Luetke's "cacheable" rails plugin (http://github.com/tobi/cacheable/tree/master)
require 'zlib' 
require 'stringio'

module Merb::Cache
  # Store that compresses cached data using GZip.
  # Usually wraps other stores and good for caching of
  # large pages.
  class GzipStore < AbstractStrategyStore
    def writable?(key, parameters = {}, conditions = {})
      true
    end

    def read(key, parameters = {})
      decompress(@stores.capture_first {|c| c.read(key, parameters)})
    end

    def write(key, data = nil, parameters = {}, conditions = {})
      if writable?(key, parameters, conditions)
        @stores.capture_first {|c| c.write(key, compress(data), parameters, conditions)}
      end
    end

    def write_all(key, data = nil, parameters = {}, conditions = {})
      if writable?(key, parameters, conditions)
        @stores.map {|c| c.write_all(key, compress(data), parameters, conditions)}.all?
      end
    end

    def fetch(key, parameters = {}, conditions = {}, &blk)
      wrapper_blk = lambda { compress(blk.call) }
      decompress(read(key, parameters) || @stores.capture_first {|s| s.fetch(key, parameters, conditions, &wrapper_blk)})
    end

    def exists?(key, parameters = {})
      @stores.capture_first {|c| c.exists?(key, parameters)}
    end

    def delete(key, parameters = {})
      @stores.map {|c| c.delete(key, parameters)}.any?
    end

    def delete_all!
      @stores.map {|c| c.delete_all! }.all?
    end

    def compress(data)
      return if data.nil?

      output = StringIO.new
      gz = Zlib::GzipWriter.new(output)
      gz.write(Marshal.dump(data))
      gz.close
      output.string
    end

    def decompress(data)
      return if data.nil?

      Marshal.load(Zlib::GzipReader.new(StringIO.new(data)).read)
    end
  end
end
