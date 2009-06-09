module Merb
  module Session
      
    # Access to the authentication object directly.  Particularly useful
    # for accessing the errors.
    # 
    # === Example
    #
    #    <%= error_messages_for session.authentication %>
    # 
    def authentication
      @authentication ||= Merb::Authentication.new(self)
    end
  
    # Check to see if the current session is authenticated
    # @return true if authenticated.  false otherwise
    def authenticated?
      authentication.authenticated?
    end
  
    # Authenticates the session via the authentication object. 
    #
    # See Merb::Authentication#authenticate for usage
    def authenticate!(request, params, *rest)
      authentication.authenticate!(request, params, *rest)
    end
  
    # Provides access to the currently authenticated user.
    def user
      authentication.user
    end
  
    # set the currently authenticated user manually
    # Merb::Authentication#store_user should know how to store the object into the session
    def user=(the_user)
      authentication.user = the_user
    end
  
    # Remove the user from the session and clear all data.
    def abandon!
      authentication.abandon!
    end
  
  end # Session
end # Merb