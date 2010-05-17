module Merb
  module Rack

    class ConditionalGet < Merb::Rack::Middleware

      # :api: plugin
      def call(env)
        status, headers, body = @app.call(env)

        if document_not_modified?(env, headers)
          status = 304
          body = Merb::Const::EMPTY_STRING
          # set Date header using RFC1123 date format as specified by HTTP
          # RFC2616 section 3.3.1.
        end
        
        [status, headers, body]
      end
    
    private
      # :api: private
      def document_not_modified?(env, headers)
        if etag = headers[Merb::Const::ETAG]
          etag == env[Merb::Const::HTTP_IF_NONE_MATCH]
        elsif last_modified = headers[Merb::Const::LAST_MODIFIED]
          last_modified == env[Merb::Const::HTTP_IF_MODIFIED_SINCE]
        end
      end
    end
    
  end
end
