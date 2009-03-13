require 'webrick'
require 'webrick/utils'
require 'rack/handler/webrick'
module Merb
  module Rack

    class WEBrick < Merb::Rack::AbstractAdapter
      
      class << self
        # :api: private
        attr_accessor :server
      end

      # :api: plugin
      def self.new_server(port)
        options = {
          :Port        => port,
          :BindAddress => @opts[:host],
          :Logger      => Merb.logger,
          :AccessLog   => [
            [Merb.logger, ::WEBrick::AccessLog::COMMON_LOG_FORMAT],
            [Merb.logger, ::WEBrick::AccessLog::REFERER_LOG_FORMAT]
          ]
        }

        sockets = ::WEBrick::Utils.create_listeners nil, port
        @server = ::WEBrick::HTTPServer.new(options.merge(:DoNotListen => true))
        @server.listeners.replace sockets
      end

      # :api: plugin
      def self.start_server
        @server.mount("/", ::Rack::Handler::WEBrick, @opts[:app])
        @server.start
        exit(@status)
      end
      
      # :api: plugin
      def self.stop(status = 0)
        @status = status
        @server.shutdown
      end

    end
  end
end
