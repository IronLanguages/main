require 'tempfile'

module Merb

  class Request
    
    # :api: private
    attr_accessor :env, :route
    # :api: public
    attr_accessor :exceptions
    # :api: private
    attr_reader :route_params

    # by setting these to false, auto-parsing is disabled; this way you can
    # do your own parsing instead
    cattr_accessor :parse_multipart_params, :parse_json_params,
      :parse_xml_params
    self.parse_multipart_params = true
    self.parse_json_params = true
    self.parse_xml_params = true

    # Flash, and some older browsers can't use arbitrary
    # request methods -- i.e., are limited to GET/POST.
    # These user-agents can make POST requests in combination
    # with these overrides to participate fully in REST.
    # Common examples are _method or fb_sig_request_method
    # in the params, or an X-HTTP-Method-Override header
    cattr_accessor :http_method_overrides
    self.http_method_overrides = []

    # Initialize the request object.
    #
    # ==== Parameters
    # http_request<~params:~[], ~body:IO>::
    #   An object like an HTTP Request.
    #
    # :api: private
    def initialize(rack_env)
      @env  = rack_env
      @body = rack_env[Merb::Const::RACK_INPUT]
      @route_params = {}
    end

    # Returns the controller object for initialization and dispatching the
    # request.
    #
    # ==== Returns
    # Class:: The controller class matching the routed request,
    #   e.g. Posts.
    #
    # :api: private
    def controller
      unless params[:controller]
        raise ControllerExceptions::NotFound,
          "Route matched, but route did not specify a controller.\n" +
          "Did you forgot to add :controller => \"people\" or :controller " +
          "segment to route definition?\nHere is what's specified:\n" +
          route.inspect
      end
      path = [params[:namespace], params[:controller]].compact.join(Merb::Const::SLASH)
      controller = path.snake_case.to_const_string

      begin
        Object.full_const_get(controller)
      rescue NameError => e
        msg = "Controller class not found for controller `#{path}'"
        Merb.logger.warn!(msg)
        raise ControllerExceptions::NotFound, msg
      end
    end

    METHODS = %w{get post put delete head options}

    # ==== Returns
    # Symbol:: The name of the request method, e.g. :get.
    #
    # ==== Notes
    # If the method is post, then the blocks specified in
    # http_method_overrides will be checked for the masquerading method.
    # The block will get the controller yielded to it.  The first matching workaround wins.
    # To disable this behavior, set http_method_overrides = []
    #
    # :api: public
    def method
      @method ||= begin
        request_method = @env[Merb::Const::REQUEST_METHOD].downcase.to_sym
        case request_method
        when :get, :head, :put, :delete, :options
          request_method
        when :post
          m = nil
          self.class.http_method_overrides.each do |o|
            m ||= o.call(self); break if m
          end
          m.downcase! if m
          METHODS.include?(m) ? m.to_sym : :post
        else
          raise "Unknown REQUEST_METHOD: #{@env[Merb::Const::REQUEST_METHOD]}"
        end
      end
    end

    # create predicate methods for querying the REQUEST_METHOD
    # get? post? head? put? etc
    METHODS.each do |m|
      class_eval "def #{m}?() method == :#{m} end"
    end

    # ==== Notes
    # Find route using requested URI and merges route
    # parameters (:action, :controller and named segments)
    # into request params hash.
    #
    # :api: private
    def find_route!
      @route, @route_params = Merb::Router.route_for(self)
      params.merge! @route_params if @route_params.is_a?(Hash)
    end

    # ==== Notes
    # Processes the return value of a deferred router block
    # and returns the current route params for the current
    # request evaluation
    #
    # :api: private
    def _process_block_return(retval)
      # If the return value is an array, then it is a redirect
      # so we must set the request as a redirect and extract
      # the redirect params and return it as a hash so that the
      # dispatcher can handle it
      matched! if retval.is_a?(Array)
      retval
    end

    # Sets the request as matched. This will abort evaluating any
    # further deferred procs.
    #
    # :api: private
    def matched!
      @matched = true
    end

    # Checks whether or not the request has been matched to a route.
    #
    # :api: private
    def matched?
      @matched
    end

    # ==== Returns
    # (Array, Hash):: the route params for the matched route.
    #
    # ==== Notes
    # If the response is an Array then it is considered a direct Rack response
    # to be sent back as a response. Otherwise, the route_params is a Hash with
    # routing data (controller, action, et al).
    #
    # :api: private
    def rack_response
      @route_params
    end

    # If @route_params is an Array, then it will be the rack response.
    # In this case, the request is considered handled.
    #
    # ==== Returns
    # Boolean:: true if @route_params is an Array, false otherwise.
    #
    # :api: private
    def handled?
      @route_params.is_a?(Array)
    end

    # == Params
    #
    # Handles processing params from raw data and merging them together to get
    # the final request params.

    private

    # ==== Returns
    # Hash:: Parameters passed from the URL like ?blah=hello.
    #
    # :api: private
    def query_params
      @query_params ||= Merb::Parse.query(query_string || '')
    end

    # Parameters passed in the body of the request. Ajax calls from
    # prototype.js and other libraries pass content this way.
    #
    # ==== Returns
    # Hash:: The parameters passed in the body.
    #
    # :api: private
    def body_params
      @body_params ||= begin
        if content_type && content_type.match(Merb::Const::FORM_URL_ENCODED_REGEXP) # or content_type.nil?
          Merb::Parse.query(raw_post)
        end
      end
    end

    # ==== Returns
    # Mash::
    #   The parameters gathered from the query string and the request body,
    #   with parameters in the body taking precedence.
    #
    # :api: private
    def body_and_query_params
      # ^-- FIXME a better name for this method
      @body_and_query_params ||= begin
        h = query_params
        h.merge!(body_params) if body_params
        h.to_mash
      end
    end

    # ==== Raises
    # ControllerExceptions::MultiPartParseError::
    #   Unable to parse the multipart form data.
    #
    # ==== Returns
    # Hash:: The parsed multipart parameters.
    #
    # :api: private
    def multipart_params
      @multipart_params ||=
        begin
          # if the content-type is multipart
          # parse the multipart. Otherwise return {}
          if (Merb::Const::MULTIPART_REGEXP =~ content_type)
            Merb::Parse.multipart(@body, $1, content_length)
          else
            {}
          end
        rescue ControllerExceptions::MultiPartParseError => e
          @multipart_params = {}
          raise e
        end
    end

    # ==== Returns
    # Hash:: Parameters from body if this is a JSON request.
    #
    # ==== Notes
    # If the JSON object parses as a Hash, it will be merged with the
    # parameters hash.  If it parses to anything else (such as an Array, or
    # if it inflates to an Object) it will be accessible via the inflated_object
    # parameter.
    #
    # :api: private
    def json_params
      @json_params ||= begin
        if Merb::Const::JSON_MIME_TYPE_REGEXP.match(content_type)
          begin
            jobj = JSON.parse(raw_post)
            jobj = jobj.to_mash if jobj.is_a?(Hash)
          rescue JSON::ParserError
            jobj = Mash.new
          end
          jobj.is_a?(Hash) ? jobj : { :inflated_object => jobj }
        end
      end
    end

    # ==== Returns
    # Hash:: Parameters from body if this is an XML request.
    #
    # :api: private
    def xml_params
      @xml_params ||= begin
        if Merb::Const::XML_MIME_TYPE_REGEXP.match(content_type)
          Hash.from_xml(raw_post) rescue Mash.new
        end
      end
    end

    public

    # ==== Returns
    # Mash:: All request parameters.
    #
    # ==== Notes
    # The order of precedence for the params is XML, JSON, multipart, body and
    # request string.
    #
    # :api: public
    def params
      @params ||= begin
        h = body_and_query_params.merge(route_params)
        h.merge!(multipart_params) if self.class.parse_multipart_params && multipart_params
        h.merge!(json_params) if self.class.parse_json_params && json_params
        h.merge!(xml_params) if self.class.parse_xml_params && xml_params
        h
      end
    end

    # ==== Returns
    # String:: Returns the redirect message Base64 unencoded.
    #
    # :api: public
    def message
      return {} unless params[:_message]
      begin
        Marshal.load(Merb::Parse.unescape(params[:_message]).unpack("m").first)
      rescue ArgumentError, TypeError
        {}
      end
    end

    # ==== Notes
    # Resets the params to a nil value.
    #
    # :api: private
    def reset_params!
      @params = nil
    end

    # ==== Returns
    # String:: The raw post.
    #
    # :api: private
    def raw_post
      @body.rewind if @body.respond_to?(:rewind)
      @raw_post ||= @body.read
    end

    # ==== Returns
    # Boolean:: If the request is an XML HTTP request.
    #
    # :api: public
    def xml_http_request?
      not Merb::Const::XML_HTTP_REQUEST_REGEXP.match(@env[Merb::Const::HTTP_X_REQUESTED_WITH]).nil?
    end
    alias xhr? :xml_http_request?
    alias ajax? :xml_http_request?

    # ==== Returns
    # String:: The remote IP address.
    #
    # :api: public
    def remote_ip
      return @env[Merb::Const::HTTP_CLIENT_IP] if @env.include?(Merb::Const::HTTP_CLIENT_IP)

      if @env.include?(Merb::Const::HTTP_X_FORWARDED_FOR) then
        remote_ips = @env[Merb::Const::HTTP_X_FORWARDED_FOR].split(',').reject do |ip|
          ip =~ Merb::Const::LOCAL_IP_REGEXP
        end

        return remote_ips.first.strip unless remote_ips.empty?
      end

      return @env[Merb::Const::REMOTE_ADDR]
    end

    # ==== Returns
    # String::
    #   The protocol, i.e. either "https" or "http" depending on the
    #   HTTPS header.
    #
    # :api: public
    def protocol
      ssl? ? Merb::Const::HTTPS : Merb::Const::HTTP
    end

    # ==== Returns
    # Boolean::: True if the request is an SSL request.
    #
    # :api: public
    def ssl?
      @env[Merb::Const::UPCASE_HTTPS] == 'on' || @env[Merb::Const::HTTP_X_FORWARDED_PROTO] == Merb::Const::HTTPS
    end

    # ==== Returns
    # String:: The HTTP referer.
    #
    # :api: public
    def referer
      @env[Merb::Const::HTTP_REFERER]
    end

    # ==== Returns
    # String:: The full URI, including protocol and host
    #
    # :api: public
    def full_uri
      protocol + "://" + host + uri
    end

    # ==== Returns
    # String:: The request URI.
    #
    # :api: public
    def uri
      @env[Merb::Const::REQUEST_PATH] || @env[Merb::Const::REQUEST_URI] || path_info
    end

    # ==== Returns
    # String:: The HTTP user agent.
    #
    # :api: public
    def user_agent
      @env[Merb::Const::HTTP_USER_AGENT]
    end

    # ==== Returns
    # String:: The server name.
    #
    # :api: public
    def server_name
      @env[Merb::Const::SERVER_NAME]
    end

    # ==== Returns
    # String:: The accepted encodings.
    #
    # :api: private
    def accept_encoding
      @env[Merb::Const::HTTP_ACCEPT_ENCODING]
    end

    # ==== Returns
    # String:: The script name.
    #
    # :api: public
    def script_name
      @env[Merb::Const::SCRIPT_NAME]
    end

    # ==== Returns
    # String:: HTTP cache control.
    #
    # :api: public
    def cache_control
      @env[Merb::Const::HTTP_CACHE_CONTROL]
    end

    # ==== Returns
    # String:: The accepted language.
    #
    # :api: public
    def accept_language
      @env[Merb::Const::HTTP_ACCEPT_LANGUAGE]
    end

    # ==== Returns
    # String:: The server software.
    #
    # :api: public
    def server_software
      @env[Merb::Const::SERVER_SOFTWARE]
    end

    # ==== Returns
    # String:: Value of HTTP_KEEP_ALIVE.
    #
    # :api: public
    def keep_alive
      @env[Merb::Const::HTTP_KEEP_ALIVE]
    end

    # ==== Returns
    # String:: The accepted character sets.
    #
    # :api: public
    def accept_charset
      @env[Merb::Const::HTTP_ACCEPT_CHARSET]
    end

    # ==== Returns
    # String:: The HTTP version
    #
    # :api: private
    def version
      @env[Merb::Const::HTTP_VERSION]
    end

    # ==== Returns
    # String:: The gateway.
    #
    # :api: public
    def gateway
      @env[Merb::Const::GATEWAY_INTERFACE]
    end

    # ==== Returns
    # String:: The accepted response types. Defaults to "*/*".
    #
    # :api: private
    def accept
      @env[Merb::Const::HTTP_ACCEPT].blank? ? "*/*" : @env[Merb::Const::HTTP_ACCEPT]
    end

    # ==== Returns
    # String:: The HTTP connection.
    #
    # :api: private
    def connection
      @env[Merb::Const::HTTP_CONNECTION]
    end

    # ==== Returns
    # String:: The query string.
    #
    # :api: private
    def query_string
      @env[Merb::Const::QUERY_STRING]
    end

    # ==== Returns
    # String:: The request content type.
    #
    # :api: private
    def content_type
      @env[Merb::Const::UPCASE_CONTENT_TYPE]
    end

    # ==== Returns
    # Fixnum:: The request content length.
    #
    # :api: public
    def content_length
      @content_length ||= @env[Merb::Const::CONTENT_LENGTH].to_i
    end

    # ==== Returns
    # String::
    #   The URI without the query string. Strips trailing "/" and reduces
    #   duplicate "/" to a single "/".
    #
    # :api: public
    def path
      # Merb::Const::SLASH is /
      # Merb::Const::QUESTION_MARK is ?
      path = (uri.empty? ? Merb::Const::SLASH : uri.split(Merb::Const::QUESTION_MARK).first).squeeze(Merb::Const::SLASH)
      path = path[0..-2] if (path[-1] == ?/) && path.size > 1
      path
    end

    # ==== Returns
    # String:: The path info.
    #
    # :api: public
    def path_info
      @path_info ||= Merb::Parse.unescape(@env[Merb::Const::PATH_INFO])
    end

    # ==== Returns
    # Fixnum:: The server port.
    #
    # :api: public
    def port
      @env[Merb::Const::SERVER_PORT].to_i
    end

    # ==== Returns
    # String:: The full hostname including the port.
    #
    # :api: public
    def host
      @env[Merb::Const::HTTP_X_FORWARDED_HOST] || @env[Merb::Const::HTTP_HOST] ||
        @env[Merb::Const::SERVER_NAME]
    end

    # ==== Parameters
    # tld_length<Fixnum>::
    #   Number of domains levels to inlclude in the top level domain. Defaults
    #   to 1.
    #
    # ==== Returns
    # Array:: All the subdomain parts of the host.
    #
    # :api: public
    def subdomains(tld_length = 1)
      parts = host.split(Merb::Const::DOT)
      parts[0..-(tld_length+2)]
    end

    # ==== Parameters
    # tld_length<Fixnum>::
    #   Number of domains levels to inlclude in the top level domain. Defaults
    #   to 1.
    #
    # ==== Returns
    # String:: The full domain name without the port number.
    #
    # :api: public
    def domain(tld_length = 1)
      host.split(Merb::Const::DOT).last(1 + tld_length).join(Merb::Const::DOT).sub(/:\d+$/,'')
    end

    # ==== Returns
    # Value of If-None-Match request header.
    #
    # :api: private
    def if_none_match
      @env[Merb::Const::HTTP_IF_NONE_MATCH]
    end

    # ==== Returns
    # Value of If-Modified-Since request header.
    #
    # :api: private
    def if_modified_since
      if time = @env[Merb::Const::HTTP_IF_MODIFIED_SINCE]
        Time.rfc2822(time)
      end
    end

  end
end
