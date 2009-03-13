module Merb::AuthenticationMixin
  
  # Attempts to authenticate the user via HTTP Basic authentication. Takes a
  # block with the username and password, if the block yields false the
  # authentication is not accepted and :halt is thrown.
  #
  # If no block is passed, +basic_authentication+, the +request+ and +authenticate+
  # methods can be chained. These can be used to independently request authentication
  # or confirm it, if more control is desired.
  #
  # ==== Parameters
  # realm<~to_s>:: The realm to authenticate against. Defaults to 'Application'.
  # &authenticator:: A block to check if the authentication is valid.
  #
  # ==== Returns
  # Merb::AuthenticationMixin::BasicAuthentication
  #
  # ==== Examples
  #     class Application < Merb::Controller
  #     
  #       before :authenticate
  #     
  #       protected
  #     
  #       def authenticate
  #         basic_authentication("My App") do |username, password|
  #           password == "secret"
  #         end
  #       end
  #     
  #     end
  #
  #     class Application < Merb::Controller
  #     
  #       before :authenticate
  #     
  #       def authenticate
  #         user = basic_authentication.authenticate do |username, password|
  #           User.authenticate(username, password)
  #         end
  #     
  #         if user
  #           @current_user = user
  #         else
  #           basic_authentication.request
  #         end
  #       end
  #     
  #     end
  #
  # If you need to request basic authentication inside an action you need to use the request! method.
  #
  # ==== Example
  #
  #    class Sessions < Application
  #  
  #      def new
  #        case content_type
  #        when :html
  #          render
  #
  #        else
  #         user = basic_authentication.authenticate do |username, password|
  #           User.authenticate(username, password)
  #         end
  #
  #         if user
  #           display(user)
  #         else
  #           basic_authentication.request
  #         end
  #        end
  #      end
  # 
  #    end 
  #
  #
  # :api: public
  def basic_authentication(realm = "Application", &authenticator)
    @_basic_authentication ||= BasicAuthentication.new(self, realm, &authenticator)
  end
  
  class BasicAuthentication
    # So we can have access to the status codes
    include Merb::ControllerExceptions

    # :api: private
    def initialize(controller, realm = "Application", &authenticator)
      @controller = controller
      @realm = realm
      @auth = Rack::Auth::Basic::Request.new(@controller.request.env)
      authenticate_or_request(&authenticator) if authenticator
    end

    # Determines whether or not the user is authenticated using the criteria
    # in the provided authenticator block.
    #
    # ==== Parameters
    # &authenticator:: A block that decides whether the provided username and password
    #   are valid.
    #
    # ==== Returns
    # Object:: False if basic auth is not provided, otherwise the return value of the authenticator block.
    # 
    # @overridable
    # :api: public
    def authenticate(&authenticator)
      if @auth.provided? and @auth.basic?
        authenticator.call(*@auth.credentials)
      else
        false
      end
    end

    # Request basic authentication and halt the filter chain. This is for use in a before filter.
    #
    # ==== Throws
    # :halt with an "HTTP Basic: Access denied." message with no layout, and sets the status to Unauthorized.
    #
    # :api: public
    def request
      request!
      throw :halt, @controller.render("HTTP Basic: Access denied.\n", :status => Unauthorized.status, :layout => false)
    end
    
    # Sets headers to request basic auth.
    #
    # ==== Returns
    # String:: Returns the empty string to provide a response body.
    #
    # :api: public
    def request!
      @controller.status = Unauthorized.status
      @controller.headers['WWW-Authenticate'] = 'Basic realm="%s"' % @realm
      ""
    end
    
    # ==== Returns
    # Boolean:: Whether there has been any basic authentication credentials provided
    #
    # :api: public
    def provided?
      @auth.provided?
    end
    
    # ==== Returns
    # String:: The username provided in the request.
    #
    # :api: public
    def username
      provided? ? @auth.credentials.first : nil
    end
    
    # ==== Returns
    # String:: The password provided in the request.
    #
    # :api: public
    def password
      provided? ? @auth.credentials.last : nil
    end
    
    protected
    
    # :api: private
    def authenticate_or_request(&authenticator)
      authenticate(&authenticator) || request
    end
    
  end

end
