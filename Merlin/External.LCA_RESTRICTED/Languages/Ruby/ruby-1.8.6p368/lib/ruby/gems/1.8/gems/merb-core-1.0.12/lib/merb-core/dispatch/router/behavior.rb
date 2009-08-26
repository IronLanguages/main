module Merb
  
  class Router
    
    class Behavior
      
      class Error < StandardError; end
      
      # Proxy catches any methods and proxies them to the current behavior.
      # This allows building routes without constantly having to catching the
      # yielded behavior object
      # 
      # :api: private
      class Proxy
        
        # Undefine as many methods as possible so that everything can be proxied
        # along to the behavior
        instance_methods.each { |m| undef_method m unless %w[ __id__ __send__ class kind_of? respond_to? assert_kind_of should should_not instance_variable_set instance_variable_get instance_eval].include?(m) }
        
        # :api: private
        def initialize
          @behaviors = []
        end
        
        # Puts a behavior on the bottom of the stack.
        # 
        # ==== Notes
        # The behaviors keep track of nested scopes.
        # 
        # :api: private
        def push(behavior)
          @behaviors.push(behavior)
        end
        
        # Removes the top-most behavior.
        # 
        # ==== Notes
        # This occurs at the end of a nested scope (namespace, etc).
        # 
        # :api: private
        def pop
          @behaviors.pop
        end
        
        # Tests whether the top-most behavior responds to the arguments.
        # 
        # ==== Notes
        # Behaviors contain the actual functionality of the proxy.
        # 
        # :api: private
        def respond_to?(*args)
          super || @behaviors.last.respond_to?(*args)
        end
        
        # Rake does some stuff with methods in the global namespace, so if I don't
        # explicitly define the Behavior methods to proxy here (specifically namespace)
        # Rake's methods take precedence.
        # 
        # Removing the following:
        # name full_name fixatable redirect
        %w(
          match to with register default defaults options option namespace identify
          default_routes defer defer_to capture resources resource
        ).each do |method|
          class_eval %{
            def #{method}(*args, &block)
              @behaviors.last.#{method}(*args, &block)
            end
          }
        end
        
        # == These methods are to be used in defer_to blocks
        
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
          args << {}
          Merb::Router.url(name, *args)
        end
        
        # Generates a Rack redirection response.
        # 
        # ==== Notes
        # Refer to Merb::Rack::Helpers.redirect for documentation.
        # 
        # :api: public
        def redirect(url, opts = {})
          Merb::Rack::Helpers.redirect(url, opts)
        end
        
        private
        
        # Proxies the method calls to the behavior.
        # 
        # ==== Notes
        # Please refer to:
        # http://ruby-doc.org/core/classes/Kernel.html#M005951
        # 
        # :api: private
        def method_missing(method, *args, &block)
          behavior = @behaviors.last
          
          if behavior.respond_to?(method)
            behavior.send(method, *args, &block)
          else
            super
          end
        end
      end
      
      # Behavior objects are used for the Route building DSL. Each object keeps
      # track of the current definitions for the level at which it is defined.
      # Each time a method is called on a Behavior object that accepts a block,
      # a new instance of the Behavior class is created.
      # 
      # ==== Parameters
      # 
      # proxy<Proxy>::
      #   This is the object initialized by Merb::Router.prepare that tracks the
      #   current Behavior object stack so that Behavior methods can be called
      #   without explicitly calling them on an instance of Behavior.
      # conditions<Hash>::
      #   The initial route conditions. See #match.
      # params<Hash>::
      #   The initial route parameters. See #to.
      # defaults<Hash>::
      #   The initial route default parameters. See #defaults.
      # options<Hash>::
      #   The initial route options. See #options.
      # blocks<Array>::
      #   The stack of deferred routing blocks for the route
      # 
      # ==== Returns
      # Behavior:: The initialized Behavior object
      # 
      # :api: private
      def initialize(proxy = nil, conditions = {}, params = {}, defaults = {}, identifiers = {}, options = {}, blocks = []) #:nodoc:
        @proxy       = proxy
        @conditions  = conditions
        @params      = params
        @defaults    = defaults
        @identifiers = identifiers
        @options     = options
        @blocks      = blocks
        
        stringify_condition_values
      end
      
      # Defines the +conditions+ that are required to match a Request. Each
      # +condition+ is applied to a method of the Request object. Conditions
      # can also be applied to segments of the +path+.
      # 
      # If #match is passed a block, it will create a new route scope with
      # the conditions passed to it and yield to the block such that all
      # routes that are defined in the block have the conditions applied
      # to them.
      # 
      # ==== Parameters
      # 
      # path<String, Regexp>::
      #   The pattern against which Merb::Request path is matched.
      # 
      #   When +path+ is a String, any substring that is wrapped in parenthesis
      #   is considered optional and any segment that begins with a colon, ex.:
      #   ":login", defines both a capture and a named param. Extra conditions
      #   can then be applied each named param individually.
      # 
      #   When +path+ is a Regexp, the pattern is left untouched and the
      #   Merb::Request path is matched against it as is.
      #
      #   +path+ is optional.
      # 
      # conditions<Hash>::
      #   Additional conditions that the request must meet in order to match.
      #   The keys must be the names of previously defined path segments or
      #   be methods that the Merb::Request instance will respond to.  The
      #   value is the string or regexp that matched the returned value.
      #   Conditions are inherited by child routes.
      # 
      # &block::
      #   All routes defined in the block will be scoped to the conditions
      #   defined by the #match method.
      # 
      # ==== Block parameters
      # r<Behavior>:: +optional+ - The match behavior object.
      # 
      # ==== Returns
      # Behavior::
      #   A new instance of Behavior with the specified path and conditions.
      # 
      # +Tip+: When nesting always make sure the most inner sub-match registers
      # a Route and doesn't just return new Behaviors.
      # 
      # ==== Examples
      # 
      #   # registers /foo/bar to controller => "foo", :action => "bar"
      #   # and /foo/baz to controller => "foo", :action => "baz"
      #   match("/foo") do
      #     match("/bar").to(:controller => "foo", :action => "bar")
      #     match("/baz").to(:controller => "foo", :action => "caz")
      #   end
      # 
      #   # Checks the format of the segments against the specified Regexp
      #   match("/:string/:number", :string => /[a-z]+/, :number => /\d+/).
      #     to(:controller => "string_or_numbers")
      # 
      #   # Equivalent to the default_route
      #   match("/:controller(/:action(:id))(.:format)").register
      # 
      #   #match only if the browser string contains MSIE or Gecko
      #   match("/foo", :user_agent => /(MSIE|Gecko)/ )
      #        .to(:controller => 'foo', :action => 'popular')
      # 
      #   # Route GET and POST requests to different actions (see also #resources)
      #   r.match('/foo', :method => :get).to(:action => 'show')
      #   r.match('/foo', :method => :post).to(:action => 'create')
      # 
      #   # match also takes regular expressions
      # 
      #   r.match(%r[/account/([a-z]{4,6})]).to(:controller => "account",
      #      :action => "show", :id => "[1]")
      # 
      #   r.match(%r{/?(en|es|fr|be|nl)?}).to(:language => "[1]") do
      #     match("/guides/:action/:id").to(:controller => "tour_guides")
      #   end
      # 
      # :api: public
      def match(path = {}, conditions = {}, &block)
        path, conditions = path[:path], path if path.is_a?(Hash)
        
        raise Error, "The route has already been committed. Further conditions cannot be specified" if @route
        
        conditions.delete_if { |k, v| v.nil? }
        conditions[:path] = merge_paths(path)
        
        behavior = Behavior.new(@proxy, @conditions.merge(conditions), @params, @defaults, @identifiers, @options, @blocks)
        with_behavior_context(behavior, &block)
      end
      
      # Creates a Route from one or more Behavior objects, unless a +block+ is
      # passed in.
      # 
      # ==== Parameters
      # params<Hash>:: The parameters the route maps to.
      # 
      # &block::
      #   All routes defined in the block will be scoped to the params
      #   defined by the #to method.
      # 
      # ==== Block parameters
      # r<Behavior>:: +optional+ - The to behavior object.
      # 
      # ==== Returns
      # Route:: It registers a new route and returns it.
      # 
      # ==== Examples
      #   match('/:controller/:id).to(:action => 'show')
      # 
      #   to(:controller => 'simple') do
      #     match('/test').to(:action => 'index')
      #     match('/other').to(:action => 'other')
      #   end
      # 
      # :api: public
      def to(params = {}, &block)
        raise Error, "The route has already been committed. Further params cannot be specified" if @route
        
        behavior = Behavior.new(@proxy, @conditions, @params.merge(params), @defaults, @identifiers, @options, @blocks)
        
        if block_given?
          with_behavior_context(behavior, &block)
        else
          behavior.to_route
        end
      end
      
      # Equivalent of #to. Allows for some nicer syntax when scoping blocks
      # 
      # ==== Examples
      # Merb::Router.prepare do
      #   with(:controller => "users") do
      #     match("/signup").to(:action => "signup")
      #     match("/login").to(:action => "login")
      #     match("/logout").to(:action => "logout")
      #   end
      # end
      alias :with :to
      
      # Equivalent of #to. Allows for nicer syntax when registering routes with no params
      # 
      # ==== Examples
      # Merb::Router.prepare do
      #   match("/:controller(/:action(/:id))(.:format)").register
      # end
      alias :register :to
      
      # Sets default values for route parameters. If no value for the key
      # can be extracted from the request, then the value provided here
      # will be used.
      # 
      # ==== Parameters
      # defaults<Hash>::
      #   The default values for named segments.
      # 
      # &block::
      #   All routes defined in the block will be scoped to the defaults defined
      #   by the #default method.
      # 
      # ==== Block parameters
      # r<Behavior>:: +optional+ - The defaults behavior object.
      # 
      # :api: public
      def default(defaults = {}, &block)
        behavior = Behavior.new(@proxy, @conditions, @params, @defaults.merge(defaults), @identifiers, @options, @blocks)
        with_behavior_context(behavior, &block)
      end
      
      alias_method :defaults, :default
      
      # Allows the fine tuning of certain router options.
      # 
      # ==== Parameters
      # options<Hash>::
      #   The options to set for all routes defined in the scope. The currently
      #   supported options are:
      #   * :controller_prefix - The module that the controller is included in.
      #   * :name_prefix       - The prefix added to all routes named with #name
      # 
      # &block::
      #   All routes defined in the block will be scoped to the options defined
      #   by the #options method.
      # 
      # ==== Block parameters
      # r<Behavior>:: The options behavior object. This is optional
      # 
      # ==== Examples
      #   # If :group is not matched in the path, it will be "registered" instead
      #   # of nil.
      #   match("/users(/:group)").default(:group => "registered")
      # 
      # :api: public
      def options(opts = {}, &block)
        options = @options.dup
        
        opts.each_pair do |key, value|
          options[key] = (options[key] || []) + [value.freeze] if value
        end
        
        behavior = Behavior.new(@proxy, @conditions, @params, @defaults, @identifiers, options, @blocks)
        with_behavior_context(behavior, &block)
      end
      
      alias_method :options, :options
      
      # Creates a namespace for a route. This way you can have logical
      # separation to your routes.
      # 
      # ==== Parameters
      # name_or_path<String, Symbol>::
      #   The name or path of the namespace.
      # 
      # options<Hash>::
      #   Optional hash (see below)
      # 
      # &block::
      #   All routes defined in the block will be scoped to the namespace defined
      #   by the #namespace method.
      # 
      # ==== Options (opts)
      # :path<String>:: match against this url
      # 
      # ==== Block parameters
      # r<Behavior>:: The namespace behavior object. This is optional
      # 
      # ==== Examples
      #   namespace :admin do
      #     resources :accounts
      #     resource :email
      #   end
      # 
      #   # /super_admin/accounts
      #   namespace(:admin, :path=>"super_admin") do
      #     resources :accounts
      #   end
      # 
      # :api: public
      def namespace(name_or_path, opts = {}, &block)
        name = name_or_path.to_s # We don't want this modified ever
        path = opts.has_key?(:path) ? opts[:path] : name
        
        raise Error, "The route has already been committed. Further options cannot be specified" if @route
        
        # option keys could be nil
        opts[:controller_prefix] = name unless opts.has_key?(:controller_prefix)
        opts[:name_prefix]       = name unless opts.has_key?(:name_prefix)
        opts[:resource_prefix]   = opts[:name_prefix] unless opts.has_key?(:resource_prefix)
        
        behavior = self
        behavior = behavior.match("/#{path}") unless path.nil? || path.empty?
        behavior.options(opts, &block)
      end
      
      # Sets a method for instances of specified Classes to be called before
      # insertion into a route. This is useful when using models and want a
      # specific method to be called on it (For example, for ActiveRecord::Base
      # it would be #to_param).
      # 
      # The default method called on objects is #to_s.
      # 
      # ==== Paramters
      # identifiers<Hash>::
      #   The keys are Classes and the values are the method that instances of the specified
      #   class should have called on.
      # 
      # &block::
      #   All routes defined in the block will be call the specified methods during
      #   generation.
      # 
      # ==== Block parameters
      # r<Behavior>:: The identify behavior object. This is optional
      # 
      # :api: public
      def identify(identifiers = {}, &block)
        identifiers = if Hash === identifiers
          @identifiers.merge(identifiers)
        else
          { Object => identifiers }
        end
        
        behavior = Behavior.new(@proxy, @conditions, @params, @defaults, identifiers.freeze, @options, @blocks)
        with_behavior_context(behavior, &block)
      end
      
      # Creates the most common routes /:controller/:action/:id.format when
      # called with no arguments. You can pass a hash or a block to add parameters
      # or override the default behavior.
      # 
      # ==== Parameters
      # params<Hash>::
      #   This optional hash can be used to augment the default settings
      # 
      # &block::
      #   When passing a block a new behavior is yielded and more refinement is
      #   possible.
      # 
      # ==== Returns
      # Route:: the default route
      # 
      # ==== Examples
      # 
      #   # Passing an extra parameter "mode" to all matches
      #   r.default_routes :mode => "default"
      # 
      #   # specifying exceptions within a block
      #   r.default_routes do |nr|
      #     nr.defer_to do |request, params|
      #       nr.match(:protocol => "http://").to(:controller => "login",
      #         :action => "new") if request.env["REQUEST_URI"] =~ /\/private\//
      #     end
      #   end
      # 
      # :api: public
      def default_routes(params = {}, &block)
        match("/:controller(/:action(/:id))(.:format)").to(params, &block).name(:default)
      end
      
      # Takes a block and stores it for deferred conditional routes. The block
      # takes the +request+ object and the +params+ hash as parameters.
      # 
      # ==== Parameters
      # params<Hash>:: Parameters and conditions associated with this behavior.
      # &conditional_block::
      #   A block with the conditions to be met for the behavior to take
      #   effect.
      # 
      # ==== Returns
      # Route :: The default route.
      # 
      # ==== Note
      # The block takes two parameters, request and params. The params that
      # are passed into the block are *just* the placeholder params from the
      # route. If you want the full parsed params, use request.params.
      #
      # The rationale for this is that request.params is a fairly slow
      # operation, and if the full params parsing is not required, we would
      # rather not do the full parsing.
      # 
      # ==== Examples
      #   defer_to do |request, params|
      #     params.merge :controller => 'here',
      #       :action => 'there' if request.xhr?
      #   end
      # 
      # :api: public
      def defer_to(params = {}, &block)
        defer(block).to(params)
      end
      
      # Takes a Proc as a parameter and applies it as a deferred proc for all the
      # routes defined in the block. This is mostly interesting for plugin
      # developers.
      # 
      # ==== Examples
      #   defered_block = proc do |r, p|
      #     p.merge :controller => 'api/comments' if request.xhr?
      #   end
      #   defer(defered_block) do
      #     resources :comments
      #   end
      # 
      # :api: public
      def defer(deferred_block, &block)
        blocks = @blocks + [CachedProc.new(deferred_block)]
        behavior = Behavior.new(@proxy, @conditions, @params, @defaults, @identifiers, @options, blocks)
        with_behavior_context(behavior, &block)
      end
      
      # Registers the route as a named route with the name given.
      # 
      # ==== Parameters
      # symbol<Symbol>:: the name of the route.
      # 
      # ==== Raises
      # ArgumentError:: symbol is not a Symbol.
      # 
      # :api: public
      def name(prefix, name = nil)
        unless name
          name, prefix = prefix, nil
        end
        
        full_name([prefix, @options[:name_prefix], name].flatten.compact.join('_'))
      end
      
      # Names this route in Router. Name must be a Symbol. The current
      # name_prefix is ignored.
      #
      # ==== Parameters
      # symbol<Symbol>:: The name of the route.
      #
      # ==== Raises
      # ArgumentError:: symbol is not a Symbol.
      # 
      # :api: private
      def full_name(name)
        raise Error, ":this is reserved. Please pick another name." if name == :this
        
        if @route
          @route.name = name
          self
        else
          register.full_name(name)
        end
      end
      
      # Specifies that a route can be fixatable.
      # 
      # ==== Parameters
      # enabled<Boolean>:: True enables fixation on the route.
      # 
      # :api: public
      def fixatable(enable = true)
        @route.fixation = enable
        self
      end
      
      # Redirects the current route.
      # 
      # ==== Parameters
      # path<String>:: The path to redirect to.
      # 
      # options<Hash>::
      #   Options (see below)
      # 
      # ==== Options (opts)
      # :permanent<Boolean>::
      #   Whether or not the redirect should be permanent.
      #   The default value is false.
      # 
      # :api: public
      def redirect(url, opts = {})
        raise Error, "The route has already been committed." if @route
        
        status = opts[:permanent] ? 301 : 302
        @route = Route.new(@conditions, {:url => url.freeze, :status => status.freeze}, @blocks, :redirects => true)
        @route.register
        self
      end
      
      # Capture any new routes that have been added within the block.
      #
      # This utility method lets you track routes that have been added;
      # it doesn't affect how/which routes are added.
      #
      # &block:: A context in which routes are generated.
      # 
      # :api: public
      def capture(&block)
        captured_routes = {}
        name_prefix     = [@options[:name_prefix]].flatten.compact.map { |p| "#{p}_"}
        current_names   = Merb::Router.named_routes.keys
        
        behavior = Behavior.new(@proxy, @conditions, @params, @defaults, @identifiers, @options, @blocks)
        with_behavior_context(behavior, &block)
        
        Merb::Router.named_routes.reject { |k,v| current_names.include?(k) }.each do |name, route|
          name = route.name.to_s.sub("#{name_prefix}", '').to_sym unless name_prefix.empty?
          captured_routes[name] = route
        end
        
        captured_routes
      end
      
      # Proxy routes with the default behaviors.
      # 
      # ==== Parameters
      # &block:: defines routes within the provided context.
      # 
      # :api: private
      def _with_proxy(&block)
        proxy = Proxy.new
        proxy.push Behavior.new(proxy, @conditions, @params, @defaults, @identifiers, @options, @blocks)
        proxy.instance_eval(&block)
        proxy
      end
      
      protected
      
      # Returns the current route.
      # 
      # ==== Returns
      # Route:: the route.
      # 
      # :api: private
      def _route
        @route
      end
      
      # Turns a route definition into a Route object.
      # 
      # ==== Returns
      # Route:: the route generated.
      # 
      # :api: private
      def to_route
        raise Error, "The route has already been committed." if @route
        
        controller = @params[:controller]
        
        if prefixes = @options[:controller_prefix]
          controller ||= ":controller"
          
          prefixes.reverse_each do |prefix|
            break if controller =~ %r{^/(.*)} && controller = $1
            controller = "#{prefix}/#{controller}"
          end
        end
        
        @params.merge!(:controller => controller.to_s.gsub(%r{^/}, '')) if controller
        
        # Sorts the identifiers so that modules that are at the bottom of the
        # inheritance chain come first (more specific modules first). Object
        # should always be last.
        identifiers = @identifiers.sort { |(first,_),(sec,_)| first <=> sec || 1 }
        
        @route = Route.new(@conditions.dup,@params, @blocks, :defaults => @defaults.dup, :identifiers => identifiers)
        
        if before = @options[:before] && @options[:before].last
          @route.register_at(Router.routes.index(before))
        else
          @route.register
        end
        self
      end
      
      # Allows to insert the route at a certain spot in the list of routes
      # instead of appending to the list.
      # 
      # ==== Params
      # route<Route>:: the route to insert before.
      # &block:: the route definition to insert.
      # 
      # :api: plugin
      def before(route, &block)
        options(:before => route, &block)
      end
      
      private
      
      # Takes @conditions and turns values into strings (except for Regexp and
      # Array values).
      # 
      # :api: private
      def stringify_condition_values # :nodoc:
        @conditions.each do |key, value|
          unless value.nil? || Regexp === value || Array === value
            @conditions[key] = value.to_s
          end
        end
      end
      
      # Creates a new context with a given behavior for the route definition in
      # the block.
      # 
      # ==== Params
      # behavior<Behavior>:: the behavior to proxy the calls in the block.
      # &block:: the routing definitions to be nested within the behavior.
      # 
      # ==== Returns
      # Behavior:: the behavior wrapping.
      # 
      # :api: private
      def with_behavior_context(behavior, &block) # :nodoc:
        if block_given?
          @proxy.push(behavior)
          retval = yield(behavior)
          @proxy.pop
        end
        behavior
      end
      
      # Merges the path elements together into an array to be joined for
      # generating named routes.
      # 
      # ==== Parameters
      # path<String>:: the path to merge at the end.
      # 
      # ==== Returns
      # Array:: array of path elements.
      # 
      # ==== Notes
      # An array of ['a', 'b'] (the 'a' namespace with the 'b' action) will
      # produce a name of :a_b.
      # 
      # :api: private
      def merge_paths(path) # :nodoc:
        [@conditions[:path], path.freeze].flatten.compact
      end
      
    end
  end
end
