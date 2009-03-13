module Merb::Test::Rspec::RouteMatchers

  class RouteToMatcher

    # ==== Parameters
    # klass_or_name<Class, String>::
    #   The controller class or class name to match routes for.
    # action<~to_s>:: The name of the action to match routes for.
    def initialize(klass_or_name, action)
      @expected_controller = Class === klass_or_name ? klass_or_name.name : klass_or_name
      @expected_action = action.to_s
    end

    # ==== Parameters
    # target<Hash>:: The route parameters to match.
    #
    # ==== Returns
    # Boolean:: True if the controller action and parameters match.
    def matches?(target)
      @target_env = target.dup
      @target_controller, @target_action = @target_env.delete(:controller).to_s, @target_env.delete(:action).to_s

      @target_controller = "#{target.delete(:namespace)}::#{@target_controller}" if target.has_key?(:namespace)

      @expected_controller.snake_case == @target_controller.snake_case && @expected_action == @target_action && match_parameters(@target_env)
    end

    # ==== Parameters
    # target<Hash>:: The route parameters to match.
    #
    # ==== Returns
    # Boolean::
    #   True if the parameter matcher created with #with matches or if no
    #   parameter matcher exists.
    def match_parameters(target)
      @parameter_matcher.nil? ? true : @parameter_matcher.matches?(target)
    end

    # Creates a new paramter matcher.
    #
    # ==== Parameters
    # parameters<Hash, ~to_param>:: The parameters to match.
    #
    # ==== Returns
    # RouteToMatcher:: This matcher.
    #
    # ==== Alternatives
    # If parameters is an object, then a new expected hash will be constructed
    # with the key :id set to parameters.to_param.
    def with(parameters)
      @parameter_matcher = ParameterMatcher.new(parameters)

      self
    end

    # ==== Returns
    # String:: The failure message.
    def failure_message
      "expected the request to route to #{@expected_controller.to_const_string}##{@expected_action}#{expected_parameters_message}, but was #{@target_controller.to_const_string}##{@target_action}#{actual_parameters_message}"
    end

    # ==== Returns
    # String:: The failure message to be displayed in negative matches.
    def negative_failure_message
      "expected the request not to route to #{@expected_controller.camel_case}##{@expected_action}#{expected_parameters_message}, but it did"
    end

    def expected_parameters_message
      " with #{@parameter_matcher.expected.inspect}" if @parameter_matcher
    end

    def actual_parameters_message
      " with #{(@parameter_matcher.actual || {}).inspect}" if @parameter_matcher
    end
  end

  class ParameterMatcher
    attr_accessor :expected, :actual

    # ==== Parameters
    # hash_or_object<Hash, ~to_param>:: The parameters to match.
    #
    # ==== Alternatives
    # If hash_or_object is an object, then a new expected hash will be
    # constructed with the key :id set to hash_or_object.to_param.
    def initialize(hash_or_object)
      @expected = {}
      case hash_or_object
      when Hash then @expected = hash_or_object
      else @expected[:id] = hash_or_object.to_param
      end
    end

    # ==== Parameters
    # parameter_hash<Hash>:: The route parameters to match.
    #
    # ==== Returns
    # Boolean:: True if the route parameters match the expected ones.
    def matches?(parameter_hash)
      @actual = parameter_hash.dup.except(:controller, :action)

      return @actual.empty? if @expected.empty?
      @expected.all? {|(k, v)| @actual.has_key?(k) && @actual[k] == v}
    end

    # ==== Returns
    # String:: The failure message.
    def failure_message
      "expected the route to contain parameters #{@expected.inspect}, but instead contained #{@actual.inspect}"
    end

    # ==== Returns
    # String:: The failure message to be displayed in negative matches.
    def negative_failure_message
      "expected the route not to contain parameters #{@expected.inspect}, but it did"
    end
  end

  # Passes when the actual route parameters match the expected controller class
  # and controller action. Exposes a +with+ method for specifying parameters.
  #
  # ==== Parameters
  # klass_or_name<Class, String>::
  #   The controller class or class name to match routes for.
  # action<~to_s>:: The name of the action to match routes for.
  #
  # ==== Example
  #   # Passes if a GET request to "/" is routed to the Widgets controller's
  #   # index action.
  #   request_to("/", :get).should route_to(Widgets, :index)
  #
  #   # Use the 'with' method for parameter checks
  #   request_to("/123").should route_to(widgets, :show).with(:id => "123")
  def route_to(klass_or_name, action)
    RouteToMatcher.new(klass_or_name, action)
  end
end
