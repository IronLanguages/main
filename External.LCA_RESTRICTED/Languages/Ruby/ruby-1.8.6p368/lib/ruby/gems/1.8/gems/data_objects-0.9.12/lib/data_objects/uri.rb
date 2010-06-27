gem 'addressable', '~>2.0'
require 'addressable/uri'

module DataObjects

  # A DataObjects URI is of the form scheme://user:password@host:port/path#fragment
  #
  # The elements are all optional except scheme and path:
  # scheme:: The name of a DBMS for which you have a do_\&lt;scheme\&gt; adapter gem installed. If scheme is *jdbc*, the actual DBMS is in the _path_ followed by a colon.
  # user:: The name of the user to authenticate to the database
  # password:: The password to use in authentication
  # host:: The domain name (defaulting to localhost) where the database is available
  # port:: The TCP/IP port number to use for the connection
  # path:: The name or path to the database
  # query:: Parameters for the connection, for example encoding=utf8
  # fragment:: Not currently known to be in use, but available to the adapters
  class URI < Struct.new(:scheme, :user, :password, :host, :port, :path, :query, :fragment)
    # Make a DataObjects::URI object by parsing a string. Simply delegates to Addressable::URI::parse.
    def self.parse(uri)
      return uri if uri.kind_of?(self)
      uri = Addressable::URI::parse(uri) unless uri.kind_of?(Addressable::URI)
      self.new(uri.scheme, uri.user, uri.password, uri.host, uri.port, uri.path, uri.query_values, uri.fragment)
    end

    # Display this URI object as a string
    def to_s
      string = ""
      string << "#{scheme}://"   if scheme
      if user
        string << "#{user}"
        string << ":#{password}" if password
        string << "@"
      end
      string << "#{host}"        if host
      string << ":#{port}"       if port
      string << path.to_s
      if query
        string << "?" << query.map do |key, value|
          "#{key}=#{value}"
        end.join("&")
      end
      string << "##{fragment}"   if fragment
      string
    end

    # Compare this URI to another for hashing
    def eql?(other)
      to_s.eql?(other.to_s)
    end

    # Hash this URI
    def hash
      to_s.hash
    end
  end
end
