module Merb
  module Rack
    class PathPrefix < Merb::Rack::Middleware

      # :api: private
      def initialize(app, path_prefix = nil)
        super(app)
        @path_prefix = /^#{Regexp.escape(path_prefix)}/
      end
      
      # :api: plugin
      def deferred?(env)
        strip_path_prefix(env) 
        @app.deferred?(env)
      end
      
      # :api: plugin
      def call(env)
        strip_path_prefix(env) 
        @app.call(env)
      end

      # :api: private
      def strip_path_prefix(env)
        ['PATH_INFO', 'REQUEST_URI'].each do |path_key|
          if env[path_key] =~ @path_prefix
            env[path_key].sub!(@path_prefix, Merb::Const::EMPTY_STRING)
            env[path_key] = Merb::Const::SLASH if env[path_key].empty?
          end
        end
      end
      
    end
  end
end
