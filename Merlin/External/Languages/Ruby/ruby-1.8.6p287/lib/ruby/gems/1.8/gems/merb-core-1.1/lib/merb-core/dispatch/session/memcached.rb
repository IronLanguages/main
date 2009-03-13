module Merb

  # Sessions stored in memcached.
  #
  # Requires setup in your +init.rb+.
  # 
  # This for the 'memcache-client' gem:
  #
  #   Merb::BootLoader.after_app_loads do
  #     require 'memcache'
  #     Merb::MemcacheSession.store = 
  #        MemCache.new('127.0.0.1:11211', :namespace => 'my_app')
  #   end
  #
  # Or this for the 'memcached' gem:
  #
  #   Merb::BootLoader.after_app_loads do
  #     require 'memcache'
  #     Merb::MemcacheSession.store = 
  #        Memcached.new('127.0.0.1:11211', :namespace => 'my_app')
  #   end
  
  class MemcacheSession < SessionStoreContainer
    
    # The session store type
    self.session_store_type = :memcache
    
  end
  
  module MemcacheStore
    
    # Make the Memcached gem conform to the SessionStoreContainer interface
    
    # ==== Parameters
    # session_id<String>:: ID of the session to retrieve.
    #
    # ==== Returns
    # ContainerSession:: The session corresponding to the ID.
    # 
    # :api: private
    def retrieve_session(session_id)
      get("session:#{session_id}")
    end
    
    # ==== Parameters
    # session_id<String>:: ID of the session to set.
    # data<ContainerSession>:: The session to set.
    # 
    # :api: private
    def store_session(session_id, data)
      set("session:#{session_id}", data)
    end
    
    # ==== Parameters
    # session_id<String>:: ID of the session to delete.
    #
    # :api: private
    def delete_session(session_id)
      delete("session:#{session_id}")
    end
    
  end
  
end

# For the memcached gem.
class Memcached
  include Merb::MemcacheStore
end

# For the memcache-client gem.
class MemCache
  include Merb::MemcacheStore
end
