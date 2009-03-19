# make sure we're running inside Merb
if defined?(Merb::Plugins)
  # Merb gives you a Merb::Plugins.config hash...feel free to put your stuff in your piece of it
  Merb::Plugins.config[:"merb-auth_more"] = {
    :chickens => false
  }
  
  # Register the strategies so that plugins and apps may utilize them
  basic_path = File.expand_path(File.dirname(__FILE__)) / "merb-auth-more" / "strategies" / "basic"
  
  Merb::Authentication.register(:default_basic_auth,    basic_path / "basic_auth.rb")
  Merb::Authentication.register(:default_openid,        basic_path / "openid.rb")
  Merb::Authentication.register(:default_password_form, basic_path / "password_form.rb")
  
  Merb::BootLoader.before_app_loads do
    # require code that must be loaded before the application
  end
  
  Merb::BootLoader.after_app_loads do
    # code that can be required after the application loads
  end
  
  Merb::Plugins.add_rakefiles "merb-auth-more/merbtasks"
end