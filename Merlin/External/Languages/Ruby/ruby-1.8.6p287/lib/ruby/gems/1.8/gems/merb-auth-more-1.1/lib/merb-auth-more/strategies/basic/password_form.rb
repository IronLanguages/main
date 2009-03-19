require 'merb-auth-more/strategies/abstract_password'
# This strategy uses a login  and password parameter.
#
# Overwrite the :password_param, and :login_param
# to return the name of the field (on the form) that you're using the 
# login with.  These can be strings or symbols
#
# == Required
#
# === Methods
# <User>.authenticate(login_param, password_param)
#
class Merb::Authentication
  module Strategies
    module Basic
      class Form < Base
        
        def run!
          if (login = request.params[login_param]) && (password = request.params[password_param])
            user = user_class.authenticate(login, password)
            if !user
              errors = request.session.authentication.errors
              errors.clear!
              errors.add(login_param, strategy_error_message)
            end
            user
          end
        end # run!
        
        def strategy_error_message
          "#{login_param.to_s.capitalize} or #{password_param.to_s.capitalize} were incorrect"
        end
        
      end # Form
    end # Password
  end # Strategies
end # Authentication