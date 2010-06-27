class Merb::Cache::AbstractStore

  def initialize(config = {}); end

  # determines if the store is able to persist data identified by the key & parameters
  # with the given conditions.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @param [Hash] conditions optional conditions that place constraints or detail instructions for storing an entry
  # @return [TrueClass] the ability of the store to write an entry based on the key, parameters, and conditions
  # @raise [NotImplementedError] API method has not been implemented
  def writable?(key, parameters = {}, conditions = {})
    raise NotImplementedError
  end

  # gets the data from the store identified by the key & parameters.
  # return nil if the entry does not exist.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @return [Object, NilClass] the match entry, or nil if no entry exists matching the key and parameters
  # @raise [NotImplementedError] API method has not been implemented
  def read(key, parameters = {})
    raise NotImplementedError
  end

  # persists the data so that it can be retrieved by the key & parameters.
  # returns nil if it is unable to persist the data.
  # returns true if successful.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param data the object to persist as an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @param [Hash] conditions optional conditions that place constraints or detail instructions for storing an entry
  # @return [TrueClass, NilClass] true if the entry was successfully written, otherwise nil
  # @raise [NotImplementedError] API method has not been implemented
  def write(key, data = nil, parameters = {}, conditions = {})
    raise NotImplementedError
  end

  # @param [#to_s] key the key used to identify an entry
  # @param data the object to persist as an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @param [Hash] conditions optional conditions that place constraints or detail instructions for storing an entry
  # @return [TrueClass, NilClass] true if the entry was successfully written, otherwise nil
  # @raise [NotImplementedError] API method has not been implemented
  def write_all(key, data = nil, parameters = {}, conditions = {})
    write(key, data, parameters, conditions)
  end

  # tries to read the data from the store.  If that fails, it calls
  # the block parameter and persists the result.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @param [Hash] conditions optional conditions that place constraints or detail instructions for storing an entry
  # @return [Object, NilClass] the match entry or the result of the block call, or nil if the entry is not successfully written
  # @raise [NotImplementedError] API method has not been implemented
  def fetch(key, parameters = {}, conditions = {}, &blk)
    raise NotImplementedError
  end

  # returns true/false/nil based on if data identified by the key & parameters
  # is persisted in the store.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @return [TrueClass] true if the key and parameters match an entry in the store, false otherwise
  # @raise [NotImplementedError] API method has not been implemented
  def exists?(key, parameters = {})
    raise NotImplementedError
  end

  # deletes the entry for the key & parameter from the store.
  #
  # @param [#to_s] key the key used to identify an entry
  # @param [Hash] parameters optional parameters used to identify an entry
  # @raise [TrueClass] true if the an entry matching the key and parameters is successfully deleted, false otherwise
  # @raise [NotImplementedError] API method has not been implemented
  def delete(key, parameters = {})
    raise NotImplementedError
  end

  # deletes all entries for the key & parameters for the store.
  #
  # @return [TrueClass] true if all entries in the store are erased, false otherwise
  # @raise [NotImplementedError] API method has not been implemented
  def delete_all
    raise NotImplementedError
  end

  # dangerous version of delete_all.  Used by strategy stores, which may delete entries not associated with the strategy
  # store making the call.
  #
  # @return [TrueClass] true if all entries in the store are erased, false otherwise
  # @raise [NotImplementedError] API method has not been implemented
  def delete_all!
    delete_all
  end
end
