require 'rack/mount/utils'

module Rack::Mount
  module Recognition
    module RouteSet
      attr_reader :parameters_key

      # Adds recognition related concerns to RouteSet.new.
      def initialize(options = {})
        @parameters_key = options.delete(:parameters_key) || 'rack.routing_args'
        @parameters_key.freeze
        @recognition_key_analyzer = Analysis::Frequency.new_with_module(Analysis::Splitting)

        super
      end

      # Adds recognition aspects to RouteSet#add_route.
      def add_route(*args)
        route = super
        @recognition_key_analyzer << route.conditions
        route
      end

      def recognize(obj)
        raise 'route set not finalized' unless @recognition_graph

        cache = {}
        keys = @recognition_keys.map { |key|
          if key.is_a?(Array)
            key.call(cache, obj)
          else
            obj.send(key)
          end
        }
        @recognition_graph[*keys].each do |route|
          if params = route.recognize(obj)
            if block_given?
              yield route, params
            else
              return route, params
            end
          end
        end

        nil
      end

      EXPECT    = 'Expect'.freeze
      PATH_INFO = 'PATH_INFO'.freeze

      # Rack compatible recognition and dispatching method. Routes are
      # tried until one returns a non-catch status code. If no routes
      # match, the catch status code is returned.
      #
      # This method can only be invoked after the RouteSet has been
      # finalized.
      def call(env)
        raise 'route set not finalized' unless @recognition_graph

        set_expectation = env[EXPECT] != '100-continue'
        env[EXPECT] = '100-continue' if set_expectation

        env[PATH_INFO] = Utils.normalize_path(env[PATH_INFO])

        req = @request_class.new(env)
        recognize(req) do |route, params|
          # TODO: We only want to unescape params from uri related methods
          params.each { |k, v| params[k] = Utils.unescape_uri(v) if v.is_a?(String) }

          env[@parameters_key] = params
          result = route.app.call(env)
          return result unless result[0].to_i == 417
        end

        set_expectation ? [404, {'Content-Type' => 'text/html'}, ['Not Found']] : [417, {'Content-Type' => 'text/html'}, ['Expectation failed']]
      ensure
        env.delete(EXPECT) if set_expectation
      end

      def rehash #:nodoc:
        @recognition_keys  = build_recognition_keys
        @recognition_graph = build_recognition_graph

        super
      end

      def valid_conditions #:nodoc:
        @valid_conditions ||= begin
          conditions = @request_class.instance_methods(false)
          conditions.map! { |m| m.to_sym }
          conditions.freeze
        end
      end

      private
        def expire!
          @recognition_keys = @recognition_graph = nil
          super
        end

        def build_recognition_graph
          build_nested_route_set(@recognition_keys) { |k, i|
            @recognition_key_analyzer.possible_keys[i][k]
          }
        end

        def build_recognition_keys
          @recognition_key_analyzer.report
        end
    end
  end
end
