require "thin"

module Merb

  module Rack

    class Thin < Merb::Rack::AbstractAdapter
      # start a Thin server on given host and port.

      # :api: plugin
      def self.new_server(port)
        Merb::Dispatcher.use_mutex = false
        
        if (@opts[:socket] || @opts[:socket_file])
          socket = port.to_s
          socket_file = @opts[:socket_file] || "#{Merb.log_path}/#{Merb::Config[:name]}.%s.sock"
          socket_file = socket_file % port
          Merb.logger.warn!("Using Thin adapter with socket file #{socket_file}.")
          @server = ::Thin::Server.new(socket_file, @opts[:app], @opts)
        else
          Merb.logger.warn!("Using Thin adapter on host #{@opts[:host]} and port #{port}.")
          @opts[:host] = "#{@opts[:host]}-#{port}" if @opts[:host].include?('/')
          @server = ::Thin::Server.new(@opts[:host], port, @opts[:app], @opts)
        end
      end

      # :api: plugin
      def self.start_server
        ::Thin::Logging.silent = true
        @server.start
      end
      
      # :api: plugin
      def self.stop(status = 0)
        if @server
          @server.stop
          true
        end
      end
    end
  end
end
