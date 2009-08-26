module Merb
  
  module Rack
    
    class Adapter

      class << self
        # Get a rack adapter by id. 
        # ==== Parameters
        # id<String>:: The identifier of the Rack adapter class to retrieve.
        #
        # ==== Returns.
        # Class:: The adapter class.
        #
        # :api: private
        def get(id)
          if @adapters[id.to_s]
            Object.full_const_get(@adapters[id.to_s])
          else
            Merb.fatal! "The adapter #{id} did not exist"
          end
        end

        # Registers a new Rack adapter.
        #
        # ==== Parameters
        # ids<Array>:: Identifiers by which this adapter is recognized by.
        # adapter_class<Class>:: The Rack adapter class.
        #
        # :api: plugin
        def register(ids, adapter_class)
          @adapters ||= Hash.new
          ids.each { |id| @adapters[id] = "Merb::Rack::#{adapter_class}" }
        end
      end # class << self
      
    end # Adapter
    
    # Register some Rack adapters
    Adapter.register %w{ebb},            :Ebb
    Adapter.register %w{emongrel},       :EventedMongrel
    Adapter.register %w{fastcgi fcgi},   :FastCGI
    Adapter.register %w{irb},            :Irb
    Adapter.register %w{mongrel},        :Mongrel  
    Adapter.register %w{runner},         :Runner
    Adapter.register %w{smongrel swift}, :SwiftipliedMongrel
    Adapter.register %w{thin},           :Thin
    Adapter.register %w{thin-turbo tt},  :ThinTurbo
    Adapter.register %w{webrick},        :WEBrick
    
  end # Rack
end # Merb
