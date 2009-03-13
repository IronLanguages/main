module Merb

  class Cookies < Mash
  
    # :api: private
    def initialize(constructor = {})
      @_options_lookup  = Mash.new
      @_cookie_defaults = { "domain" => Merb::Controller._default_cookie_domain, "path" => '/' }
      super constructor
    end
    
    # Implicit assignment of cookie key and value.
    #
    # ==== Parameters
    # name<~to_s>:: Name of the cookie.
    # value<~to_s>:: Value of the cookie.
    #
    # ==== Notes
    # By using this method, a cookie key is marked for being
    # included in the Set-Cookie response header.
    #
    # :api: public
    def []=(key, value)
      @_options_lookup[key] ||= {}
      super
    end
    
    # Explicit assignment of cookie key, value and options
    #
    # ==== Parameters
    # name<~to_s>:: Name of the cookie.
    # value<~to_s>:: Value of the cookie.
    # options<Hash>:: Additional options for the cookie (see below).
    #
    # ==== Options (options)
    # :path<String>:: The path for which this cookie applies. Defaults to "/".
    # :expires<Time>:: Cookie expiry date.
    # :domain<String>:: The domain for which this cookie applies.
    # :secure<Boolean>:: Security flag.
    #
    # ==== Notes
    # By using this method, a cookie key is marked for being
    # included in the Set-Cookie response header.
    #
    # :api: private
    def set_cookie(name, value, options = {})
      @_options_lookup[name] = options
      self[name] = value
    end
    
    # Removes the cookie on the client machine by setting the value to an empty
    # string and setting its expiration date into the past.
    #
    # ==== Parameters
    # name<~to_s>:: Name of the cookie to delete.
    # options<Hash>:: Additional options to pass to +set_cookie+.
    #
    # :api: public
    def delete(name, options = {})
      set_cookie(name, "", options.merge("expires" => Time.at(0)))
    end
    
    # Generate any necessary headers.
    #
    # ==== Returns
    # Hash:: The headers to set, or an empty array if no cookies are set.
    #
    # :api: private
    def extract_headers(controller_defaults = {})
      defaults = @_cookie_defaults.merge(controller_defaults)
      cookies = []
      self.each do |name, value|
        # Only set cookies that marked for inclusion in the response header. 
        next unless @_options_lookup[name]
        options = defaults.merge(@_options_lookup[name])
        if (expiry = options["expires"]).respond_to?(:gmtime)
          options["expires"] = expiry.gmtime.strftime(Merb::Const::COOKIE_EXPIRATION_FORMAT)
        end
        secure  = options.delete("secure")
        kookie  = "#{name}=#{Merb::Parse.escape(value)}; "
        # WebKit in particular doens't like empty cookie options - skip them.
        options.each { |k, v| kookie << "#{k}=#{v}; " unless v.blank? }
        kookie  << 'secure' if secure
        cookies << kookie.rstrip
      end
      cookies.empty? ? {} : { 'Set-Cookie' => cookies }
    end
    
  end
  
  module CookiesMixin
    
    def self.included(base)
      # Allow per-controller default cookie domains (see callback below)
      base.class_inheritable_accessor :_default_cookie_domain
      base._default_cookie_domain = Merb::Config[:default_cookie_domain]
      
      # Add a callback to enable Set-Cookie headers
      base._after_dispatch_callbacks << lambda do |c|
        headers = c.request.cookies.extract_headers("domain" => c._default_cookie_domain)
        c.headers.update(headers)
      end
    end
    
    # ==== Returns
    # Merb::Cookies::
    #   A new Merb::Cookies instance representing the cookies that came in
    #   from the request object
    #
    # ==== Notes
    # Headers are passed into the cookie object so that you can do:
    #   cookies[:foo] = "bar"
    #
    # :api: public
    def cookies
      request.cookies
    end
    
    module RequestMixin
            
      # ==== Returns
      # Hash:: The cookies for this request.
      #
      # ==== Notes
      # If a method #default_cookies is defined it will be called. This can
      # be used for session fixation purposes for example. The method returns
      # a Hash of key => value pairs.
      #
      # :api: public
      def cookies
        @cookies ||= begin
          values  = Merb::Parse.query(@env[Merb::Const::HTTP_COOKIE], ';,')
          cookies = Merb::Cookies.new(values)
          cookies.update(default_cookies) if respond_to?(:default_cookies)
          cookies
        end
      end
      
    end   
    
  end
  
end
