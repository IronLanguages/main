class Merb::Authentication
  module Strategies
    # To use the password strategies, it is expected that you will provide
    # an @authenticate@ method on your user class.  This should take two parameters
    # login, and password.  It should return nil or the user object.
    module Basic
      
      class Base < Merb::Authentication::Strategy
        abstract!
        
        # Overwrite this method to customize the field
        def self.password_param
          (Merb::Plugins.config[:"merb-auth"][:password_param] || :password).to_s.to_sym
        end
        
        # Overwrite this method to customize the field
        def self.login_param
          (Merb::Plugins.config[:"merb-auth"][:login_param] || :login).to_s.to_sym
        end
        
        def password_param
          @password_param ||= Base.password_param
        end
        
        def login_param
          @login_param ||= Base.login_param
        end
      end # Base      
    end # Password
  end # Strategies
end # Merb::Authentication