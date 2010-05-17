module Merb
  module Rack

    class ContentLength < Merb::Rack::Middleware

      # :api: plugin
      def call(env)
        status, headers, body = @app.call(env)

        # to_s is because Rack spec expects header
        # values to be iterable and yield strings
        header = 'Content-Length'.freeze
        headers[header] = body.size.to_s unless headers.has_key?(header)

        [status, headers, body]
      end
    end
    
  end
end
