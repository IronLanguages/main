require 'rack/mount/multimap'
require 'rack/mount/route'
require 'rack/mount/utils'

module Rack::Mount
  class RouteSet
    extend Mixover

    # Include generation and recognition concerns
    include Generation::RouteSet, Recognition::RouteSet
    include Recognition::CodeGeneration

    # Initialize a new RouteSet without optimizations
    def self.new_without_optimizations(*args, &block)
      new_without_module(Recognition::CodeGeneration, *args, &block)
    end

    # Basic RouteSet initializer.
    #
    # If a block is given, the set is yielded and finalized.
    #
    # See other aspects for other valid options:
    # - <tt>Generation::RouteSet.new</tt>
    # - <tt>Recognition::RouteSet.new</tt>
    def initialize(options = {}, &block)
      @request_class = options.delete(:request_class) || Rack::Request
      @routes = []
      expire!

      if block_given?
        yield self
        rehash
      end
    end

    # Builder method to add a route to the set
    #
    # <tt>app</tt>:: A valid Rack app to call if the conditions are met.
    # <tt>conditions</tt>:: A hash of conditions to match against.
    #                       Conditions may be expressed as strings or
    #                       regexps to match against.
    # <tt>defaults</tt>:: A hash of values that always gets merged in
    # <tt>name</tt>:: Symbol identifier for the route used with named
    #                 route generations
    def add_route(app, conditions = {}, defaults = {}, name = nil)
      route = Route.new(self, app, conditions, defaults, name)
      @routes << route
      expire!
      route
    end

    # See <tt>Recognition::RouteSet#call</tt>
    def call(env)
      raise NotImplementedError
    end

    # See <tt>Generation::RouteSet#url</tt>
    def url(*args)
      raise NotImplementedError
    end

    # See <tt>Generation::RouteSet#generate</tt>
    def generate(*args)
      raise NotImplementedError
    end

    # Number of routes in the set
    def length
      @routes.length
    end

    def rehash #:nodoc:
    end

    # Finalizes the set and builds optimized data structures. You *must*
    # freeze the set before you can use <tt>call</tt> and <tt>url</tt>.
    # So remember to call freeze after you are done adding routes.
    def freeze
      unless frozen?
        rehash
        @routes.each { |route| route.freeze }
        @routes.freeze
      end

      super
    end

    private
      def expire! #:nodoc:
      end

      # An internal helper method for constructing a nested set from
      # the linear route set.
      #
      # build_nested_route_set([:request_method, :path_info]) { |route, method|
      #   route.send(method)
      # }
      def build_nested_route_set(keys, &block)
        graph = Multimap.new
        @routes.each_with_index do |route, index|
          catch(:skip) do
            k = keys.map { |key| block.call(key, index) }
            Utils.pop_trailing_nils!(k)
            k.map! { |key| key || /.+/ }
            graph[*k] = route
          end
        end
        graph
      end
  end
end
