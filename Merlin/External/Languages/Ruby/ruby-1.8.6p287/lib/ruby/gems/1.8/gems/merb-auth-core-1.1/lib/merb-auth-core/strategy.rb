module Merb
  class Authentication
    cattr_reader   :strategies, :default_strategy_order, :registered_strategies
    @@strategies, @@default_strategy_order, @@registered_strategies = [], [], {}
  
    # Use this to set the default order of strategies 
    # if you need to in your application.  You don't need to use all avaiable strategies
    # in this array, but you may not include a strategy that has not yet been defined.
    # 
    # @params [Merb::Authentiation::Strategy,Merb::Authentication::Strategy]
    # 
    # @public
    def self.default_strategy_order=(*order)
      order = order.flatten
      bad = order.select{|s| !s.ancestors.include?(Strategy)}
      raise ArgumentError, "#{bad.join(",")} do not inherit from Merb::Authentication::Strategy" unless bad.empty?
      @@default_strategy_order = order
    end
  
    # Allows for the registration of strategies.
    # @params <Symbol, String>
    #   +label+ The label is the label to identify this strategy
    #   +path+  The path to the file containing the strategy.  This must be an absolute path!
    # 
    # Registering a strategy does not add it to the list of strategies to use
    # it simply makes it available through the Merb::Authentication.activate method
    # 
    # This is for plugin writers to make a strategy availalbe but this should not
    # stop you from declaring your own strategies
    # 
    # @plugin
    def self.register(label, path)
      self.registered_strategies[label] = path
    end

    # Activates a registered strategy by it's label.
    # Intended for use with plugin authors.  There is little
    # need to register your own strategies.  Just declare them
    # and they will be active.
    def self.activate!(label)
      path = self.registered_strategies[label]
      raise "The #{label} Strategy is not registered" unless path
      require path
    end
  
    # The Merb::Authentication::Strategy is where all the action happens in the merb-auth framework.
    # Inherit from this class to setup your own strategy.  The strategy will automatically
    # be placed in the default_strategy_order array, and will be included in the strategy runs.
    #
    # The strategy you implment should have a YourStrategy#run! method defined that returns
    #   1. A user object if authenticated
    #   2. nil if no authenticated user was found.
    #
    # === Example
    #
    #    class MyStrategy < Merb::Authentication::Strategy
    #      def run!
    #        u = User.get(params[:login])
    #        u if u.authentic?(params[:password])
    #      end
    #    end
    #
    #
    class Strategy
      attr_accessor :request
      attr_writer   :body
    
      class << self
        def inherited(klass)
          Merb::Authentication.strategies << klass
          Merb::Authentication.default_strategy_order << klass
        end
    
        # Use this to declare the strategy should run before another strategy
        def before(strategy)
          order =  Merb::Authentication.default_strategy_order
          order.delete(self)
          index = order.index(strategy)
          order.insert(index,self)
        end
    
        # Use this to declare the strategy should run after another strategy
        def after(strategy)
          order = Merb::Authentication.default_strategy_order
          order.delete(self)
          index = order.index(strategy)
          index == order.size ? order << self : order.insert(index + 1, self)
        end
      
        # Mark a strategy as abstract.  This means that a strategy will not
        # ever be run as part of the authentication.  Instead this 
        # will be available to inherit from as a way to share code.
        # 
        # You could for example setup a strategy to check for a particular kind of login
        # and then have a subclass for each class type of user in your system.
        # i.e. Customer / Staff, Student / Staff etc
        def abstract!
          @abstract = true
        end
    
        # Asks is this strategy abstract. i.e. can it be run as part of the authentication
        def abstract?
          !!@abstract
        end
    
      end # End class << self
    
      def initialize(request, params)
        @request = request
        @params  = params
      end
    
      # An alias to the request.params hash
      # Only rely on this hash to find any router params you are looking for.
      # If looking for paramteres use request.params
      def params
        @params
      end
    
      # An alials to the request.cookies hash
      def cookies
        request.cookies
      end
    
      # An alias to the request.session hash
      def session
        request.session
      end
    
      # Redirects causes the strategy to signal a redirect
      # to the provided url.
      # 
      # ====Parameters
      # url<String>:: The url to redirect to
      # options<Hash>:: An options hash with the following keys:
      #   +:permanent+ Set this to true to make the redirect permanent
      #   +:status+ Set this to an integer for the status to return
      def redirect!(url, opts = {})
        self.headers["Location"] = url
        self.status = opts[:permanent] ? 301 : 302
        self.status = opts[:status] if opts[:status]
        self.body   = opts[:message] || "<div>You are being redirected to <a href='#{url}'>#{url}</a></div>"
        halt!
        return true
      end
    
      # Returns ture if the strategy redirected
      def redirected?
        !!headers["Location"]
      end
      
      # Provides a place to put the status of the response
      attr_accessor :status
    
      # Provides a place to put headers
      def headers
        @headers ||={}
      end
      
      # Mark this strategy as complete for this request.  Will cause that no other
      # strategies will be executed.  
      def halt!
        @halt = true
      end
      
      # Checks to see if this strategy has been halted
      def halted?
        !!@halt
      end
      
      
      # Allows you to provide a body of content to return when halting
      def body
        @body || ""
      end
    
      # This is the method that is called as the test for authentication and is where
      # you put your code.
      # 
      # You must overwrite this method in your strategy
      #
      # @api overwritable
      def run!
        raise NotImplemented
      end
    
      # Overwrite this method to scope a strategy to a particular user type
      # you can use this with inheritance for example to try the same strategy
      # on different user types
      #
      # By default, Merb::Authentication.user_class is used.  This method allows for 
      # particular strategies to deal with a different type of user class.
      #
      # For example.  If Merb::Authentication.user_class is Customer
      # and you have a PasswordStrategy, you can subclass the PasswordStrategy
      # and change this method to return Staff.  Giving you a PasswordStrategy strategy
      # for first Customer(s) and then Staff.  
      #
      # @api overwritable
      def user_class
        Merb::Authentication.user_class
      end
      
    end # Strategy
  end # Merb::Authentication
end # Merb
