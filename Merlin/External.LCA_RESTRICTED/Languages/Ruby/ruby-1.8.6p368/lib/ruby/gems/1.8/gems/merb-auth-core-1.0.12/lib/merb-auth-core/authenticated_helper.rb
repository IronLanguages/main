module Merb
  class Controller::Unauthenticated < ControllerExceptions::Unauthorized; end
  
  module AuthenticatedHelper  
    protected
    # This is the main method to use as a before filter.  You can call it with options
    # and strategies to use.  It will check if a user is logged in, and failing that
    # will run through either specified.  
    # 
    # @params all are optional.  A list of strategies, optionally followed by a 
    # options hash.  
    #
    # If used with no options, or only the hash, the default strategies will be used
    # see Authentictaion.default_strategy_order.  
    #
    # If a list of strategies is passed in, the default strategies are ignored, and
    # the passed in strategies are used in order until either one is found, or all fail.  
    #
    # A failed login will result in an Unauthenticated exception being raised.  
    #
    # Use the :message key in the options hash to pass in a failure message to the
    # exception.
    # 
    # === Example
    #
    #    class MyController < Application
    #      before :ensure_authenticated, :with => [OpenID,FormPassword, :message => "Failz!"]
    #       #... <snip>
    #    end
    # 
    def ensure_authenticated(*strategies)
      session.authenticate!(request, params, *strategies) unless session.authenticated?
      auth = session.authentication
      if auth.halted?
        self.headers.merge!(auth.headers)
        self.status  = auth.status
        throw :halt, auth.body
      end
      session.user
    end 
  end
end
