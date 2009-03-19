require 'merb-auth-more/strategies/abstract_password'
# This strategy is used with basic authentication in Merb.
#
# == Requirements
#
# == Methods
#   <User>.authenticate(login_field, password_field)
#
class Merb::Authentication
  module Strategies
    module Basic
      class BasicAuth < Base
        
        def run!
          if basic_authentication?
            basic_authentication do |login, password|
              user = user_class.authenticate(login, password) 
              unless user
                request_basic_auth!
              end
              user
            end
          end
        end
        
        def self.realm
          @realm ||= "Application"
        end
        
        cattr_writer :realm
        def realm
          @realm ||= self.class.realm
        end
        
        cattr_accessor :failure_message
        @@failure_message = "Login Required"
        
        private
        def initialize(request, params)
          super
          @auth = Rack::Auth::Basic::Request.new(request.env)
        end
        
        def basic_authentication?
          @auth.provided? and @auth.basic?
        end
        
        def username
          basic_authentication? ? @auth.credentials.first : nil
        end

        def password
          basic_authentication? ? @auth.credentials.last : nil
        end
        
        def request_basic_auth!
          self.status = Merb::Controller::Unauthorized.status
          self.headers['WWW-Authenticate'] = 'Basic realm="%s"' % realm
          self.body = self.class.failure_message
          halt!
        end
        
        def basic_authentication(realm = nil, &authenticator)
          self.realm = realm if realm
          if basic_authentication?
            authenticator.call(*@auth.credentials)
          else
            false
          end
        end
        
        
      end # BasicAuth
    end # Password
  end # Strategies
end # Merb::Authentication