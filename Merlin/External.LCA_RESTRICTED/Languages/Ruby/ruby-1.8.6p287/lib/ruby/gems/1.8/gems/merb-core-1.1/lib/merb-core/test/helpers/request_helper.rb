require "rack"

module Merb
  module Test
    module MakeRequest

      def request(uri, env = {})
        uri = url(uri) if uri.is_a?(Symbol)
        uri = URI(uri)
        uri.scheme ||= "http"
        uri.host   ||= "example.org"

        if (env[:method] == "POST" || env["REQUEST_METHOD"] == "POST")
          params = env.delete(:body_params) if env.key?(:body_params)
          params = env.delete(:params) if env.key?(:params) && !env.key?(:input)

          unless env.key?(:input)
            env[:input] = Merb::Parse.params_to_query_string(params)
            env["CONTENT_TYPE"] = "application/x-www-form-urlencoded"
          end
        end

        if env[:params]
          uri.query = [
            uri.query, Merb::Parse.params_to_query_string(env.delete(:params))
          ].compact.join("&")
        end
        
        ignore_cookies = env.has_key?(:jar) && env[:jar].nil?

        unless ignore_cookies
          # Setup a default cookie jar container
          @__cookie_jar__ ||= Merb::Test::CookieJar.new
          # Grab the cookie group name
          jar = env.delete(:jar) || :default
          # Add the cookies explicitly set by the user
          @__cookie_jar__.update(jar, uri, env.delete(:cookie)) if env.has_key?(:cookie)
          # Set the cookie header with the cookies
          env["HTTP_COOKIE"] = @__cookie_jar__.for(jar, uri)
        end
        
        app = Merb::Config[:app]
        rack = app.call(::Rack::MockRequest.env_for(uri.to_s, env))

        rack = Struct.new(:status, :headers, :body, :url, :original_env).
          new(rack[0], rack[1], rack[2], uri.to_s, env)
          
        @__cookie_jar__.update(jar, uri, rack.headers["Set-Cookie"]) unless ignore_cookies

        Merb::Dispatcher.work_queue.size.times do
          Merb::Dispatcher.work_queue.pop.call
        end

        rack
      end
    end
    
    module RequestHelper
      include MakeRequest

      def describe_request(rack)
        "a #{rack.original_env[:method] || rack.original_env["REQUEST_METHOD"] || "GET"} to '#{rack.url}'"
      end

      def describe_input(input)
        if input.respond_to?(:controller_name)
          "#{input.controller_name}##{input.action_name}"
        elsif input.respond_to?(:original_env)
          describe_request(input)
        else
          input
        end
      end
      
      def status_code(input)
        input.respond_to?(:status) ? input.status : input
      end
      
      def requesting(*args)   request(*args) end
      def response_for(*args) request(*args) end
    end
  end
end
