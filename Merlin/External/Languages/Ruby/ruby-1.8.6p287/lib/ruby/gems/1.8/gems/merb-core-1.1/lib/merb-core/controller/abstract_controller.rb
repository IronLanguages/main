# ==== Why do we use Underscores?
# In Merb, views are actually methods on controllers. This provides
# not-insignificant speed benefits, as well as preventing us from
# needing to copy over instance variables, which we think is proof
# that everything belongs in one class to begin with.
#
# Unfortunately, this means that view helpers need to be included
# into the <strong>Controller</strong> class. To avoid causing confusion
# when your helpers potentially conflict with our instance methods,
# we use an _ to disambiguate. As long as you don't begin your helper
# methods with _, you only need to worry about conflicts with Merb
# methods that are part of the public API.
#
#
#
# ==== Filters
# #before is a class method that allows you to specify before filters in
# your controllers. Filters can either be a symbol or string that
# corresponds to a method name to call, or a proc object. if it is a method
# name that method will be called and if it is a proc it will be called
# with an argument of self where self is the current controller object.
# When you use a proc as a filter it needs to take one parameter.
# 
# #after is identical, but the filters are run after the action is invoked.
#
# ===== Examples
#   before :some_filter
#   before :authenticate, :exclude => [:login, :signup]
#   before :has_role, :with => ["Admin"], :exclude => [:index, :show]
#   before Proc.new { some_method }, :only => :foo
#   before :authorize, :unless => :logged_in?  
#
# You can use either <code>:only => :actionname</code> or 
# <code>:exclude => [:this, :that]</code> but not both at once. 
# <code>:only</code> will only run before the listed actions and 
# <code>:exclude</code> will run for every action that is not listed.
#
# Merb's before filter chain is very flexible. To halt the filter chain you
# use <code>throw :halt</code>. If <code>throw</code> is called with only one 
# argument of <code>:halt</code> the return value of the method 
# <code>filters_halted</code> will be what is rendered to the view. You can 
# override <code>filters_halted</code> in your own controllers to control what 
# it outputs. But the <code>throw</code> construct is much more powerful than 
# just that.
#
# <code>throw :halt</code> can also take a second argument. Here is what that 
# second argument can be and the behavior each type can have:
#
# * +String+:
#   when the second argument is a string then that string will be what
#   is rendered to the browser. Since merb's <code>#render</code> method returns
#   a string you can render a template or just use a plain string:
#
#     throw :halt, "You don't have permissions to do that!"
#     throw :halt, render(:action => :access_denied)
#
# * +Symbol+:
#   If the second arg is a symbol, then the method named after that
#   symbol will be called
#
#     throw :halt, :must_click_disclaimer
#
# * +Proc+:
#   If the second arg is a Proc, it will be called and its return
#   value will be what is rendered to the browser:
#
#     throw :halt, proc { access_denied }
#     throw :halt, proc { Tidy.new(c.index) }
#
# ===== Filter Options (.before, .after, .add_filter, .if, .unless)
# :only<Symbol, Array[Symbol]>::
#   A list of actions that this filter should apply to
#
# :exclude<Symbol, Array[Symbol]::
#   A list of actions that this filter should *not* apply to
# 
# :if<Symbol, Proc>::
#   Only apply the filter if the method named after the symbol or calling the proc evaluates to true
# 
# :unless<Symbol, Proc>::
#   Only apply the filter if the method named after the symbol or calling the proc evaluates to false
#
# :with<Array[Object]>::
#   Arguments to be passed to the filter. Since we are talking method/proc calls,
#   filter method or Proc should to have the same arity
#   as number of elements in Array you pass to this option.
#
# ===== Types (shortcuts for use in this file)
# Filter:: <Array[Symbol, (Symbol, String, Proc)]>
#
# ==== params[:action] and params[:controller] deprecated
# <code>params[:action]</code> and <code>params[:controller]</code> have been deprecated as of
# the 0.9.0 release. They are no longer set during dispatch, and
# have been replaced by <code>action_name</code> and <code>controller_name</code> respectively.

