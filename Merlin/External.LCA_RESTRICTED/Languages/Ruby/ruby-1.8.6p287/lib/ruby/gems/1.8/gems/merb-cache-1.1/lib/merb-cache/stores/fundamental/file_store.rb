module Merb::Cache
  # Cache store that uses files. Usually this is good for fragment
  # and page caching but not object caching.
  #
  # By default cached files are stored in tmp/cache under Merb.root directory.
  # To use other location pass :dir option to constructor.
  #
  # File caching does not support expiration time.
  class FileStore < AbstractStore
    attr_accessor :dir

    # Creates directory for cached files unless it exists.
    def initialize(config = {})
      @dir = config[:dir] || Merb.root_path(:tmp / :cache)

      create_path(@dir)
    end

    # File caching does not support expiration time.
    def writable?(key, parameters = {}, conditions = {})
      case key
      when String, Numeric, Symbol
        !conditions.has_key?(:expire_in)
      else nil
      end
    end

    # Reads cached template from disk if it exists.
    def read(key, parameters = {})
      if exists?(key, parameters)
        read_file(pathify(key, parameters))
      end
    end

    # Writes cached template to disk, creating cache directory
    # if it does not exist.
    def write(key, data = nil, parameters = {}, conditions = {})
      if writable?(key, parameters, conditions)
        if File.file?(path = pathify(key, parameters))
          write_file(path, data)
        else
          create_path(path) && write_file(path, data)
        end
      end
    end

    # Fetches cached data by key if it exists. If it does not,
    # uses passed block to get new cached value and writes it
    # using given key.
    def fetch(key, parameters = {}, conditions = {}, &blk)
      read(key, parameters) || (writable?(key, parameters, conditions) && write(key, value = blk.call, parameters, conditions) && value)
    end

    # Checks if cached template with given key exists.
    def exists?(key, parameters = {})
      File.file?(pathify(key, parameters))
    end

    # Deletes cached template by key using FileUtils#rm.
    def delete(key, parameters = {})
      if File.file?(path = pathify(key, parameters))
        FileUtils.rm(path)
      end
    end

    def delete_all
      raise NotSupportedError
    end

    def pathify(key, parameters = {})
      if key.to_s =~ /^\//
        path = "#{@dir}#{key}"
      else
        path = "#{@dir}/#{key}"
      end

      path << "--#{parameters.to_sha2}" unless parameters.empty?
      path
    end
    
    protected

    def create_path(path)
      FileUtils.mkdir_p(File.dirname(path))
    end

    # Reads file content. Access to the file
    # uses file locking.
    def read_file(path)
      data = nil
      File.open(path, "r") do |file|
        file.flock(File::LOCK_EX)
        data = file.read
        file.flock(File::LOCK_UN)
      end

      data
    end

    # Writes file content. Access to the file
    # uses file locking.    
    def write_file(path, data)
      File.open(path, "w+") do |file|
        file.flock(File::LOCK_EX)
        file.write(data)
        file.flock(File::LOCK_UN)
      end

      true
    end
  end
end
