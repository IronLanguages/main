##
# A utility wrapper around the MemCache client to simplify cache access.  All
# methods silently ignore MemCache errors.
#
# This API is deprecated, please use the Rails.cache API or your own wrapper API
# around MemCache.

module Cache

  ##
  # Try to return a logger object that does not rely
  # on ActiveRecord for logging.
  def self.logger
    @logger ||= if defined? Rails.logger # Rails 2.1 +
      Rails.logger
    elsif defined? RAILS_DEFAULT_LOGGER # Rails 1.2.2 +
      RAILS_DEFAULT_LOGGER
    else
      ActiveRecord::Base.logger # ... very old Rails.
    end
  end
  ##
  # Returns the object at +key+ from the cache if successful, or nil if either
  # the object is not in the cache or if there was an error attermpting to
  # access the cache.
  #
  # If there is a cache miss and a block is given the result of the block will
  # be stored in the cache with optional +expiry+, using the +add+ method rather
  # than +set+.

  def self.get(key, expiry = 0)
    start_time = Time.now
    value = CACHE.get key
    elapsed = Time.now - start_time
    logger.debug('MemCache Get (%0.6f)  %s' % [elapsed, key])
    if value.nil? and block_given? then
      value = yield
      add key, value, expiry
    end
    value
  rescue MemCache::MemCacheError => err
    logger.debug "MemCache Error: #{err.message}"
    if block_given? then
      value = yield
      put key, value, expiry
    end
    value
  end

  ##
  # Sets +value+ in the cache at +key+, with an optional +expiry+ time in
  # seconds.

  def self.put(key, value, expiry = 0)
    start_time = Time.now
    CACHE.set key, value, expiry
    elapsed = Time.now - start_time
    logger.debug('MemCache Set (%0.6f)  %s' % [elapsed, key])
    value
  rescue MemCache::MemCacheError => err
    ActiveRecord::Base.logger.debug "MemCache Error: #{err.message}"
    nil
  end

  ##
  # Sets +value+ in the cache at +key+, with an optional +expiry+ time in
  # seconds.  If +key+ already exists in cache, returns nil.

  def self.add(key, value, expiry = 0)
    start_time = Time.now
    response = CACHE.add key, value, expiry
    elapsed = Time.now - start_time
    logger.debug('MemCache Add (%0.6f)  %s' % [elapsed, key])
    (response == "STORED\r\n") ? value : nil
  rescue MemCache::MemCacheError => err
    ActiveRecord::Base.logger.debug "MemCache Error: #{err.message}"
    nil
  end

  ##
  # Deletes +key+ from the cache in +delay+ seconds.

  def self.delete(key, delay = nil)
    start_time = Time.now
    CACHE.delete key, delay
    elapsed = Time.now - start_time
    logger.debug('MemCache Delete (%0.6f)  %s' %
                                    [elapsed, key])
    nil
  rescue MemCache::MemCacheError => err
    logger.debug "MemCache Error: #{err.message}"
    nil
  end

  ##
  # Resets all connections to MemCache servers.

  def self.reset
    CACHE.reset
    logger.debug 'MemCache Connections Reset'
    nil
  end

end

