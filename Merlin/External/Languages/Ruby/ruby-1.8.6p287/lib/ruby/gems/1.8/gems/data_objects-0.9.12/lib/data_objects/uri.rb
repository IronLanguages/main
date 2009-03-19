gem 'addressable', '~>2.0'
require 'addressable/uri'

module DataObjects
  URI = Struct.new(:scheme, :user, :password, :host, :port, :path, :query, :fragment)

  class URI
    def self.parse(uri)
      return uri if uri.kind_of?(self)
      uri = Addressable::URI::parse(uri) unless uri.kind_of?(Addressable::URI)
      self.new(uri.scheme, uri.user, uri.password, uri.host, uri.port, uri.path, uri.query_values, uri.fragment)
    end

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

    def eql?(other)
      to_s.eql?(other.to_s)
    end

    def hash
      to_s.hash
    end
  end
end
