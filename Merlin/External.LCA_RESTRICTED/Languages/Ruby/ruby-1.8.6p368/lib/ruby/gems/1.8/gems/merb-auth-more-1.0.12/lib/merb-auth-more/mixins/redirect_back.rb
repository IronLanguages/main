# Impelments redirect_back_or.  i.e. remembers where you've come from on a failed login
# and stores this inforamtion in the session.  When you're finally logged in you can use
# the redirect_back_or helper to redirect them either back where they came from, or a pre-defined url.
# 
# Here's some examples:
#
#  1. User visits login form and is logged in
#     - redirect to the provided (default) url
#
#  2. User vists a page (/page) and gets kicked to login (raised Unauthenticated)
#     - On successful login, the user may be redirect_back_or("/home") and they will 
#       return to the /page url.  The /home  url is ignored
#
#

#
module Merb::AuthenticatedHelper
  
  # Add a helper to do the redirect_back_or  for you.  Also tidies up the session afterwards
  # If there has been a failed login attempt on some page using this method
  # you'll be redirected back to that page.  Otherwise redirect to the default_url
  #
  # To make sure you're not redirected back to the login page after a failed then successful login,
  # you can include an ignore url.  Basically, if the return url == the ignore url go to the default_url
  #
  # set the ignore url via an :ignore option in the opts hash.
  def redirect_back_or(default_url, opts = {})
    if !session[:return_to].blank? && ![opts[:ignore]].flatten.include?(session[:return_to].first)
      redirect session[:return_to].first, opts
      session[:return_to] = nil
    else
      redirect default_url, opts
    end
    "Redirecting to <a href='#{default_url}'>#{default_url}</a>"
  end
  
end

# This mixin is mixed into the Exceptions controller to setup the correct methods
# And filters.  It is implemented as a mixin so that it is completely overwritable in 
# your controllers
module Merb::Authentication::Mixins
  module RedirectBack
    def self.included(base)
      base.class_eval do  
        after  :_set_return_to,   :only => :unauthenticated
      end
    end
    
    private   
    def _set_return_to
      unless request.exceptions.blank?
        session[:return_to] ||= []
        session[:return_to] << request.uri
        session[:return_to]
      end
    end

  end # RedirectBack
end # Merb::Authentication::Mixins

# Adds required methods to  the Authentication object for redirection
Merb::BootLoader.after_app_loads do
  Merb::Authentication.maintain_session_keys << :return_to
end
# class Merb::Authentication
# 
#   def return_to_url
#     @return_to_url ||= session[:return_to]
#   end
#   
#   def return_to_url=(return_url)
#     @return_to_url = session[:return_to] = return_url
#   end
# end

# Mixin the RedirectBack mixin before the after_app_loads block (i.e. make sure there is an exceptions controller)
Merb::Authentication.customize_default do
  Exceptions.class_eval{ include Merb::Authentication::Mixins::RedirectBack }
end