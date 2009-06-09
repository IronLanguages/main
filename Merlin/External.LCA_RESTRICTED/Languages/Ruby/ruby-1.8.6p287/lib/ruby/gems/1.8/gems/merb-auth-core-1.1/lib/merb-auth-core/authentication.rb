module Merb
  class Authentication
    module Strategies; end
    include Extlib::Hook
    attr_accessor :session
    attr_writer   :error_message
  
    class DuplicateStrategy < Exception; end
    class MissingStrategy < Exception; end
    class NotImplemented < Exception; end
  
    # This method returns the default user class to use throughout the
    # merb-auth authentication framework.  Merb::Authentication.user_class can
    # be used by other plugins, and by default by strategies.
    #
    # By Default it is set to User class.  If you need a different class
    # The intention is that you overwrite this method
    #
    # @return <User Class Object>
    #
    # @api overwritable
    cattr_accessor :user_class
  
    def initialize(session)
      @session = session
    end
  
    # Returns true if there is an authenticated user attached to this session
    #
    # @return <TrueClass|FalseClass>
    # 
    def authenticated?
      !!session[:user]
    end
  
    # This method will retrieve the user object stored in the session or nil if there
    # is no user logged in.
    # 
    # @return <User class>|NilClass
    def user
      return nil if !session[:user]
      @user ||= fetch_user(session[:user])
    end
  
    # This method will store the user provided into the session
    # and set the user as the currently logged in user
    # @return <User Class>|NilClass
    def user=(user)
      session[:user] = nil && return if user.nil?
      session[:user] = store_user(user)
      @user = session[:user] ? user : session[:user]  
    end
  
    # The workhorse of the framework.  The authentiate! method is where
    # the work is done.  authenticate! will try each strategy in order
    # either passed in, or in the default_strategy_order.  
    #
    # If a strategy returns some kind of user object, this will be stored
    # in the session, otherwise a Merb::Controller::Unauthenticated exception is raised
    #
    # @params Merb::Request, [List,Of,Strategies, optional_options_hash]
    #
    # Pass in a list of strategy objects to have this list take precedence over the normal defaults
    # 
    # Use an options hash to provide an error message to be passed into the exception.
    #
    # @return user object of the verified user.  An exception is raised if no user is found
    #
    def authenticate!(request, params, *rest)
      opts = rest.last.kind_of?(Hash) ? rest.pop : {}
      rest = rest.flatten
      
      strategies = if rest.empty?
        if request.session[:authentication_strategies] 
          request.session[:authentication_strategies]
        else
          Merb::Authentication.default_strategy_order
        end
      else
        request.session[:authentication_strategies] ||= []
        request.session[:authentication_strategies] << rest
        request.session[:authentication_strategies].flatten!.uniq!
        request.session[:authentication_strategies]
      end
    
      msg = opts[:message] || error_message
      user = nil    
      # This one should find the first one that matches.  It should not run antother
      strategies.detect do |s|
        s = Merb::Authentication.lookup_strategy[s] # Get the strategy from string or class
        unless s.abstract?
          strategy = s.new(request, params)
          user = strategy.run! 
          if strategy.halted?
            self.headers, self.status, self.body = [strategy.headers, strategy.status, strategy.body]
            halt!
            return
          end
          user
        end
      end
      
      # Check after callbacks to make sure the user is still cool
      user = run_after_authentication_callbacks(user, request, params) if user
      
      # Finally, Raise an error if there is no user found, or set it in the session if there is.
      raise Merb::Controller::Unauthenticated, msg unless user
      session[:authentication_strategies] = nil # clear the session of Failed Strategies if login is successful      
      self.user = user
    end
  
    # "Logs Out" a user from the session.  Also clears out all session data
    def abandon!
      @user = nil
      session.clear
    end
  
    # A simple error message mechanism to provide general information.  For more specific information
    #
    # This message is the default message passed to the Merb::Controller::Unauthenticated exception
    # during authentication.  
    #
    # This is a very simple mechanism for error messages.  For more detailed control see Authenticaiton#errors
    #
    # @api overwritable
    def error_message
      @error_message || "Could not log in"
    end
  
    # Tells the framework how to store your user object into the session so that it can be re-created 
    # on the next login.  
    # You must overwrite this method for use in your projects.  Slices and plugins may set this.
    #
    # @api overwritable
    def store_user(user)
      raise NotImplemented
    end
  
    # Tells the framework how to reconstitute a user from the data stored by store_user.
    #
    # You must overwrite this method for user in your projects.  Slices and plugins may set this.
    #
    # @api overwritable
    def fetch_user(session_contents = session[:user])
      raise NotImplemented
    end
    
    # Keeps track of strategies by class or string
    # When loading from string, strategies are loaded withing the Merb::Authentication::Strategies namespace
    # When loaded by class, the class is stored directly
    # @private
    def self.lookup_strategy
      @strategy_lookup || reset_strategy_lookup!
    end
    
    # Restets the strategy lookup.  Useful in specs
    def self.reset_strategy_lookup!
      @strategy_lookup = Mash.new do |h,k| 
        case k
        when Class
          h[k] = k
        when String, Symbol
          h[k] = Merb::Authentication::Strategies.full_const_get(k.to_s) 
        end
      end
    end
    
    # Maintains a list of keys to maintain when needing to keep some state 
    # in the face of session.abandon! You need to maintain this state yourself
    # @public
    def self.maintain_session_keys
      @maintain_session_keys ||= [:authentication_strategies]
    end
    
    private
    def run_after_authentication_callbacks(user, request, params)
      Merb::Authentication.after_callbacks.each do |cb|
        user = case cb
        when Proc
          cb.call(user, request, params)
        when Symbol, String
          user.send(cb)
        end
        break unless user
      end
      user
    end
    
  end # Merb::Authentication
end # Merb
