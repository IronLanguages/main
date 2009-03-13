require 'digest'
require 'digest/sha2'

module DataObjects

  class Transaction

    HOST = "#{Socket::gethostbyname(Socket::gethostname)[0]}" rescue "localhost"
    @@counter = 0

    attr_reader :connection
    attr_reader :id

    def self.create_for_uri(uri)
      uri = uri.is_a?(String) ? URI::parse(uri) : uri
      DataObjects.const_get(uri.scheme.capitalize)::Transaction.new(uri)
    end

    #
    # Creates a Transaction bound to the given connection
    #
    # ==== Parameters
    # conn<DataObjects::Connection>:: The Connection to bind the new Transaction to
    #
    def initialize(uri)
      @connection = DataObjects::Connection.new(uri)
      @id = Digest::SHA256.hexdigest("#{HOST}:#{$$}:#{Time.now.to_f}:#{@@counter += 1}")
    end

    def close
      @connection.close
    end

    [:begin, :commit, :rollback, :rollback_prepared, :prepare].each do |method_name|

      eval <<EOF
def #{method_name}
  raise NotImplementedError
end
EOF

    end

  end
end
