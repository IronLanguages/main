class Merb::Authentication
  module Mixins
    module SaltedUser
      module RDBClassMethods
        
        def self.extended(base)
          base.class_eval do
 
            property :crypted_password
            property :salt
 
            before_save :password_checks
            
            def password_checks
              if password_required?
                return false unless !password.blank? && password == password_confirmation
              end
              encrypt_password
              true
            end
            
          end
        end
 
        def authenticate(login, password)
          login_param = Merb::Authentication::Strategies::Basic::Base.login_param
          @u = all.sorted_by(login_param) { |q| q.key(login) }.first
          @u && @u.authenticated?(password) ? @u : nil
        end 
                 
      end # RDBClassMethods
    end # SaltedUser
  end # Mixins
end # Merb::Authentication