module Merb
  module InlineTemplates; end
  
  class AbstractController
    include Merb::RenderMixin
    include Merb::InlineTemplates

    class_inheritable_accessor :_layout, :_template_root, :template_roots
    class_inheritable_accessor :_before_filters, :_after_filters
    class_inheritable_accessor :_before_dispatch_callbacks, :_after_dispatch_callbacks

    cattr_accessor :_abstract_subclasses

    # :api: plugin
    attr_accessor :body, :action_name, :_benchmarks
    # :api: private
    attr_accessor :_thrown_content  

    # Stub so content-type support in RenderMixin doesn't throw errors
    # :api: private
    attr_accessor :content_type

    FILTER_OPTIONS = [:only, :exclude, :if, :unless, :with]

    self._before_filters, self._after_filters = [], []
    self._before_dispatch_callbacks, self._after_dispatch_callbacks = [], []

    #---
    # We're using abstract_subclasses so that Merb::Controller can have its
    # own subclasses. We're using a Set so we don't have to worry about
    # uniqueness.
    self._abstract_subclasses = Set.new

    # ==== Returns
    # String:: The controller name in path form, e.g. "admin/items".
    # :api: public
    def self.controller_name() @controller_name ||= self.name.to_const_path end

    # ==== Returns
    # String:: The controller name in path form, e.g. "admin/items".
    #
    # :api: public
    def controller_name()      self.class.controller_name                   end
  
    # This is called after the controller is instantiated to figure out where to
    # look for templates under the _template_root. Override this to define a new
    # structure for your app.
    #
    # ==== Parameters
    # context<~to_s>:: The controller context (the action or template name).
    # type<~to_s>:: The content type. Could be nil. 
    # controller<~to_s>::
    #   The name of the controller. Defaults to being called with the controller_name.  Set t
    #
    #
    # ==== Returns
    # String:: 
    #   Indicating where to look for the template for the current controller,
    #   context, and content-type.
    #
    # ==== Notes
    # The type is irrelevant for controller-types that don't support
    # content-type negotiation, so we default to not include it in the
    # superclass.
    #
    # ==== Examples
    #   def _template_location
    #     "#{params[:controller]}.#{params[:action]}.#{content_type}"
    #   end
    #
    # This would look for templates at controller.action.mime.type instead
    # of controller/action.mime.type
    #
    # :api: public
    # @overridable
    def _template_location(context, type, controller)
      controller ? "#{controller}/#{context}" : context
    end

    # The location to look for a template - override this method for particular behaviour. 
    #
    # ==== Parameters
    # template<String>:: The absolute path to a template - without template extension.
    # type<~to_s>::
    #    The mime-type of the template that will be rendered. Defaults to being called with nil.
    #
    # :api: public
    # @overridable
    def _absolute_template_location(template, type)
      template
    end

    # Resets the template roots to the template root passed in.
    #
    # ==== Parameters
    # root<~to_s>:: 
    #   The new path to set the template root to.  
    #
    # :api: public
    def self._template_root=(root)
      @_template_root = root
      _reset_template_roots
    end

    # Reset the template root based on the @_template_root ivar.
    #
    # :api: private
    def self._reset_template_roots
      self.template_roots = [[self._template_root, :_template_location]]
    end

    # ==== Returns
    # roots<Array[Array]>::
    #   Template roots as pairs of template root path and template location
    #   method.
    #
    # :api: plugin
    def self._template_roots
      self.template_roots || _reset_template_roots
    end

    # ==== Parameters
    # roots<Array[Array]>::
    #   Template roots as pairs of template root path and template location
    #   method.
    #
    # :api: plugin
    def self._template_roots=(roots)
      self.template_roots = roots
    end
  
    # Returns the list of classes that have specifically subclassed AbstractController.  
    # Does not include all decendents.  
    #
    # ==== Returns
    # Set:: The subclasses.
    #
    # :api: private
    def self.subclasses_list() _abstract_subclasses end
  
    # ==== Parameters
    # klass<Merb::AbstractController>::
    #   The controller that is being inherited from Merb::AbstractController
    #
    # :api: private
    def self.inherited(klass)
      _abstract_subclasses << klass.to_s
      helper_module_name = klass.to_s =~ /^(#|Merb::)/ ? "#{klass}Helper" : "Merb::#{klass}Helper"
      Object.make_module helper_module_name
      klass.class_eval <<-HERE
        include Object.full_const_get("#{helper_module_name}") rescue nil
      HERE
      super
    end    
  
    # This will initialize the controller, it is designed to be overridden in subclasses (like MerbController)
    # ==== Parameters
    # *args:: The args are ignored in this class, but we need this so that subclassed initializes can have parameters
    #
    # :api: private
    def initialize(*args)
      @_benchmarks = {}
      @_caught_content = {}
    end
  
    # This will dispatch the request, calling internal before/after dispatch callbacks.  
    # If the return value of _call_filters is not :filter_chain_completed the action is not called, and the return from the filters is used instead. 
    # 
    # ==== Parameters
    # action<~to_s>::
    #   The action to dispatch to. This will be #send'ed in _call_action.
    #   Defaults to :to_s.
    #
    # ==== Returns
    # <~to_s>::
    #   Returns the string that was returned from the action. 
    #
    # ==== Raises
    # ArgumentError:: Invalid result caught from before filters.
    #
    # :api: plugin
    def _dispatch(action)
      self.action_name = action
      self._before_dispatch_callbacks.each { |cb| cb.call(self) }

      caught = catch(:halt) do
        start = Time.now
        result = _call_filters(_before_filters)
        @_benchmarks[:before_filters_time] = Time.now - start if _before_filters
        result
      end
  
      @body = case caught
      when :filter_chain_completed  then _call_action(action_name)
      when String                   then caught
      # return *something* if you throw halt with nothing
      when nil                      then "<html><body><h1>Filter Chain Halted!</h1></body></html>"
      when Symbol                   then __send__(caught)
      when Proc                     then self.instance_eval(&caught)
      else
        raise ArgumentError, "Threw :halt, #{caught}. Expected String, nil, Symbol, Proc."
      end
      start = Time.now
      _call_filters(_after_filters)
      @_benchmarks[:after_filters_time] = Time.now - start if _after_filters
    
      self._after_dispatch_callbacks.each { |cb| cb.call(self) }
    
      @body
    end
  
    # This method exists to provide an overridable hook for ActionArgs.  It uses #send to call the action method.
    #
    # ==== Parameters
    # action<~to_s>:: the action method to dispatch to
    #
    # :api: plugin
    # @overridable
    def _call_action(action)
      send(action)
    end
  
    # Calls a filter chain. 
    #
    # ==== Parameters
    # filter_set<Array[Filter]>::
    #   A set of filters in the form [[:filter, rule], [:filter, rule]]
    #
    # ==== Returns
    # Symbol:: :filter_chain_completed.
    #
    # ==== Notes
    # Filter rules can be Symbols, Strings, or Procs.
    #
    # Symbols or Strings::
    #   Call the method represented by the +Symbol+ or +String+.
    # Procs::
    #   Execute the +Proc+, in the context of the controller (self will be the
    #   controller)
    #
    # :api: private
    def _call_filters(filter_set)
      (filter_set || []).each do |filter, rule|
        if _call_filter_for_action?(rule, action_name) && _filter_condition_met?(rule)
          case filter
          when Symbol, String
            if rule.key?(:with)
              args = rule[:with]
              send(filter, *args)
            else
              send(filter)
            end
          when Proc then self.instance_eval(&filter)
          end
        end
      end
      return :filter_chain_completed
    end

    # Determine whether the filter should be called for the current action using :only and :exclude.
    #
    # ==== Parameters
    # rule<Hash>:: Rules for the filter (see below).
    # action_name<~to_s>:: The name of the action to be called.
    #
    # ==== Options (rule)
    # :only<Array>::
    #   Optional list of actions to fire. If given, action_name must be a part of
    #   it for this function to return true.
    # :exclude<Array>::
    #   Optional list of actions not to fire. If given, action_name must not be a
    #   part of it for this function to return true.
    #
    # ==== Returns
    # Boolean:: True if the action should be called.
    #
    # :api: private
    def _call_filter_for_action?(rule, action_name)
      # Both:
      # * no :only or the current action is in the :only list
      # * no :exclude or the current action is not in the :exclude list
      (!rule.key?(:only) || rule[:only].include?(action_name)) &&
      (!rule.key?(:exclude) || !rule[:exclude].include?(action_name))
    end

    # Determines whether the filter should be run based on the conditions passed (:if and :unless)
    #
    # ==== Parameters
    # rule<Hash>:: Rules for the filter (see below).
    #
    # ==== Options (rule)
    # :if<Array>:: Optional conditions that must be met for the filter to fire.
    # :unless<Array>::
    #   Optional conditions that must not be met for the filter to fire.
    #
    # ==== Returns
    # Boolean:: True if the conditions are met.
    #
    # :api: private
    def _filter_condition_met?(rule)
      # Both:
      # * no :if or the if condition evaluates to true
      # * no :unless or the unless condition evaluates to false
      (!rule.key?(:if) || _evaluate_condition(rule[:if])) &&
      (!rule.key?(:unless) || ! _evaluate_condition(rule[:unless]))
    end

    # Evaluates a filter condition (:if or :unless)
    #
    # ==== Parameters
    # condition<Symbol, Proc>:: The condition to evaluate.
    #
    # ==== Raises
    # ArgumentError:: condition not a Symbol or Proc.
    #
    # ==== Returns
    # Boolean:: True if the condition is met.
    #
    # ==== Alternatives
    # If condition is a symbol, it will be send'ed. If it is a Proc it will be
    # called directly with self as an argument.
    #
    # :api: private
    def _evaluate_condition(condition)
      case condition
      when Symbol then self.send(condition)
      when Proc then self.instance_eval(&condition)
      else
        raise ArgumentError,
              'Filter condtions need to be either a Symbol or a Proc'
      end
    end

    # Adds a filter to the after filter chain
    # ==== Parameters
    # filter<Symbol, Proc>:: The filter to add. Defaults to nil.
    # opts<Hash>::
    #   Filter options (see class documentation under <tt>Filter Options</tt>).
    # &block:: A block to use as a filter if filter is nil.
    #
    # ==== Notes
    # If the filter already exists, its options will be replaced with opts.;
    #
    # :api: public
    def self.after(filter = nil, opts = {}, &block)
      add_filter(self._after_filters, filter || block, opts)
    end

    # Adds a filter to the before filter chain.  
    #
    # ==== Parameters
    # filter<Symbol, Proc>:: The filter to add. Defaults to nil.
    # opts<Hash>::
    #   Filter options (see class documentation under <tt>Filter Options</tt>).
    # &block:: A block to use as a filter if filter is nil.
    #
    # ==== Notes
    # If the filter already exists, its options will be replaced with opts.
    #
    # :api: public
    def self.before(filter = nil, opts = {}, &block)
      add_filter(self._before_filters, filter || block, opts)
    end
     
    # Removes a filter from the after filter chain.  This removes the 
    # filter from the filter chain for the whole controller and does not 
    # take any options. 
    #
    # ==== Parameters
    # filter<Symbol, String>:: A filter name to skip.
    #
    # :api: public
    def self.skip_after(filter)
      skip_filter(self._after_filters, filter)
    end
  
    # Removes a filter from the before filter chain.  This removes the 
    # filter from the filter chain for the whole controller and does not 
    # take any options.
    #
    # ==== Parameters
    # filter<Symbol, String>:: A filter name to skip.
    #
    # :api: public
    def self.skip_before(filter)
      skip_filter(self._before_filters , filter)
    end  

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
  
    alias_method :relative_url, :url

    # Returns the absolute url including the passed protocol and host.  
    # 
    # This uses the same arguments as the url method, with added requirements 
    # of protocol and host options. 
    #
    # :api: public
    def absolute_url(*args)
      # FIXME: arrgh, why request.protocol returns http://?
      # :// is not part of protocol name
      options  = extract_options_from_args!(args) || {}
      protocol = options.delete(:protocol)
      host     = options.delete(:host)
    
      raise ArgumentError, "The :protocol option must be specified" unless protocol
      raise ArgumentError, "The :host option must be specified"     unless host
    
      args << options
    
      protocol + "://" + host + url(*args)
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
      args << {}
      Merb::Router.resource(*args)
    end

    # Calls the capture method for the selected template engine.
    #
    # ==== Parameters
    # *args:: Arguments to pass to the block.
    # &block:: The block to call.
    #
    # ==== Returns
    # String:: The output of a template block or the return value of a non-template block converted to a string.
    #
    # :api: public
    def capture(*args, &block)
      ret = nil

      captured = send("capture_#{@_engine}", *args) do |*args|
        ret = yield *args
      end

      # return captured value only if it is not empty
      captured.empty? ? ret.to_s : captured
    end

    # Calls the concatenate method for the selected template engine.
    #
    # ==== Parameters
    # str<String>:: The string to concatenate to the buffer.
    # binding<Binding>:: The binding to use for the buffer.
    #
    # :api: public
    def concat(str, binding)
      send("concat_#{@_engine}", str, binding)
    end

    private
    # adds a filter to the specified filter chain
    # ==== Parameters
    # filters<Array[Filter]>:: The filter chain that this should be added to.
    # filter<Filter>:: A filter that should be added.
    # opts<Hash>::
    #   Filter options (see class documentation under <tt>Filter Options</tt>).
    #
    # ==== Raises
    # ArgumentError::
    #   Both :only and :exclude, or :if and :unless given, if filter is not a
    #   Symbol, String or Proc, or if an unknown option is passed.
    #
    # :api: private
    def self.add_filter(filters, filter, opts={})
      raise(ArgumentError,
        "You can specify either :only or :exclude but 
         not both at the same time for the same filter.") if opts.key?(:only) && opts.key?(:exclude)
       
       raise(ArgumentError,
         "You can specify either :if or :unless but 
          not both at the same time for the same filter.") if opts.key?(:if) && opts.key?(:unless)
        
      opts.each_key do |key| raise(ArgumentError,
        "You can only specify known filter options, #{key} is invalid.") unless FILTER_OPTIONS.include?(key)
      end

      opts = normalize_filters!(opts)
    
      case filter
      when Proc
        # filters with procs created via class methods have identical signature
        # regardless if they handle content differently or not. So procs just
        # get appended
        filters << [filter, opts]
      when Symbol, String
        if existing_filter = filters.find {|f| f.first.to_s == filter.to_s}
          filters[ filters.index(existing_filter) ] = [filter, opts]
        else
          filters << [filter, opts]
        end
      else
        raise(ArgumentError, 
          'Filters need to be either a Symbol, String or a Proc'
        )        
      end
    end  

    # Skip a filter that was previously added to the filter chain. Useful in
    # inheritence hierarchies.
    #
    # ==== Parameters
    # filters<Array[Filter]>:: The filter chain that this should be removed from.
    # filter<Filter>:: A filter that should be removed.
    #
    # ==== Raises
    # ArgumentError:: filter not Symbol or String.
    #
    # :api: private
    def self.skip_filter(filters, filter)
      raise(ArgumentError, 'You can only skip filters that have a String or Symbol name.') unless
        [Symbol, String].include? filter.class

      Merb.logger.warn("Filter #{filter} was not found in your filter chain.") unless
        filters.reject! {|f| f.first.to_s[filter.to_s] }
    end

    # Ensures that the passed in hash values are always arrays.
    #
    # ==== Parameters
    # opts<Hash>:: Options for the filters (see below).
    #
    # ==== Options (opts)
    # :only<Symbol, Array[Symbol]>:: A list of actions.
    # :exclude<Symbol, Array[Symbol]>:: A list of actions.
    #
    # ==== Examples
    #   normalize_filters!(:only => :new) #=> {:only => [:new]}
    #
    # :api: public
    def self.normalize_filters!(opts={})
      opts[:only]     = Array(opts[:only]).map {|x| x.to_s} if opts[:only]
      opts[:exclude]  = Array(opts[:exclude]).map {|x| x.to_s} if opts[:exclude]
      return opts
    end
  end
end