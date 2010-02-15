require 'rack/mount/utils'
require 'forwardable'

module Rack::Mount
  module Generation
    module RouteSet
      # Adds generation related concerns to RouteSet.new.
      def initialize(*args)
        @named_routes = {}
        @generation_key_analyzer = Analysis::Frequency.new

        super
      end

      # Adds generation aspects to RouteSet#add_route.
      def add_route(*args)
        route = super
        @named_routes[route.name] = route if route.name
        @generation_key_analyzer << route.generation_keys
        route
      end

      # Generates path from identifiers or significant keys.
      #
      # To generate a url by named route, pass the name in as a +Symbol+.
      #   url(:dashboard) # => "/dashboard"
      #
      # Additional parameters can be passed in as a hash
      #   url(:people, :id => "1") # => "/people/1"
      #
      # If no name route is given, it will fall back to a slower
      # generation search.
      #   url(:controller => "people", :action => "show", :id => "1")
      #     # => "/people/1"
      def url(*args)
        named_route, params, recall, options = extract_params!(*args)

        options[:parameterize] ||= lambda { |name, param| Utils.escape_uri(param) }

        unless result = generate(:path_info, named_route, params, recall, options)
          return
        end

        uri, params = result
        params.each do |k, v|
          if v
            params[k] = v
          else
            params.delete(k)
          end
        end

        uri << "?#{Utils.build_nested_query(params)}" if uri && params.any?
        uri
      end

      def generate(method, *args) #:nodoc:
        raise 'route set not finalized' unless @generation_graph

        named_route, params, recall, options = extract_params!(*args)
        merged = recall.merge(params)
        route = nil

        if named_route
          if route = @named_routes[named_route.to_sym]
            recall = route.defaults.merge(recall)
            url = route.generate(method, params, recall, options)
            [url, params]
          else
            raise RoutingError, "#{named_route} failed to generate from #{params.inspect}"
          end
        else
          keys = @generation_keys.map { |key|
            if k = merged[key]
              k.to_s
            else
              nil
            end
          }
          @generation_graph[*keys].each do |r|
            next unless r.significant_params?
            if url = r.generate(method, params, recall, options)
              return [url, params]
            end
          end

          raise RoutingError, "No route matches #{params.inspect}"
        end
      end

      def rehash #:nodoc:
        @generation_keys  = build_generation_keys
        @generation_graph = build_generation_graph

        super
      end

      private
        def expire!
          @generation_keys = @generation_graph = nil
          super
        end

        def build_generation_graph
          build_nested_route_set(@generation_keys) { |k, i|
            throw :skip unless @routes[i].significant_params?

            if k = @generation_key_analyzer.possible_keys[i][k]
              k.to_s
            else
              nil
            end
          }
        end

        def build_generation_keys
          @generation_key_analyzer.report
        end

        def extract_params!(*args)
          case args.length
          when 4
            named_route, params, recall, options = args
          when 3
            if args[0].is_a?(Hash)
              params, recall, options = args
            else
              named_route, params, recall = args
            end
          when 2
            if args[0].is_a?(Hash)
              params, recall = args
            else
              named_route, params = args
            end
          when 1
            if args[0].is_a?(Hash)
              params = args[0]
            else
              named_route = args[0]
            end
          else
            raise ArgumentError
          end

          named_route ||= nil
          params  ||= {}
          recall  ||= {}
          options ||= {}

          [named_route, params.dup, recall.dup, options.dup]
        end
    end
  end
end
