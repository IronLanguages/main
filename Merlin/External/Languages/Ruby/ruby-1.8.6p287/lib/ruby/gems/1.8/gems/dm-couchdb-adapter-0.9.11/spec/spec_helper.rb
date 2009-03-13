require 'pathname'
require Pathname(__FILE__).dirname.parent.expand_path + 'lib/couchdb_adapter'

COUCHDB_LOCATION = "couchdb://localhost:5984/test_cdb_adapter"

DataMapper.setup(
  :couch,
  Addressable::URI.parse(COUCHDB_LOCATION)
)

#drop/recreate db

@adapter = DataMapper::Repository.adapters[:couch]
begin
  @adapter.send(:http_delete, "/#{@adapter.escaped_db_name}")
  @adapter.send(:http_put, "/#{@adapter.escaped_db_name}")
  COUCHDB_AVAILABLE = true
rescue Errno::ECONNREFUSED
  warn "CouchDB could not be contacted at #{COUCHDB_LOCATION}, skipping online dm-couchdb-adapter specs"
  COUCHDB_AVAILABLE = false
end

begin
  gem 'dm-serializer'
  require 'dm-serializer'
  DMSERIAL_AVAILABLE = true
rescue LoadError
  DMSERIAL_AVAILABLE = false
end

if COUCHDB_AVAILABLE
  class Person
    include DataMapper::CouchResource
    def self.default_repository_name
      :couch
    end

    property :type, Discriminator
    property :name, String

    view(:by_name) {{ "map" => "function(doc) { if (#{couchdb_types_condition}) { emit(doc.name, doc); } }" }}
  end

  class Employee < Person
    property :rank, String
  end
end
