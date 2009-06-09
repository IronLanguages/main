module Merb
  module Rack
    
    class FastCGI
      # ==== Parameters
      # opts<Hash>:: Options for FastCGI (see below).
      #
      # ==== Options (opts)
      # :app<String>>:: The application name.
      #
      # :api: plugin
      def self.start(opts={})
        Merb.logger.warn!("Using FastCGI adapter")
        Merb::Server.change_privilege
        ::Rack::Handler::FastCGI.run(opts[:app], opts)
      end
    end
  end
end
