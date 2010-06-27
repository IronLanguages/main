module Merb::RenderMixin
  # So we can do raise TemplateNotFound
  include Merb::ControllerExceptions

  # ==== Parameters
  # base<Module>:: Module that is including RenderMixin (probably a controller)
  #
  # @private
  def self.included(base)
    base.extend(ClassMethods)
    base.class_eval do
      class_inheritable_accessor :_default_render_options
    end
  end

  module ClassMethods

    def _templates_for
      @_templates_for ||= {}
    end

    # Return the default render options.
    #
    # ==== Returns
    # Hash:: An options hash
    #
    # :api: public
    def default_render_options
      self._default_render_options ||= {}
    end

    # Set default render options at the class level.
    #
    # ==== Parameters
    # opts<Hash>:: An options hash
    #
    # :api: public
    def render_options(opts)
      self._default_render_options = opts
    end

    # Set the default layout to use or nil/false to disable layout rendering.
    # This is a shortcut for render_options :layout => false.
    #
    # ==== Parameters
    # layout<~to_s>:: The layout that should be used for this class.
    #
    # ==== Notes
    # You can override by passing :layout => true to render method.
    #
    # ==== Returns
    # Hash:: The default render options.
    #
    # :api: public
    def layout(layout)
      self.default_render_options.update(:layout => (layout || false))
    end

    # Enable the default layout logic - reset the layout option.
    #
    # ==== Returns
    # ~to_s:: The layout that was previously set.
    #
    # :api: public
    def default_layout
      self.default_render_options.delete(:layout)
    end

  end

  # Render the specified item, with the specified options.
  #
  # ==== Parameters
  # thing<String, Symbol, nil>::
  #   The thing to render. This will default to the current action
  # opts<Hash>:: An options hash (see below)
  #
  # ==== Options (opts)
  # :format<Symbol>:: A registered mime-type format
  # :template<String>::
  #   The path to the template relative to the template root
  # :status<~to_i>::
  #   The status to send to the client. Typically, this would be an integer
  #   (200), or a Merb status code (Accepted)
  # :layout<~to_s, FalseClass>::
  #   A layout to use instead of the default. This should be relative to the
  #   layout root. By default, the layout will be either the controller_name or
  #   application. If you want to use an alternative content-type than the one
  #   that the base template was rendered as, you will need to do :layout =>
  #   "foo.#{content_type}" (i.e. "foo.json"). If you want to render without
  #   layout, use :layout => false. This overrides layout set by +layout+ method.
  #
  # ==== Returns
  # String:: The rendered template, including layout, if appropriate.
  #
  # ==== Raises
  # TemplateNotFound:: There is no template for the specified location.
  #
  # ==== Alternatives
  # If you pass a Hash as the first parameter, it will be moved to opts and
  # "thing" will be the current action
  #
  # :api: public
  def render(thing = nil, opts = {})
    # render :format => :xml means render nil, :format => :xml
    opts, thing = thing, nil if thing.is_a?(Hash)

    # Merge with class level default render options
    opts = self.class.default_render_options.merge(opts)

    # If you don't specify a thing to render, assume they want to render the current action
    thing ||= action_name.to_sym

    # Content negotiation
    self.content_type = opts[:format] if opts[:format]

    # Handle options (:status)
    _handle_options!(opts)

    # Do we have a template to try to render?
    if thing.is_a?(Symbol) || opts[:template]

      template_method, template_location = 
        _template_for(thing, content_type, controller_name, opts[:template])

      # Raise an error if there's no template
      unless template_method && self.respond_to?(template_method)
        template_files = Merb::Template.template_extensions.map { |ext| "#{template_location}.#{ext}" }
        raise TemplateNotFound, "Oops! No template found. Merb was looking for #{template_files.join(', ')} " + 
          "for content type '#{content_type}'. You might have mispelled the template or file name. " + 
          "Registered template extensions: #{Merb::Template.template_extensions.join(', ')}. " +
          "If you use Haml or some other template plugin, make sure you required Merb plugin dependency " + 
          "in your init file."
      end

      # Call the method in question and throw the content for later consumption by the layout
      throw_content(:for_layout, self.send(template_method))

    # Do we have a string to render?
    elsif thing.is_a?(String)

      # Throw it for later consumption by the layout
      throw_content(:for_layout, thing)
    end

    # If we find a layout, use it. Otherwise, just render the content thrown for layout.
    (layout = _get_layout(opts[:layout])) ? send(layout) : catch_content(:for_layout)
  end

  # Renders an object using to registered transform method based on the
  # negotiated content-type, if a template does not exist. For instance, if the
  # content-type is :json, Merb will first look for current_action.json.*.
  # Failing that, it will run object.to_json.
  #
  # ==== Parameter
  # object<Object>::
  #   An object that responds_to? the transform method registered for the
  #   negotiated mime-type.
  # thing<String, Symbol>::
  #   The thing to attempt to render via #render before calling the transform
  #   method on the object. Defaults to nil.
  # opts<Hash>::
  #   An options hash that will be used for rendering
  #   (passed on to #render or serialization methods like #to_json or #to_xml)
  #
  # ==== Returns
  # String::
  #   The rendered template or if no template is found, the transformed object.
  #
  # ==== Raises
  # NotAcceptable::
  #   If there is no transform method for the specified mime-type or the object
  #   does not respond to the transform method.
  #
  # ==== Alternatives
  # A string in the second parameter will be interpreted as a template:
  #   display @object, "path/to/foo"
  #   #=> display @object, nil, :template => "path/to/foo"
  #
  # A hash in the second parameters will be interpreted as opts:
  #   display @object, :layout => "zoo"
  #   #=> display @object, nil, :layout => "zoo"
  #
  # If you need to pass extra parameters to serialization method, for instance,
  # to exclude some of attributes or serialize associations, just pass options
  # for it.
  # For instance,
  #
  # display @locations, :except => [:locatable_type, :locatable_id], :include => [:locatable]
  #
  # serializes object with polymorphic association, not raw locatable_* attributes.
  #
  #
  # ==== Options
  #
  # :template                a template to use for rendering
  # :layout                  a layout to use for rendering
  # :status                  the status code to return (defaults to 200)
  # :location                the value of the Location header
  #
  # all other options        options that will be pass to serialization method
  #                          like #to_json or #to_xml
  #
  # ==== Notes
  # The transformed object will not be used in a layout unless a :layout is
  # explicitly passed in the opts.
  #
  # :api: public
  def display(object, thing = nil, opts = {})
    template_opt = thing.is_a?(Hash) ? thing.delete(:template) : opts.delete(:template)

    case thing
    # display @object, "path/to/foo" means display @object, nil, :template => "path/to/foo"
    when String
      template_opt, thing = thing, nil
    # display @object, :template => "path/to/foo" means display @object, nil, :template => "path/to/foo"
    when Hash
      opts, thing = thing, nil
    end

    # Try to render without the object
    render(thing || action_name.to_sym, opts.merge(:template => template_opt))

  # If the render fails (i.e. a template was not found)
  rescue TemplateNotFound => e
    # Merge with class level default render options
    # @todo can we find a way to refactor this out so we don't have to do it everywhere?
    opts = self.class.default_render_options.merge(opts)

    # Figure out what to transform and raise NotAcceptable unless there's a transform method assigned
    transform = Merb.mime_transform_method(content_type)
    if !transform
      raise NotAcceptable, "#{e.message} and there was no transform method registered for #{content_type.inspect}"
    elsif !object.respond_to?(transform)
      raise NotAcceptable, "#{e.message} and your object does not respond to ##{transform}"
    end

    layout_opt = opts.delete(:layout)
    _handle_options!(opts)
    throw_content(:for_layout, opts.empty? ? object.send(transform) : object.send(transform, opts))
    
    meth, _ = _template_for(layout_opt, layout_opt.to_s.index(".") ? nil : content_type, "layout") if layout_opt
    meth ? send(meth) : catch_content(:for_layout)
  end

  # Render a partial template.
  #
  # ==== Parameters
  # template<~to_s>::
  #   The path to the template, relative to the current controller or the
  #   template root; absolute path will work too. If the template contains a "/", 
  #   Merb will search for it relative to the template root; otherwise, 
  #   Merb will search for it relative to the current controller.
  # opts<Hash>:: A hash of options (see below)
  #
  # ==== Options (opts)
  # :with<Object, Array>::
  #   An object or an array of objects that will be passed into the partial.
  # :as<~to_sym>:: The local name of the :with Object inside of the partial.
  # :format<Symbol>:: The mime format that you want the partial to be in (:js, :html, etc.)
  # others::
  #   A Hash object names and values that will be the local names and values
  #   inside the partial.
  #
  # ==== Examples
  #   partial :foo, :hello => @object
  #
  # The "_foo" partial will be called, relative to the current controller,
  # with a local variable of +hello+ inside of it, assigned to @object.
  #
  #   partial :bar, :with => ['one', 'two', 'three']
  #
  # The "_bar" partial will be called once for each element of the array
  # specified by :with for a total of three iterations. Each element
  # of the array will be available in the partial via a local variable named
  # +bar+. Additionally, there will be two extra local variables:
  # +collection_index+ and +collection_size+. +collection_index+ is the index
  # of the object currently referenced by +bar+ in the collection passed to
  # the partial. +collection_size+ is the total size of the collection.
  #
  # By default, the object specified by :with will be available through a
  # local variable with the same name as the partial template. However,
  # this can be changed using the :as option.
  #
  #   partial :bar, :with => "one", :as => :number
  #
  # In this case, "one" will be available in the partial through the local
  # variable named +number+.
  #
  # :api: public
  def partial(template, opts={})

    # partial :foo becomes "#{controller_name}/_foo"
    # partial "foo/bar" becomes "foo/_bar"
    template = template.to_s
    if template =~ %r{^/}
      template_path = File.dirname(template) / "_#{File.basename(template)}"
    else
      kontroller = (m = template.match(/.*(?=\/)/)) ? m[0] : controller_name
    end
    template = "_#{File.basename(template)}"

    # This handles no :with as well
    with = [opts.delete(:with)].flatten
    as = (opts.delete(:as) || template.match(%r[(?:.*/)?_([^\./]*)])[1]).to_sym

    # Ensure that as is in the locals hash even if it isn't passed in here
    # so that it's included in the preamble. 
    locals = opts.merge(:collection_index => -1, :collection_size => with.size, as => opts[as])
    template_method, template_location = _template_for(
      template, 
      opts.delete(:format) || content_type, 
      kontroller, 
      template_path, 
      locals.keys)
    
    # this handles an edge-case where the name of the partial is _foo.* and your opts
    # have :foo as a key.
    named_local = opts.key?(as)
    
    sent_template = with.map do |temp|
      locals[as] = temp unless named_local

      if template_method && self.respond_to?(template_method)
        locals[:collection_index] += 1
        send(template_method, locals)
      else
        raise TemplateNotFound, "Could not find template at #{template_location}.*"
      end
    end.join
    
    sent_template
  end

  # Take the options hash and handle it as appropriate.
  #
  # ==== Parameters
  # opts<Hash>:: The options hash that was passed into render.
  #
  # ==== Options
  # :status<~to_i>::
  #   The status of the response will be set to opts[:status].to_i
  #
  # ==== Returns
  # Hash:: The options hash that was passed in.
  #
  # :api: private
  def _handle_options!(opts)
    self.status = opts.delete(:status).to_i if opts[:status]
    headers["Location"] = opts.delete(:location) if opts[:location]
    opts
  end

  # Get the layout that should be used. The content-type will be appended to
  # the layout unless the layout already contains a "." in it.
  #
  # If no layout was passed in, this method will look for one with the same
  # name as the controller, and finally one in "application.#{content_type}".
  #
  # ==== Parameters
  # layout<~to_s>:: A layout, relative to the layout root. Defaults to nil.
  #
  # ==== Returns
  # String:: The method name that corresponds to the found layout.
  #
  # ==== Raises
  # TemplateNotFound::
  #   If a layout was specified (either via layout in the class or by passing
  #   one in to this method), and not found. No error will be raised if no
  #   layout was specified, and the default layouts were not found.
  #
  # :api: private
  def _get_layout(layout = nil)
    return false if layout == false
    
    layout = layout.instance_of?(Symbol) && self.respond_to?(layout, true) ? send(layout) : layout
    layout = layout.to_s if layout

    # If a layout was provided, throw an error if it's not found
    if layout      
      template_method, template_location = 
        _template_for(layout, layout.index(".") ? nil : content_type, "layout")
        
      raise TemplateNotFound, "No layout found at #{template_location}" unless template_method
      template_method

    # If a layout was not provided, try the default locations
    else
      template, location = _template_for(controller_name, content_type, "layout")
      template, location = _template_for("application", content_type, "layout") unless template
      template
    end
  end

  # Iterate over the template roots in reverse order, and return the template
  # and template location of the first match.
  #
  # ==== Parameters
  # context<Object>:: The controller action or template (basename or absolute path).
  # content_type<~to_s>:: The content type (like html or json).
  # controller<~to_s>:: The name of the controller. Defaults to nil.
  # locals<Array[Symbol]>:: A list of locals to assign from the args passed into the compiled template.
  #
  # ==== Options (opts)
  # :template<String>::
  #   The location of the template to use. Defaults to whatever matches this
  #   context, content_type and controller.
  #
  # ==== Returns
  # Array[Symbol, String]::
  #   A pair consisting of the template method and location.
  #
  # :api: private
  def _template_for(context, content_type, controller=nil, template=nil, locals=[])
    tmp = self.class._templates_for[[context, content_type, controller, template, locals]]
    return tmp if tmp

    template_method, template_location = nil, nil

    # absolute path to a template (:template => "/foo/bar")
    if template.is_a?(String) && template =~ %r{^/}
      template_location = self._absolute_template_location(template, content_type)
      return [_template_method_for(template_location, locals), template_location]
    end

    self.class._template_roots.reverse_each do |root, template_meth|
      # :template => "foo/bar.html" where root / "foo/bar.html.*" exists
      if template
        template_location = root / self.send(template_meth, template, content_type, nil)
      # :layout => "foo" where root / "layouts" / "#{controller}.html.*" exists        
      else
        template_location = root / self.send(template_meth, context, content_type, controller)
      end
    
      break if template_method = _template_method_for(template_location.to_s, locals)
    end

    # template_location is a Pathname
    ret = [template_method, template_location.to_s]
    unless Merb::Config[:reload_templates]
      self.class._templates_for[[context, content_type, controller, template, locals]] = ret
    end
    ret
  end
  
  # Return the template method for a location, and check to make sure the current controller
  # actually responds to the method.
  #
  # ==== Parameters
  # template_location<String>:: The phyical path of the template
  # locals<Array[Symbol]>:: A list of locals to assign from the args passed into the compiled template.
  #
  # ==== Returns
  # String:: The method, if it exists. Otherwise return nil.
  #
  # :api: private
  def _template_method_for(template_location, locals)
    meth = Merb::Template.template_for(template_location, [], locals)
    meth && self.respond_to?(meth) ? meth : nil
  end

  # Called in templates to get at content thrown in another template. The
  # results of rendering a template are automatically thrown into :for_layout,
  # so catch_content or catch_content(:for_layout) can be used inside layouts
  # to get the content rendered by the action template.
  #
  # ==== Parameters
  # obj<Object>:: The key in the thrown_content hash. Defaults to :for_layout.
  #
  # :api: public
  def catch_content(obj = :for_layout)
    @_caught_content[obj] || ''
  end

  # Called in templates to test for the existence of previously thrown content.
  #
  # ==== Parameters
  # obj<Object>:: The key in the thrown_content hash. Defaults to :for_layout.
  #
  # :api: public
  def thrown_content?(obj = :for_layout)
    @_caught_content.key?(obj)
  end

  # Called in templates to store up content for later use. Takes a string
  # and/or a block. First, the string is evaluated, and then the block is
  # captured using the capture() helper provided by the template languages. The
  # two are concatenated together.
  #
  # ==== Parameters
  # obj<Object>:: The key in the thrown_content hash.
  # string<String>:: Textual content. Defaults to nil.
  # &block:: A block to be evaluated and concatenated to string.
  #
  # ==== Raises
  # ArgumentError:: Neither string nor block given.
  #
  # ==== Example
  #   throw_content(:foo, "Foo")
  #   catch_content(:foo) #=> "Foo"
  #
  # :api: public
  def throw_content(obj, string = nil, &block)
    unless string || block_given?
      raise ArgumentError, "You must pass a block or a string into throw_content"
    end
    @_caught_content[obj] = string.to_s << (block_given? ? capture(&block) : "")
  end

  # Called in templates to append content for later use. Works like throw_content.
  #
  # @param [Object] obj
  #   Key used in the thrown_content hash.
  # @param [String] string
  #   Textual content. Default to nil.
  # @yield 
  #   Evaluated with result concatenated to string.
  #
  # @raise [ArgumentError]
  #   Neither string nor block given
  #
  # :api: public
  def append_content(obj, string = nil, &block)
    unless string || block_given?
      raise ArgumentError, "You must pass a block or a string into append_content"
    end
    @_caught_content[obj] = "" if @_caught_content[obj].nil?
    @_caught_content[obj] << string.to_s << (block_given? ? capture(&block) : "")
  end

  # Called when renderers need to be sure that existing thrown content is cleared
  # before throwing new content. This prevents double rendering of content when
  # multiple templates are rendered after each other.
  #
  # ==== Parameters
  # obj<Object>:: The key in the thrown_content hash. Defaults to :for_layout.
  #
  # :api: public
  def clear_content(obj = :for_layout)
    @_caught_content.delete(obj) unless @_caught_content[obj].nil?
  end

end
