module Merb
  module Rack
    class Static < Merb::Rack::Middleware

      # :api: private
      def initialize(app,directory)
        super(app)
        @static_server = ::Rack::File.new(directory)
      end
      
      # :api: plugin
      def call(env)        
        path = if env[Merb::Const::PATH_INFO]
                 env[Merb::Const::PATH_INFO].chomp(Merb::Const::SLASH)
               else
                 Merb::Const::EMPTY_STRING
               end
        cached_path = (path.empty? ? 'index' : path) + '.html'
        
        if file_exist?(path) && env[Merb::Const::REQUEST_METHOD] =~ /GET|HEAD/ # Serve the file if it's there and the request method is GET or HEAD
          serve_static(env)
        elsif file_exist?(cached_path) && env[Merb::Const::REQUEST_METHOD] =~ /GET|HEAD/ # Serve the page cache if it's there and the request method is GET or HEAD
          env[Merb::Const::PATH_INFO] = cached_path
          serve_static(env)
        elsif path =~ /favicon\.ico/
          return [404, { Merb::Const::CONTENT_TYPE => Merb::Const::TEXT_SLASH_HTML }, "404 Not Found."]
        else
          @app.call(env)
        end
      end
      
        # ==== Parameters
        # path<String>:: The path to the file relative to the server root.
        #
        # ==== Returns
        # Boolean:: True if file exists under the server root and is readable.
        #
        # :api: private
        def file_exist?(path)
          full_path = ::File.join(@static_server.root, ::Merb::Parse.unescape(path))
          ::File.file?(full_path) && ::File.readable?(full_path)
        end

        # ==== Parameters
        # env<Hash>:: Environment variables to pass on to the server.
        #
        # :api: private
        def serve_static(env)
          env[Merb::Const::PATH_INFO] = ::Merb::Parse.unescape(env[Merb::Const::PATH_INFO])
          @static_server.call(env)
        end
      
    end
  end
end
