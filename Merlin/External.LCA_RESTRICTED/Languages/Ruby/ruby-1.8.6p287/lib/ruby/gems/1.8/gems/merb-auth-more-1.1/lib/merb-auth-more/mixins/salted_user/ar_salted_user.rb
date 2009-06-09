class Merb::Authentication
  module Mixins
    module SaltedUser
      module ARClassMethods
        
        def self.extended(base)
          base.class_eval do
            
            validates_presence_of     :password,                   :if => :password_required?
            validates_presence_of     :password_confirmation,      :if => :password_required?
            validates_confirmation_of :password,                   :if => :password_required?
            
            before_save :encrypt_password
          end # base.class_eval
                  
        end # self.extended
        
        def authenticate(login, password)
          @u = find(:first, :conditions => ["#{Merb::Authentication::Strategies::Basic::Base.login_param} = ?", login])
          @u && @u.authenticated?(password) ? @u : nil
        end
      end # ARClassMethods
    end # SaltedUser
  end # Mixins
end # Merb::Authentication 
