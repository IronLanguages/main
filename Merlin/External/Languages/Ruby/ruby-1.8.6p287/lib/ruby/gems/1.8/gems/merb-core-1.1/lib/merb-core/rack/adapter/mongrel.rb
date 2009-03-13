begin
  require 'mongrel'
rescue LoadError => e
  Merb.fatal! "Mongrel is not installed, but you are trying to use it. " \
              "You need to either install mongrel or a different Ruby web " \
              "server, like thin."
end

require 'merb-core/rack/handler/mongrel'

module Merb

  module Rack

    class Mongrel < Merb::Rack::AbstractAdapter

      # :api: plugin
      def self.stop(status = 0)
        if @server
          begin
            @server.stop(true)
          rescue Mongrel::TimeoutError
            Merb.logger.fatal! "Your process took too long to shut " \
              "down, so mongrel killed it."
          end
          true
        end
      end

      # :api: plugin
      def self.new_server(port)
        @server = ::Mongrel::HttpServer.new(@opts[:host], port)
      end
      
      # :api: plugin
      def self.start_server
        @server.register('/', ::Merb::Rack::Handler::Mongrel.new(@opts[:app]))
        @server.run.join
      end
      
    end
    
  end
end
