module Merb
  module Rack
    class Middleware
      
      # @overridable
      # :api: plugin
      def initialize(app)
        @app = app
      end

      # @overridable
      # :api: plugin
      def deferred?(env)
        @app.deferred?(env) if @app.respond_to?(:deferred?)
      end
  
      # @overridable
      # :api: plugin  
      def call(env)
        @app.call(env)
      end
      
    end
  end
end

