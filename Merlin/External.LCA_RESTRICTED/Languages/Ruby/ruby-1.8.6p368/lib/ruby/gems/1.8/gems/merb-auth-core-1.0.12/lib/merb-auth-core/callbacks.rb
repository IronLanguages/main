module Merb
  class Authentication
    cattr_accessor :after_callbacks
    @@after_callbacks = []
    
    # Use the after_authentication callbacks to setup things that should occur after the
    # user is authenticated.  
    #
    # Pass in symbols, procs and or a block to the method to setup the callbacks.
    # Callbacks are executed in the order they are added. 
    #
    # @params 
    # <*callbacks:[Symbol|Proc]> The callback.. Symbol == method on the user object
    #                                           Proc will be passed the user, request and param objects
    # <&block> A block to check.  The user, request and params will be passed into the block
    #
    # To confirm that the user is still eligable to login, simply return the user from
    # the method or block.  To stop the user from being authenticated return false or nil
    #
    # ====Example
    #
    #    Merb::Authentication.after_authentication do |user,request,params|
    #       user.active? ? user : nil
    #    end
    #
    # @api public
    
    def self.after_authentication(*callbacks, &block)
      self.after_callbacks = after_callbacks + callbacks.flatten unless callbacks.blank?
      after_callbacks << block if block_given?
    end
    
  end
end