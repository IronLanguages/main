require 'digest'
require 'digest/sha2'

module DataObjects

  class Transaction

    # The host name. Note, this relies on the host name being configured and resolvable using DNS
    HOST = "#{Socket::gethostbyname(Socket::gethostname)[0]}" rescue "localhost"
    @@counter = 0

    # The connection object allocated for this transaction
    attr_reader :connection
    # A unique ID for this transaction
    attr_reader :id

    # Instantiate the Transaction subclass that's appropriate for this uri scheme
    def self.create_for_uri(uri)
      uri = uri.is_a?(String) ? URI::parse(uri) : uri
      DataObjects.const_get(uri.scheme.capitalize)::Transaction.new(uri)
    end

    #
    # Creates a Transaction bound to a connection for the given DataObjects::URI
    #
    def initialize(uri)
      @connection = DataObjects::Connection.new(uri)
      @id = Digest::SHA256.hexdigest("#{HOST}:#{$$}:#{Time.now.to_f}:#{@@counter += 1}")
    end

    # Close the connection for this Transaction
    def close
      @connection.close
    end

    # Begin the Transaction
    def begin; not_implemented; end
    # Commit changes made in this Transaction
    def commit; not_implemented; end
    # Rollback changes made in this Transaction
    def rollback; not_implemented; end;
    # Prepare this Transaction for the second phase of a two-phase commit
    def prepare; not_implemented; end;
    # Abandon the second phase of a two-phase commit and roll back the changes
    def rollback_prepared; not_implemented; end;

  private
    def not_implemented
      raise NotImplementedError
    end
  end
end
