module Merb

  class Router
    # This entire class is private and should never be accessed outside of
    # Merb::Router and Behavior
    class Route #:nodoc:
      SEGMENT_REGEXP               = /(:([a-z](_?[a-z0-9])*))/
      OPTIONAL_SEGMENT_REGEX       = /^.*?([\(\)])/i
      SEGMENT_REGEXP_WITH_BRACKETS = /(:[a-z_]+)(\[(\d+)\])?/
      JUST_BRACKETS                = /\[(\d+)\]/
      SEGMENT_CHARACTERS           = "[^\/.,;?]".freeze

      # :api: private
      attr_reader :conditions, :params, :segments
      # :api: private
      attr_reader :index, :variables, :name
      # :api: private
      attr_accessor :fixation, :resource_identifiers

      # :api: private
      def initialize(conditions, params, deferred_procs, options = {})
        @conditions, @params = conditions, params

        if options[:redirects]
          @redirects         = true
          @redirect_status   = @params[:status]
          @redirect_url      = @params[:url]
          @defaults          = {}
        else
          @generatable       = true
          @defaults          = options[:defaults] || {}
        end
        
        @identifiers       = options[:identifiers]
        @deferred_procs    = deferred_procs
        @segments          = []
        @symbol_conditions = {}
        @placeholders      = {}
        compile
      end

      # :api: private
      def regexp?
        @regexp
      end
    
      # :api: private  
      def generatable?
        @generatable && !regexp?
      end

      # :api: private
      def allow_fixation?
        @fixation
      end
      
      # :api: private
      def to_s
        regexp? ?
          "/#{conditions[:path].source}/" :
          segment_level_to_s(segments)
      end
      
      alias_method :inspect, :to_s

      # Appends self to Merb::Router.routes

      # :api: private
      def register
        @index = Merb::Router.routes.size
        Merb::Router.routes << self
        self
      end
      
      # Inserts self to Merb::Router.routes at the specified index.
      # :api: private      
      def register_at(index)
        @index = index
        Merb::Router.routes.insert(index, self)
        self
      end
      
      # Sets the route as a resource route with the given key as the 
      # lookup key.
      # :api: private      
      def resource=(key)
        Router.resource_routes[key] = self
        key
      end
      
      # :api: private
      def name=(name)
        @name = name.to_sym
        Router.named_routes[@name] = self
        @name
      end
      
      # Generates the URL for the route given the passed arguments. The
      # method will first match the anonymous parameters to route params
      # and will convert all the parameters according to the specifed
      # object identifiers.
      #
      # Then the named parameters are passed to a compiled generation proc.
      #
      # ==== Parameters
      # args<Array>::
      #   The arguments passed to the public #url method with the name
      #   of the route removed. This is an array of the anonymous parameters
      #   followed by a hash containing the named parameters.
      #
      # defaults<Hash>::
      #   A hash of parameters to use to generate the route if there are
      #   any missing required parameters. This is usually the parameters
      #   for the current request
      #
      # ==== Returns
      # String:: The generated URL.
      #
      # :api: private
      def generate(args = [], defaults = {}, resource = false)
        unless generatable?
          raise GenerationError, "Cannot generate regexp Routes" if regexp?
          raise GenerationError, "Cannot generate this route"
        end
        
        params = extract_options_from_args!(args) || { }
        
        params.each do |k, v|
          params[k] = identify(v, k)
        end
        
        # Support for anonymous params
        unless args.empty?
          # First, let's determine which variables are missing
          variables = (resource ? @resource_identifiers : @variables) - params.keys
          
          args.each do |param|
            raise GenerationError, "The route has #{@variables.length} variables: #{@variables.inspect}" if variables.empty?
            
            if identifier = identifier_for(param) and identifier.is_a?(Array)
              identifier.each { |ident| params[variables.shift] = param.send(ident) }
            else
              params[variables.shift] ||= identify(param)
            end
          end
        end
        
        uri = @generator[params, defaults] or raise GenerationError, "Named route #{name} could not be generated with #{params.inspect}"
        uri = Merb::Config[:path_prefix] + uri if Merb::Config[:path_prefix]
        uri
      end
      
      # Identifies the object according to the identifiers set while building
      # the routes. Identifying an object means picking an instance method to
      # call on the object that will return a string representation of the
      # object for the route being generated. If the identifier is an array,
      # then a param_key must be present and match one of the elements of the
      # identifier array.
      #
      # param_keys that end in _id are treated slightly differently in order
      # to get nested resources to work correctly.
      #
      # :api: private
      def identify(obj, param_key = nil)
        identifier = identifier_for(obj)
        if identifier.is_a?(Array)
          # First check if the param_key exists as an identifier
          return obj.send(param_key) if identifier.include?(param_key)
          # If the param_key ends in _id, just return the object id
          return obj.id if "#{param_key}" =~ /_id$/
          # Otherwise, raise an error
          raise GenerationError, "The object #{obj.inspect} cannot be identified with #{identifier.inspect} for #{param_key}"
        else
          identifier ? obj.send(identifier) : obj
        end
      end
      
      # Returns the identifier for the passed object. Built in core ruby classes are
      # always identified with to_s. The method will return nil in that case (since
      # to_s is the default for objects that do not have identifiers.)
      #
      # :api: private
      def identifier_for(obj)
        return if obj.is_a?(String)    || obj.is_a?(Symbol)     || obj.is_a?(Numeric)  ||
                  obj.is_a?(TrueClass) || obj.is_a?(FalseClass) || obj.is_a?(NilClass) ||
                  obj.is_a?(Array)     || obj.instance_of?(Hash)
        
        @identifiers.each do |klass, identifier|
          return identifier if obj.is_a?(klass)
        end
        
        return nil
      end

      # Returns the if statement and return value for for the main
      # Router.match compiled method.
      #
      # :api: private
      def compiled_statement(first)
        els_if = first ? '  if ' : '  elsif '

        code = ""
        code << els_if << condition_statements.join(" && ") << "\n"
        
        # First, we need to always return the value of the
        # deferred block if it explicitly matched the route
        if @redirects
          code << "    return [#{@index.inspect}, block_result] if request.matched?" << "\n" if @deferred_procs.any?
          code << "    [#{@index.inspect}, Merb::Rack::Helpers.redirect(#{@redirect_url.inspect}, :status => #{@redirect_status.inspect})]" << "\n"
        elsif @deferred_procs.any?
          code << "    [#{@index.inspect}, block_result]" << "\n"
        else
          code << "    [#{@index.inspect}, #{params_as_string}]" << "\n"
        end
      end

    private
      
    # === Compilation ===

      # :api: private
      def compile
        compile_conditions
        compile_params
        @generator = Generator.new(@segments, @symbol_conditions, @identifiers).compiled
      end
      
      # The Generator class handles compiling the route down to a lambda that
      # can generate the URL from a params hash and a default params hash.
      class Generator #:nodoc:

        # :api: private        
        def initialize(segments, symbol_conditions, identifiers)
          @segments          = segments
          @symbol_conditions = symbol_conditions
          @identifiers       = identifiers
          @stack             = []
          @opt_segment_count = 0
          @opt_segment_stack = [[]]
        end
        
        #
        # :api: private
        def compiled
          ruby  = ""
          ruby << "lambda do |params, defaults|\n"
          ruby << "  fragment     = params.delete(:fragment)\n"
          ruby << "  query_params = params.dup\n"
          
          with(@segments) do
            ruby << "  include_defaults = true\n"
            ruby << "  return unless url = #{block_for_level}\n"
          end
          
          ruby << "  query_params.delete_if { |key, value| value.nil? }\n"
          ruby << "  unless query_params.empty?\n"
          ruby << '    url << "?#{Merb::Parse.params_to_query_string(query_params)}"' << "\n"
          ruby << "  end\n"
          ruby << '  url << "##{fragment}" if fragment' << "\n"
          ruby << "  url\n"
          ruby << "end\n"
          
          eval(ruby)
        end
        
      private
      
        # Cleans up methods a bunch. We don't need to pass the current segment
        # level around everywhere anymore. It's kept track for us in the stack.
        #
        # :api: private
        def with(segments, &block)
          @stack.push(segments)
          retval = yield
          @stack.pop
          retval
        end
  
        # :api: private  
        def segments
          @stack.last || []
        end

        # :api: private        
        def symbol_segments
          segments.flatten.select { |s| s.is_a?(Symbol)  }
        end

        # :api: private
        def current_segments
          segments.select { |s| s.is_a?(Symbol) }
        end

        # :api: private        
        def nested_segments
          segments.select { |s| s.is_a?(Array) }.flatten.select { |s| s.is_a?(Symbol) }
        end

        # :api: private      
        def block_for_level
          ruby  = ""
          ruby << "if #{segment_level_matches_conditions}\n"
          ruby << "  #{remove_used_segments_in_query_path}\n"
          ruby << "  #{generate_optional_segments}\n"
          ruby << %{ "#{combine_required_and_optional_segments}"\n}
          ruby << "end"
        end

        # :api: private        
        def check_if_defaults_should_be_included
          ruby = ""
          ruby << "include_defaults = "
          symbol_segments.each { |s| ruby << "params[#{s.inspect}] || " }
          ruby << "false"
        end

        # --- Not so pretty ---
        # :api: private        
        def segment_level_matches_conditions
          conditions = current_segments.map do |segment|
            condition = "(cached_#{segment} = params[#{segment.inspect}] || include_defaults && defaults[#{segment.inspect}])"

            if @symbol_conditions[segment] && @symbol_conditions[segment].is_a?(Regexp)
              condition << " && cached_#{segment}.to_s =~ #{@symbol_conditions[segment].inspect}"
            elsif @symbol_conditions[segment]
              condition << " && cached_#{segment}.to_s == #{@symbol_conditions[segment].inspect}"
            end

            condition
          end
          
          conditions << "true" if conditions.empty?
          conditions.join(" && ")
        end

        # :api: private
        def remove_used_segments_in_query_path
          "#{current_segments.inspect}.each { |s| query_params.delete(s) }"
        end

        # :api: private
        def generate_optional_segments
          optionals = []

          segments.each_with_index do |segment, i|
            if segment.is_a?(Array) && segment.any? { |s| !s.is_a?(String) }
              with(segment) do
                @opt_segment_stack.last << (optional_name = "_optional_segments_#{@opt_segment_count += 1}")
                @opt_segment_stack.push []
                optionals << "#{check_if_defaults_should_be_included}\n"
                optionals << "#{optional_name} = #{block_for_level}"
                @opt_segment_stack.pop
              end
            end
          end

          optionals.join("\n")
        end

        # :api: private
        def combine_required_and_optional_segments
          bits = ""

          segments.each_with_index do |segment, i|
            bits << case
              when segment.is_a?(String) then segment
              when segment.is_a?(Symbol) then '#{cached_' + segment.to_s + '}'
              when segment.is_a?(Array) && segment.any? { |s| !s.is_a?(String) } then "\#{#{@opt_segment_stack.last.shift}}"
              else ""
            end
          end

          bits
        end
        
      end

    # === Conditions ===

      # :api: private
      def compile_conditions
        @original_conditions = conditions.dup
        
        if conditions[:path] && !conditions[:path].empty?
          path = conditions[:path].flatten.compact
          if path = compile_path(path)
            conditions[:path] = Regexp.new("^#{path}$")
          else
            conditions.delete(:path)
          end
        else
          # If there is no path, we can't generate it
          @generatable = false
        end
      end

      # The path is passed in as an array of different parts. We basically have
      # to concat all the parts together, then parse the path and extract the
      # variables. However, if any of the parts are a regular expression, then
      # we abort the parsing and just convert it to a regexp.
      #
      # :api: private      
      def compile_path(path)
        @segments = []
        compiled  = ""

        return nil if path.nil? || path.empty?

        path.each do |part|
          case part
          when Regexp
            @regexp   = true
            @segments = []
            compiled << part.source.sub(/^\^/, '').sub(/\$$/, '')
          when String
            segments = segments_with_optionals_from_string(part.dup)
            compile_path_segments(compiled, segments)
            # Concat the segments
            unless regexp?
              if @segments[-1].is_a?(String) && segments[0].is_a?(String)
                @segments[-1] << segments.shift
              end
              @segments.concat segments
            end
          else
            raise ArgumentError.new("A route path can only be specified as a String or Regexp")
          end
        end
        
        unless regexp?
          @variables = @segments.flatten.select { |s| s.is_a?(Symbol)  }
          compiled.gsub!(%r[/+], '/')
          compiled.gsub!(%r[(.+)/$], '\1')
        end

        compiled
      end

      # Simple nested parenthesis parser
      #
      # :api: private      
      def segments_with_optionals_from_string(path, nest_level = 0)
        segments = []

        # Extract all the segments at this parenthesis level
        while segment = path.slice!(OPTIONAL_SEGMENT_REGEX)
          # Append the segments that we came across so far
          # at this level
          segments.concat segments_from_string(segment[0..-2]) if segment.length > 1
          # If the parenthesis that we came across is an opening
          # then we need to jump to the higher level
          if segment[-1,1] == '('
            segments << segments_with_optionals_from_string(path, nest_level + 1)
          else
            # Throw an error if we can't actually go back down (aka syntax error)
            raise "There are too many closing parentheses" if nest_level == 0
            return segments
          end
        end

        # Save any last bit of the string that didn't match the original regex
        segments.concat segments_from_string(path) unless path.empty?

        # Throw an error if the string should not actually be done (aka syntax error)
        raise "You have too many opening parentheses" unless nest_level == 0

        segments
      end

      # :api: private
      def segments_from_string(path)
        segments = []

        while match = (path.match(SEGMENT_REGEXP))
          segments << match.pre_match unless match.pre_match.empty?
          segments << match[2].intern
          path = match.post_match
        end
        
        raise Router::Behavior::Error, "cannot use :path as a route placeholder" if segments.include?(:path)

        segments << path unless path.empty?
        segments
      end

      # --- Yeah, this could probably be refactored
      # :api: private
      def compile_path_segments(compiled, segments)
        segments.each do |segment|
          case segment
          when String
            compiled << Regexp.escape(segment)
          when Symbol
            condition = (@symbol_conditions[segment] ||= @conditions.delete(segment))
            compiled << compile_segment_condition(condition)
            # Create a param for the Symbol segment if none already exists
            @params[segment] = "#{segment.inspect}" unless @params.has_key?(segment)
            @placeholders[segment] ||= capturing_parentheses_count(compiled)
          when Array
            compiled << "(?:"
            compile_path_segments(compiled, segment)
            compiled << ")?"
          else
            raise ArgumentError, "conditions[:path] segments can only be a Strings, Symbols, or Arrays"
          end
        end
      end

      # Handles anchors in Regexp conditions
      # :api: private
      def compile_segment_condition(condition)
        return "(#{SEGMENT_CHARACTERS}+)" unless condition
        return "(#{condition})"           unless condition.is_a?(Regexp)

        condition = condition.source
        # Handle the start anchor
        condition = if condition =~ /^\^/
          condition[1..-1]
        else
          "#{SEGMENT_CHARACTERS}*#{condition}"
        end
        # Handle the end anchor
        condition = if condition =~ /\$$/
          condition[0..-2]
        else
          "#{condition}#{SEGMENT_CHARACTERS}*"
        end

        "(#{condition})"
      end

      # :api: private
      def compile_params
        # Loop through each param and compile it
        @defaults.merge(@params).each do |key, value|
          if value.nil?
            @params.delete(key)
          elsif value.is_a?(String)
            @params[key] = compile_param(value)
          else
            @params[key] = value.inspect
          end
        end
      end

      # This was pretty much a copy / paste from the old router
      # :api: private
      def compile_param(value)
        result = []
        match  = true
        while match
          if match = SEGMENT_REGEXP_WITH_BRACKETS.match(value)
            result << match.pre_match.inspect unless match.pre_match.empty?
            placeholder_key = match[1][1..-1].intern
            if match[2] # has brackets, e.g. :path[2]
              result << "#{placeholder_key}#{match[3]}"
            else # no brackets, e.g. a named placeholder such as :controller
              if place = @placeholders[placeholder_key]
                # result << "(path#{place} || )" # <- Defaults
                with_defaults  = ["(path#{place}"]
                with_defaults << " || #{@defaults[placeholder_key].inspect}" if @defaults[placeholder_key]
                with_defaults << ")"
                result << with_defaults.join
              else
                raise GenerationError, "Placeholder not found while compiling routes: #{placeholder_key.inspect}. Add it to the conditions part of the route."
              end
            end
            value = match.post_match
          elsif match = JUST_BRACKETS.match(value)
            result << match.pre_match.inspect unless match.pre_match.empty?
            result << "path#{match[1]}"
            value = match.post_match
          else
            result << value.inspect unless value.empty?
          end
        end

        result.join(' + ').gsub("\\_", "_")
      end

      # :api: private
      def condition_statements
        statements = []

        # First, let's build the conditions for the regular 
        conditions.each_pair do |key, value|
          statements << case value
          when Regexp
            captures = ""

            if (max = capturing_parentheses_count(value)) > 0
              captures << (1..max).to_a.map { |n| "#{key}#{n}" }.join(", ")
              captures << " = "
              captures << (1..max).to_a.map { |n| "$#{n}" }.join(", ")
            end

            # Note: =~ is slightly faster than .match
            %{(#{value.inspect} =~ cached_#{key} #{' && ((' + captures + ') || true)' unless captures.empty?})}
          when Array
            %{(#{arrays_to_regexps(value).inspect} =~ cached_#{key})}
          else
            %{(cached_#{key} == #{value.inspect})}
          end
        end
        
        # The first one is special, so let's extract it
        if first = @deferred_procs.first
          deferred = ""
          deferred << "(block_result = "
          deferred <<   "request._process_block_return("
          deferred <<     "#{first}.call(request, #{params_as_string})"
          deferred <<   ")"
          deferred << ")"
          
          # Let's build the rest of them now
          if @deferred_procs.length > 1
            deferred << deferred_condition_statement(@deferred_procs[1..-1])
          end
          
          statements << deferred
        end

        statements
      end
      
      # (request.matched? || ((block_result = process(proc.call))))
      # :api: private      
      def deferred_condition_statement(deferred)
        if current = deferred.first
          html  = " && (request.matched? || ("
          html <<   "(block_result = "
          html <<     "request._process_block_return("
          html <<       "#{current}.call(request, block_result)"
          html <<     ")"
          html <<   ")"
          html <<   "#{deferred_condition_statement(deferred[1..-1])}"
          html << "))"
        end
      end

      # :api: private
      def params_as_string
        elements = params.keys.map do |k|
          "#{k.inspect} => #{params[k]}"
        end
        "{#{elements.join(', ')}}"
      end

    # ---------- Utilities ----------
      
      # :api: private      
      def arrays_to_regexps(condition)
        return condition unless condition.is_a?(Array)
        
        source = condition.map do |value|
          value = if value.is_a?(Regexp)
            value.source
          else
            "^#{Regexp.escape(value.to_s)}$"
          end
          "(?:#{value})"
        end
        
        Regexp.compile(source.join('|'))
      end
    
      # :api: private    
      def segment_level_to_s(segments)
        (segments || []).inject('') do |str, seg|
          str << case seg
            when String then seg
            when Symbol then ":#{seg}"
            when Array  then "(#{segment_level_to_s(seg)})"
          end
        end
      end

      # :api: private
      def capturing_parentheses_count(regexp)
        regexp = regexp.source if regexp.is_a?(Regexp)
        regexp.scan(/(?!\\)[(](?!\?[#=:!>-imx])/).length
      end
    end
  end  
end
