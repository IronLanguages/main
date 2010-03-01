require 'rack/mount/generatable_regexp'
require 'rack/mount/regexp_with_named_groups'
require 'rack/mount/utils'

module Rack::Mount
  # Route is an internal class used to wrap a single route attributes.
  #
  # Plugins should not depend on any method on this class or instantiate
  # new Route objects. Instead use the factory method, RouteSet#add_route
  # to create new routes and add them to the set.
  class Route
    extend Mixover

    # Include generation and recognition concerns
    include Generation::Route, Recognition::Route

    # Valid rack application to call if conditions are met
    attr_reader :app

    # A hash of conditions to match against. Conditions may be expressed
    # as strings or regexps to match against.
    attr_reader :conditions

    # A hash of values that always gets merged into the parameters hash
    attr_reader :defaults

    # Symbol identifier for the route used with named route generations
    attr_reader :name

    def initialize(set, app, conditions, defaults, name)
      @set = set

      unless app.respond_to?(:call)
        raise ArgumentError, 'app must be a valid rack application' \
          ' and respond to call'
      end
      @app = app

      @name = name ? name.to_sym : nil
      @defaults = (defaults || {}).freeze

      unless conditions.is_a?(Hash)
        raise ArgumentError, 'conditions must be a Hash'
      end
      @conditions = {}

      conditions.each do |method, pattern|
        next unless method && pattern

        unless @set.valid_conditions.include?(method)
          raise ArgumentError, 'conditions may only include ' +
            @set.valid_conditions.inspect
        end

        pattern = Regexp.compile("\\A#{Regexp.escape(pattern)}\\Z") if pattern.is_a?(String)
        pattern = Utils.normalize_extended_expression(pattern)

        pattern = RegexpWithNamedGroups.new(pattern)
        pattern.extend(GeneratableRegexp::InstanceMethods)
        pattern.defaults = @defaults

        @conditions[method] = pattern.freeze
      end

      @conditions.freeze
    end

    def inspect #:nodoc:
      "#<#{self.class.name} @app=#{@app.inspect} @conditions=#{@conditions.inspect} @defaults=#{@defaults.inspect} @name=#{@name.inspect}>"
    end
  end
end
