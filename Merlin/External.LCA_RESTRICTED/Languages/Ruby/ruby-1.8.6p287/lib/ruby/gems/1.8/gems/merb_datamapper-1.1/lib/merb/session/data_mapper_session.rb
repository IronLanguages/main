require 'merb-core/dispatch/session'
require "dm-core"
module Merb
  class DataMapperSessionStore
    include ::DataMapper::Resource

    table_name = Merb::Plugins.config[:merb_datamapper][:session_storage_name] || 'sessions'
    storage_names[default_repository_name] = table_name

    property :session_id, String, :size => 32, :nullable => false, :key => true
    property :data, Object, :default => {}, :lazy => false
    property :created_at, DateTime, :default => Proc.new { |r, p| DateTime.now }

    ##
    # Retrieves a session from the session store
    #
    # @param session_id<String> The session_id to retrieve the session for
    #
    # @returns <nil, DataMapperSessionStore> The session corresponding to the id, or nil
    def self.retrieve_session(session_id)
      if session = get(session_id)
        session.data
      end
    end

    ##
    # Stores the data in a session with the given session_id, creating it if
    # required
    #
    # @param session_id<String> The session_id to find the session by, or the id of the new session
    # @param data<Object> The data to be stored in the session. Probably a hash
    def self.store_session(session_id, data)
      if session = get(session_id)
        session.update_attributes(:data => data)
      else
        create(:session_id => session_id, :data => data)
      end
    end

    ##
    # Deletes a session with the given id
    #
    # @param session_id<String> The session to destroy
    def self.delete_session(session_id)
      all(:session_id => session_id).destroy!
    end

    def self.default_repository_name
      Merb::Plugins.config[:merb_datamapper][:session_repository_name] || :default
    end
  end

  class DataMapperSession < SessionStoreContainer

    # The session store type
    self.session_store_type = :datamapper

    # The store object is the model class itself
    self.store = DataMapperSessionStore
  end
end
