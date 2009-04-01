Merb::Router.extensions do
      
  # Add all known slices to the router
  #
  # By combining this with Merb::Slices.activate_by_file and Merb::Slices.deactivate
  # one can enable/disable slices at runtime, without restarting your app.
  #
  # @param config<Hash> 
  #  Optional hash, mapping slice module names to their settings;
  #  set :path (or use a string) if you want to override what appears on the url.
  #
  # @yield A new Behavior instance is yielded in the block for nested routes.
  # @yieldparam ns<Behavior> The namespace behavior object.
  #
  # @example all_slices('BlogSlice' => 'blog', 'ForumSlice' => { :path => 'forum' })
  #
  # @note The block is yielded for each slice individually.
  def all_slices(config = {}, &block)
    Merb::Slices.slice_names.each { |module_name| add_slice(module_name, config[module_name] || {}, &block) }
  end
  alias :add_slices :all_slices
  
  # Add a Slice in a router namespace
  # 
  # @param slice_module<String, Symbol, Module> A Slice module to mount.
  # @param options<Hash, String> Optional hash, set :path if you want to override what appears on the url.
  # 
  # @yield A new Behavior instance is yielded in the block for nested routes - runs before the slice routes are setup.
  # @yieldparam ns<Behavior> The namespace behavior object.
  #
  # @return <Behaviour> The current router context.
  #
  # @note If a slice has no routes at all, the activate hook won't be executed.
  #
  # @note Normally you should specify the slice_module using a String or Symbol
  #       this ensures that your module can be removed from the router at runtime.
  def add_slice(slice_module, options = {}, &block)
    if Merb::Slices.exists?(slice_module)
      options = { :path => options } if options.is_a?(String)
      slice_module = Object.full_const_get(slice_module.to_s.camel_case) if slice_module.class.in?(String, Symbol)
      namespace = options[:namespace] || slice_module.identifier_sym
      options[:path] ||= options[:path_prefix] || slice_module[:path_prefix] || options[:namespace] || slice_module.identifier
      options[:prepend_routes] = block if block_given?
      slice_module[:path_prefix] = options[:path]
      Merb.logger.verbose!("Mounting slice #{slice_module} at /#{options[:path]}")
      
      # reset the inherited controller prefix - especially for 'slice' entries (see below)
      @options[:controller_prefix] = nil if options.delete(:reset_controller_prefix)
      
      # setup routes - capture the slice's routes for easy reference
      self.namespace(namespace, options.except(:default_routes, :prepend_routes, :append_routes, :path_prefix)) do |ns|
        Merb::Slices.named_routes[slice_module.identifier_sym] = ns.capture do
          options[:prepend_routes].call(ns) if options[:prepend_routes].respond_to?(:call)
          slice_module.setup_router(ns)     # setup the routes from the slice itself
          options[:append_routes].call(ns)  if options[:append_routes].respond_to?(:call)
        end
      end
    else 
      Merb.logger.info!("Skipped adding slice #{slice_module} to router...")
    end
    self
  end
  
  # Insert a slice directly into the current router context.
  #
  # This will still setup a namespace, but doesn't set a path prefix. Only for special cases.
  def slice(slice_module, options = {}, &block)
    options[:path] ||= ""
    add_slice(slice_module, options.merge(:reset_controller_prefix => true), &block)
  end

end