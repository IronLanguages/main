class Merb::Controller < Merb::AbstractController

  class_inheritable_accessor :_hidden_actions, :_shown_actions, 
                             :_overridable, :_override_bang

  self._hidden_actions ||= []
  self._shown_actions  ||= []
  self._overridable    ||= []
  self._override_bang  ||= []

  cattr_accessor :_subclasses
  self._subclasses = Set.new

  # :api: private
  def self.subclasses_list() _subclasses end

  include Merb::ResponderMixin
  include Merb::ControllerMixin
  include Merb::AuthenticationMixin
  include Merb::ConditionalGetMixin

  # ==== Parameters
  # klass<Merb::Controller>::
  #   The Merb::Controller inheriting from the base class.
  #
  # :api: private
  def self.inherited(klass)
    _subclasses << klass.to_s
    super
    klass._template_root = Merb.dir_for(:view) unless self._template_root
  end

  # ==== Parameters
  # *names<Array[Symbol]>::
  #   an Array of method names that should be overridable in application
  #   controllers.
  # 
  # ==== Returns
  # Array:: The list of methods that are overridable
  #
  # :api: plugin
  def self.overridable(*names)
    self._overridable.push(*names)
  end
  
  # In an application controller, call override! before a method to indicate
  # that you want to override a method in Merb::Controller that is not
  # normally overridable.
  #
  # Doing this may potentially break your app in a future release of Merb,
  # and this is provided for users who are willing to take that risk.
  # Without using override!, Merb will raise an error if you attempt to
  # override a method defined on Merb::Controller.
  #
  # This is to help users avoid a common mistake of defining an action
  # that overrides a core method on Merb::Controller.
  #
  # ==== Parameters
  # *names<Array[Symbol]>:: 
  #   An Array of methods that will override Merb core classes on purpose
  #
  # ==== Example
  #     
  #     class Kontroller < Application
  #       def status
  #         render
  #       end
  #     end
  # 
  # will raise a Merb::ReservedError, because #status is a method on
  # Merb::Controller.
  # 
  #     class Kontroller < Application
  #       override! :status
  #       def status
  #         some_code || super
  #       end
  #     end
  #
  # will not raise a Merb::ReservedError, because the user specifically
  # decided to override the status method.
  #
  # :api: public
  def self.override!(*names)
    self._override_bang.push(*names)
  end

  # Hide each of the given methods from being callable as actions.
  #
  # ==== Parameters
  # *names<~to-s>:: Actions that should be added to the list.
  #
  # ==== Returns
  # Array[String]::
  #   An array of actions that should not be possible to dispatch to.
  #
  # :api: public
  def self.hide_action(*names)
    self._hidden_actions = self._hidden_actions | names.map { |n| n.to_s }
  end

  # Makes each of the given methods being callable as actions. You can use
  # this to make methods included from modules callable as actions.
  #
  # ==== Parameters
  # *names<~to-s>:: Actions that should be added to the list.
  #
  # ==== Returns
  # Array[String]::
  #   An array of actions that should be dispatched to even if they would not
  #   otherwise be.
  #
  # ==== Example
  #   module Foo
  #     def self.included(base)
  #       base.show_action(:foo)
  #     end
  #
  #     def foo
  #       # some actiony stuff
  #     end
  #
  #     def foo_helper
  #       # this should not be an action
  #     end
  #   end
  #
  # :api: public
  def self.show_action(*names)
    self._shown_actions = self._shown_actions | names.map {|n| n.to_s}
  end

  # The list of actions that are callable, after taking defaults,
  # _hidden_actions and _shown_actions into consideration. It is calculated
  # once, the first time an action is dispatched for this controller.
  #
  # ==== Returns
  # SimpleSet[String]:: A set of actions that should be callable.
  #
  # :api: public
  def self.callable_actions
    @callable_actions ||= Extlib::SimpleSet.new(_callable_methods)
  end

  # This is a stub method so plugins can implement param filtering if they want.
  #
  # ==== Parameters
  # params<Hash{Symbol => String}>:: A list of params
  #
  # ==== Returns
  # Hash{Symbol => String}:: A new list of params, filtered as desired
  # 
  # :api: plugin
  # @overridable
  def self._filter_params(params)
    params
  end
  overridable :_filter_params

  # All methods that are callable as actions.
  #
  # ==== Returns
  # Array:: A list of method names that are also actions
  #
  # :api: private
  def self._callable_methods
    callables = []
    klass = self
    begin
      callables << (klass.public_instance_methods(false) + klass._shown_actions) - klass._hidden_actions
      klass = klass.superclass
    end until klass == Merb::AbstractController || klass == Object
    callables.flatten.reject{|action| action =~ /^_.*/}.map {|x| x.to_s}
  end

  # The location to look for a template for a particular controller, context,
  # and mime-type. This is overridden from AbstractController, which defines a
  # version of this that does not involve mime-types.
  #
  # ==== Parameters
  # context<~to_s>:: The name of the action or template basename that will be rendered.
  # type<~to_s>::
  #    The mime-type of the template that will be rendered. Defaults to nil.
  # controller<~to_s>::
  #   The name of the controller that will be rendered. Defaults to
  #   controller_name.  This will be "layout" for rendering a layout.  
  #
  # ==== Notes
  # By default, this renders ":controller/:action.:type". To change this,
  # override it in your application class or in individual controllers.
  #
  # :api: public
  # @overridable
  def _template_location(context, type, controller)
    _conditionally_append_extension(controller ? "#{controller}/#{context}" : "#{context}", type)
  end
  overridable :_template_location

  # The location to look for a template and mime-type. This is overridden
  # from AbstractController, which defines a version of this that does not
  # involve mime-types.
  #
  # ==== Parameters
  # template<String>::
  #    The absolute path to a template - without mime and template extension.
  #    The mime-type extension is optional - it will be appended from the
  #    current content type if it hasn't been added already.
  # type<~to_s>::
  #    The mime-type of the template that will be rendered. Defaults to nil.
  #
  # :api: public
  def _absolute_template_location(template, type)
    _conditionally_append_extension(template, type)
  end

  # Build a new controller.
  #
  # Sets the variables that came in through the dispatch as available to
  # the controller.
  #
  # ==== Parameters
  # request<Merb::Request>:: The Merb::Request that came in from Rack.
  # status<Integer>:: An integer code for the status. Defaults to 200.
  # headers<Hash{header => value}>::
  #   A hash of headers to start the controller with. These headers can be
  #   overridden later by the #headers method.
  # 
  # :api: plugin
  # @overridable
  def initialize(request, status=200, headers={'Content-Type' => 'text/html; charset=utf-8'})
    super()
    @request, @_status, @headers = request, status, headers
  end
  overridable :initialize

  # Dispatch the action.
  #
  # ==== Parameters
  # action<~to_s>:: An action to dispatch to. Defaults to :index.
  #
  # ==== Returns
  # String:: The string sent to the logger for time spent.
  #
  # ==== Raises
  # ActionNotFound:: The requested action was not found in class.
  #
  # :api: plugin
  def _dispatch(action=:index)
    Merb.logger.info { "Params: #{self.class._filter_params(request.params).inspect}" }
    start = Time.now
    if self.class.callable_actions.include?(action.to_s)
      super(action)
    else
      raise ActionNotFound, "Action '#{action}' was not found in #{self.class}"
    end
    @_benchmarks[:action_time] = Time.now - start
    self
  end

  # :api: public
  attr_reader :request, :headers

  # ==== Returns
  # Fixnum:: The response status code
  #
  # :api: public
  def status
    @_status
  end

  # Set the response status code.
  #
  # ==== Parameters
  # s<Fixnum, Symbol>:: A status-code or named http-status
  #
  # :api: public
  def status=(s)
    if s.is_a?(Symbol) && STATUS_CODES.key?(s)
      @_status = STATUS_CODES[s]
    elsif s.is_a?(Fixnum)
      @_status = s
    else
      raise ArgumentError, "Status should be of type Fixnum or Symbol, was #{s.class}"
    end
  end

  # ==== Returns
  # Hash:: The parameters from the request object
  # 
  # :api: public
  def params()  request.params  end
    
  # There are three possible ways to use this method.  First, if you have a named route, 
  # you can specify the route as the first parameter as a symbol and any paramters in a 
  # hash.  Second, you can generate the default route by just passing the params hash, 
  # just passing the params hash.  Finally, you can use the anonymous parameters.  This 
  # allows you to specify the parameters to a named route in the order they appear in the 
  # router.  
  #
  # ==== Parameters(Named Route)
  # name<Symbol>:: 
  #   The name of the route. 
  # args<Hash>:: 
  #   Parameters for the route generation.
  #
  # ==== Parameters(Default Route)
  # args<Hash>:: 
  #   Parameters for the route generation.  This route will use the default route. 
  #
  # ==== Parameters(Anonymous Parameters)
  # name<Symbol>::
  #   The name of the route.  
  # args<Array>:: 
  #   An array of anonymous parameters to generate the route
  #   with. These parameters are assigned to the route parameters
  #   in the order that they are passed.
  #
  # ==== Returns
  # String:: The generated URL.
  #
  # ==== Examples
  # Named Route
  #
  # Merb::Router.prepare do
  #   match("/articles/:title").to(:controller => :articles, :action => :show).name("articles")
  # end
  #
  # url(:articles, :title => "new_article")
  #
  # Default Route
  #
  # Merb::Router.prepare do
  #   default_routes
  # end
  #
  # url(:controller => "articles", :action => "new")
  #
  # Anonymous Paramters
  #
  # Merb::Router.prepare do
  #   match("/articles/:year/:month/:title").to(:controller => :articles, :action => :show).name("articles")
  # end
  #
  # url(:articles, 2008, 10, "test_article")
  #
  # :api: public
  def url(name, *args)
    args << params
    name = request.route if name == :this
    Merb::Router.url(name, *args)
  end
  
  # Generates a URL for a single or nested resource.
  #
  # ==== Parameters
  # resources<Symbol,Object>:: The resources for which the URL
  #   should be generated. These resources should be specified
  #   in the router.rb file using #resources and #resource.
  #
  # options<Hash>:: Any extra parameters that are needed to
  #   generate the URL.
  #
  # ==== Returns
  # String:: The generated URL.
  #
  # ==== Examples
  #
  # Merb::Router.prepare do
  #   resources :users do
  #     resources :comments
  #   end
  # end
  #
  # resource(:users)            # => /users
  # resource(@user)             # => /users/10
  # resource(@user, :comments)  # => /users/10/comments
  # resource(@user, @comment)   # => /users/10/comments/15
  # resource(:users, :new)      # => /users/new
  # resource(:@user, :edit)     # => /users/10/edit
  #
  # :api: public
  def resource(*args)
    args << params
    Merb::Router.resource(*args)
  end
  

  alias_method :relative_url, :url
  
  # Returns the absolute url including the passed protocol and host.  
  # 
  # This uses the same arguments as the url method, with added requirements 
  # of protocol and host options. 
  #
  # :api: public
  def absolute_url(*args)
    options  = extract_options_from_args!(args) || {}
    options[:protocol] ||= request.protocol
    options[:host] ||= request.host
    args << options
    super(*args)
  end

  # The results of the controller's render, to be returned to Rack.
  #
  # ==== Returns
  # Array[Integer, Hash, String]::
  #   The controller's status code, headers, and body
  #
  # :api: private
  def rack_response
    [status, headers, Merb::Rack::StreamWrapper.new(body)]
  end

  # Sets a controller to be "abstract" 
  # This controller will not be able to be routed to
  # and is used for super classing only
  #
  # :api: public
  def self.abstract!
    @_abstract = true
  end
  
  # Asks a controller if it is abstract
  #
  # === Returns
  # Boolean
  #  true if the controller has been set as abstract
  #
  # :api: public
  def self.abstract?
    !!@_abstract 
  end

  # Hide any methods that may have been exposed as actions before.
  hide_action(*_callable_methods)

  private

  # If not already added, add the proper mime extension to the template path.
  #
  # ==== Parameters
  #
  # template<~to_s> ::
  #   The template path to append the mime type to.
  # type<~to_s> ::
  #   The extension to append to the template path conditionally
  #
  # :api: private
  def _conditionally_append_extension(template, type)
    type && !template.match(/\.#{type.to_s.escape_regexp}$/) ? "#{template}.#{type}" : template
  end
  
  # When a method is added to a subclass of Merb::Controller (i.e. an app controller) that
  # is defined on Merb::Controller, raise a Merb::ReservedError. An error will not be raised
  # if the method is defined as overridable in the Merb API.
  #
  # This behavior can be overridden by using override! method_name before attempting to
  # override the method.
  #
  # ==== Parameters
  # meth<~to_sym> The method that is being added
  #
  # ==== Raises
  # Merb::ReservedError::
  #   If the method being added is in a subclass of Merb::Controller,
  #   the method is defined on Merb::Controller, it is not defined
  #   as overridable in the Merb API, and the user has not specified
  #   that it can be overridden.
  #
  # ==== Returns
  # nil
  # 
  # :api: private
  def self.method_added(meth)
    if self < Merb::Controller && Merb::Controller.method_defined?(meth) && 
      !self._overridable.include?(meth.to_sym) && !self._override_bang.include?(meth.to_sym)

      raise Merb::ReservedError, "You tried to define #{meth} on " \
        "#{self.name} but it was already defined on Merb::Controller. " \
        "If you meant to override a core method, use override!"
    end
  end
end
