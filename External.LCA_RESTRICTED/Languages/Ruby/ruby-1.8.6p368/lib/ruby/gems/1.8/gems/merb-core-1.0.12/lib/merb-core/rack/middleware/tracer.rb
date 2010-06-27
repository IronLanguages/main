module Merb
  module Rack
    class Tracer < Merb::Rack::Middleware

      # :api: plugin
      def call(env)

        Merb.logger.debug!("Rack environment:\n" + env.inspect + "\n\n")

        status, headers, body = @app.call(env)

        Merb.logger.debug!("Status: #{status.inspect}")
        Merb.logger.debug!("Headers: #{headers.inspect}")
        Merb.logger.debug!("Body: #{body.inspect}")

        [status, headers, body]
      end
      
    end
  end
end
