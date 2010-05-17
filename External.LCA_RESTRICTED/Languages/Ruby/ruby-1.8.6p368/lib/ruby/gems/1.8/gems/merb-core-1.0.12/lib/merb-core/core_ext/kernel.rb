require 'rubygems/dependency'

module Gem
  class Dependency
    # :api: private
    attr_accessor :require_block, :require_as, :original_caller
  end
end

module Kernel
  
  # Keep track of all required dependencies. 
  #
  # @param name<String> The name of the gem to load.
  # @param *ver<Gem::Requirement, Gem::Version, Array, #to_str>
  #   Version requirements to be passed to Gem::Dependency.new.
  #
  # @return <Gem::Dependency> Dependency information
  #
  # :api: private
  def track_dependency(name, clr, *ver, &blk)
    options = ver.last.is_a?(Hash) ? ver.pop : {}
    new_dep = Gem::Dependency.new(name, ver.empty? ? nil : ver)
    new_dep.require_block = blk
    new_dep.require_as = options.key?(:require_as) ? options[:require_as] : name
    new_dep.original_caller = clr
    
    deps = Merb::BootLoader::Dependencies.dependencies

    idx = deps.each_with_index {|d,i| break i if d.name == new_dep.name}

    idx = idx.is_a?(Array) ? deps.size + 1 : idx
    deps.delete_at(idx)
    deps.insert(idx - 1, new_dep)

    new_dep
  end

  # Loads the given string as a gem. Execution is deferred until
  # after the logger has been instantiated and the framework directory
  # structure is defined.
  #
  # If that has already happened, the gem will be activated
  # immediately, but it will still be registered.
  # 
  # ==== Parameters
  # name<String> The name of the gem to load.
  # *ver<Gem::Requirement, Gem::Version, Array, #to_str>
  #   Version requirements to be passed to Gem::Dependency.new.
  #   If the last argument is a Hash, extract the :immediate option,
  #   forcing a dependency to load immediately.
  #
  # ==== Options
  #
  # :immediate   when true, gem is loaded immediately even if framework is not yet ready.
  # :require_as  file name to require for this gem.
  #
  # See examples below.
  #
  # ==== Notes
  #
  # If block is given, it is called after require is called. If you use a block to
  # require multiple files, require first using :require_as option and the rest
  # in the block.
  #
  # ==== Examples
  #
  # Usage scenario is typically one of the following:
  #
  # 1. Gem name and loaded file names are the same (ex.: amqp gem uses amqp.rb).
  #    In this case no extra options needed.
  #
  # dependency "amqp"
  #
  # 2. Gem name is different from the file needs to be required
  #    (ex.: ParseTree gem uses parse_tree.rb as main file).
  #
  # dependency "ParseTree", :require_as => "parse_tree"
  #
  # 3. You need to require a number of files from the library explicitly
  #    (ex.: cherry pick features from xmpp4r). Pass an array to :require_as.
  #
  # dependency "xmpp4r", :require_as => %w(xmpp4r/client xmpp4r/sasl xmpp4r/vcard)
  #
  # 4. You need to require a specific version of the gem.
  #
  # dependency "RedCloth", "3.0.4"
  #
  # 5. You want to load dependency as soon as the method is called.
  #
  # dependency "syslog", :immediate => true
  #
  # 6. You need to execute some arbitraty code after dependency is loaded:
  #
  # dependency "ruby-growl" do
  #   g = Growl.new "localhost", "ruby-growl",
  #              ["ruby-growl Notification"]
  #   g.notify "ruby-growl Notification", "Ruby-Growl is set up",
  #         "Ruby-Growl is set up"
  # end
  #
  # When specifying a gem version to use, you can use the same syntax RubyGems
  # support, for instance, >= 3.0.2 or >~ 1.2.
  #
  # See rubygems.org/read/chapter/16 for a complete reference.
  #
  # ==== Returns
  # Gem::Dependency:: The dependency information.
  #
  # :api: public
  def dependency(name, *opts, &blk)
    immediate = opts.last.delete(:immediate) if opts.last.is_a?(Hash)
    if immediate || Merb::BootLoader.finished?(Merb::BootLoader::Dependencies)
      load_dependency(name, caller, *opts, &blk)
    else
      track_dependency(name, caller, *opts, &blk)
    end
  end

  # Loads the given string as a gem.
  #
  # This new version tries to load the file via ROOT/gems first before moving
  # off to the system gems (so if you have a lower version of a gem in
  # ROOT/gems, it'll still get loaded).
  #
  # @param name<String,Gem::Dependency> 
  #   The name or dependency object of the gem to load.
  # @param *ver<Gem::Requirement, Gem::Version, Array, #to_str>
  #   Version requirements to be passed to Gem.activate.
  #
  # @note
  #   If the gem cannot be found, the method will attempt to require the string
  #   as a library.
  #
  # @return <Gem::Dependency> The dependency information.
  #
  # :api: private
  def load_dependency(name, clr, *ver, &blk)
    begin
      dep = name.is_a?(Gem::Dependency) ? name : track_dependency(name, clr, *ver, &blk)
      return unless dep.require_as
      Gem.activate(dep)
    rescue Gem::LoadError => e
      e.set_backtrace dep.original_caller
      Merb.fatal! "The gem #{name}, #{ver.inspect} was not found", e
    end
  
    begin
      require dep.require_as
    rescue LoadError => e
      e.set_backtrace dep.original_caller
      Merb.fatal! "The file #{dep.require_as} was not found", e
    end

    if block = dep.require_block
      # reset the require block so it doesn't get called a second time
      dep.require_block = nil
      block.call
    end

    Merb.logger.verbose!("loading gem '#{dep.name}' ...")
    return dep # ensure needs explicit return
  end

  # Loads both gem and library dependencies that are passed in as arguments.
  # Execution is deferred to the Merb::BootLoader::Dependencies.run during bootup.
  #
  # ==== Parameters
  # *args<String, Hash, Array> The dependencies to load.
  #
  # ==== Returns
  # Array[(Gem::Dependency, Array[Gem::Dependency])]:: Gem::Dependencies for the
  #   dependencies specified in args.
  #
  # :api: public
  def dependencies(*args)
    args.map do |arg|
      case arg
      when String then dependency(arg)
      when Hash   then arg.map { |r,v| dependency(r, v) }
      when Array  then arg.map { |r|   dependency(r)    }
      end
    end
  end

  # Loads both gem and library dependencies that are passed in as arguments.
  #
  # @param *args<String, Hash, Array> The dependencies to load.
  #
  # @note
  #   Each argument can be:
  #   String:: Single dependency.
  #   Hash::
  #     Multiple dependencies where the keys are names and the values versions.
  #   Array:: Multiple string dependencies.
  #
  # @example dependencies "RedCloth"                 # Loads the the RedCloth gem
  # @example dependencies "RedCloth", "merb_helpers" # Loads RedCloth and merb_helpers
  # @example dependencies "RedCloth" => "3.0"        # Loads RedCloth 3.0
  #
  # :api: private
  def load_dependencies(*args)
    args.map do |arg|
      case arg
      when String then load_dependency(arg)
      when Hash   then arg.map { |r,v| load_dependency(r, v) }
      when Array  then arg.map { |r|   load_dependency(r)    }
      end
    end
  end

  # Does a basic require, and prints a message if an error occurs.
  #
  # @param library<to_s> The library to attempt to include.
  # @param message<String> The error to add to the log upon failure. Defaults to nil.
  #
  # :api: private
  # @deprecated
  def rescue_require(library, message = nil)
    Merb.logger.warn("Deprecation warning: rescue_require is deprecated")
    sleep 2.0
    require library
  rescue LoadError, RuntimeError
    Merb.logger.error!(message) if message
  end

  # Used in Merb.root/config/init.rb to tell Merb which ORM (Object Relational
  # Mapper) you wish to use. Currently Merb has plugins to support
  # ActiveRecord, DataMapper, and Sequel.
  #
  # ==== Parameters
  # orm<Symbol>:: The ORM to use.
  #
  # ==== Returns
  # nil
  #
  # ==== Example
  #   use_orm :datamapper
  #
  #   # This will use the DataMapper generator for your ORM
  #   $ merb-gen model ActivityEvent
  #
  # ==== Notes
  #   If for some reason this is called more than once, latter
  #   call takes over other.
  #
  # :api: public
  def use_orm(orm, &blk)
    begin
      Merb.orm = orm
      orm_plugin = "merb_#{orm}"
      Kernel.dependency(orm_plugin, &blk)
    rescue LoadError => e
      Merb.logger.warn!("The #{orm_plugin} gem was not found.  You may need to install it.")
      raise e
    end
    nil
  end

  # Used in Merb.root/config/init.rb to tell Merb which testing framework to
  # use. Currently Merb has plugins to support RSpec and Test::Unit.
  #
  # ==== Parameters
  # test_framework<Symbol>::
  #   The test framework to use. Currently only supports :rspec and :test_unit.
  #
  # ==== Returns
  # nil
  #
  # ==== Example
  #   use_test :rspec
  #
  #   # This will now use the RSpec generator for tests
  #   $ merb-gen model ActivityEvent
  #
  # :api: public
  def use_testing_framework(test_framework, *test_dependencies)
    Merb.test_framework = test_framework
    
    Kernel.dependencies test_dependencies if Merb.env == "test" || Merb.env.nil?
    nil
  end

  def use_test(*args)
    use_testing_framework(*args)
  end
  
  # Used in Merb.root/config/init.rb to tell Merb which template engine to
  # prefer.
  #
  # ==== Parameters
  # template_engine<Symbol>
  #   The template engine to use.
  #
  # ==== Returns
  # nil
  #
  # ==== Example
  #   use_template_engine :haml
  #
  #   # This will now use haml templates in generators where available.
  #   $ merb-gen resource_controller Project 
  #
  # :api: public
  def use_template_engine(template_engine, &blk)
    Merb.template_engine = template_engine

    if template_engine != :erb
      if template_engine.in?(:haml, :builder)
        template_engine_plugin = "merb-#{template_engine}"
      else
        template_engine_plugin = "merb_#{template_engine}"
      end
      Kernel.dependency(template_engine_plugin, &blk)
    end
    
    nil
  rescue LoadError => e
    Merb.logger.warn!("The #{template_engine_plugin} gem was not found.  You may need to install it.")
    raise e
  end


  # @param i<Fixnum> The caller number. Defaults to 1.
  #
  # @return <Array[Array]> The file, line and method of the caller.
  #
  # @example
  #   __caller_info__(1)
  #     # => ['/usr/lib/ruby/1.8/irb/workspace.rb', '52', 'irb_binding']
  #
  # :api: private
  def __caller_info__(i = 1)
    file, line, meth = caller[i].scan(/(.*?):(\d+):in `(.*?)'/).first
  end

  # @param file<String> The file to read.
  # @param line<Fixnum> The line number to look for.
  # @param size<Fixnum>
  #   Number of lines to include above and below the the line to look for.
  #   Defaults to 4.
  #
  # @return <Array[Array]>
  #   Triplets containing the line number, the line and whether this was the
  #   searched line.
  #
  # @example
  #   __caller_lines__('/usr/lib/ruby/1.8/debug.rb', 122, 2) # =>
  #     [
  #       [ 120, "  def check_suspend",                               false ],
  #       [ 121, "    return if Thread.critical",                     false ],
  #       [ 122, "    while (Thread.critical = true; @suspend_next)", true  ],
  #       [ 123, "      DEBUGGER__.waiting.push Thread.current",      false ],
  #       [ 124, "      @suspend_next = false",                       false ]
  #     ]
  #
  # :api: private
  def __caller_lines__(file, line, size = 4)
    line = line.to_i
    if file =~ /\(erubis\)/
      yield :error, "Template Error! Problem while rendering", false
    elsif !File.file?(file) || !File.readable?(file)
      yield :error, "File `#{file}' not available", false
    else
      lines = File.read(file).split("\n")
      first_line = (f = line - size - 1) < 0 ? 0 : f
      
      if first_line.zero?
        new_size = line - 1
        lines = lines[first_line, size + new_size + 1]
      else
        new_size = nil
        lines = lines[first_line, size * 2 + 1]
      end

      lines && lines.each_with_index do |str, index|
        line_n = index + line
        line_n = (new_size.nil?) ? line_n - size : line_n - new_size
        yield line_n, str.chomp
      end
    end
  end

  # Takes a block, profiles the results of running the block
  # specified number of times and generates HTML report.
  #
  # @param name<#to_s>
  #   The file name. The result will be written out to
  #   Merb.root/"log/#{name}.html".
  # @param min<Fixnum>
  #   Minimum percentage of the total time a method must take for it to be
  #   included in the result. Defaults to 1.
  #
  # @return <String>
  #   The result of the profiling.
  #
  # @note
  #   Requires ruby-prof (<tt>sudo gem install ruby-prof</tt>)
  #
  # @example
  #   __profile__("MyProfile", 5, 30) do
  #     rand(10)**rand(10)
  #     puts "Profile run"
  #   end
  #
  #   Assuming that the total time taken for #puts calls was less than 5% of the
  #   total time to run, #puts won't appear in the profile report.
  #   The code block will be run 30 times in the example above.
  #
  # :api: private
  def __profile__(name, min=1, iter=100)
    require 'ruby-prof' unless defined?(RubyProf)
    return_result = ''
    result = RubyProf.profile do
      iter.times{return_result = yield}
    end
    printer = RubyProf::GraphHtmlPrinter.new(result)
    path = File.join(Merb.root, 'log', "#{name}.html")
    File.open(path, 'w') do |file|
      printer.print(file, {:min_percent => min,
                      :print_file => true})
    end
    return_result
  end

  # Extracts an options hash if it is the last item in the args array. Used
  # internally in methods that take *args.
  #
  # @param args<Array> The arguments to extract the hash from.
  #
  # @example
  #   def render(*args,&blk)
  #     opts = extract_options_from_args!(args) || {}
  #     # [...]
  #   end
  #
  # :api: public
  def extract_options_from_args!(args)
    args.pop if (args.last.instance_of?(Hash) || args.last.instance_of?(Mash))
  end

  # Checks that the given objects quack like the given conditions.
  #
  # @param opts<Hash>
  #   Conditions to enforce. Each key will receive a quacks_like? call with the
  #   value (see Object#quacks_like? for details).
  #
  # @raise <ArgumentError>
  #   An object failed to quack like a condition.
  #
  # :api: public
  def enforce!(opts = {})
    opts.each do |k,v|
      raise ArgumentError, "#{k.inspect} doesn't quack like #{v.inspect}" unless k.quacks_like?(v)
    end
  end

  unless Kernel.respond_to?(:debugger)

    # Define debugger method so that code even works if debugger was not
    # requested. Drops a note to the logs that Debugger was not available.
    def debugger
      Merb.logger.info! "\n***** Debugger requested, but was not " +
        "available: Start server with --debugger " +
        "to enable *****\n"
    end
  end
  
end
