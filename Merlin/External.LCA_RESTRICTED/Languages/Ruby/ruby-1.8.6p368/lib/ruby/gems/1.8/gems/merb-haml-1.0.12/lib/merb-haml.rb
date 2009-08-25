# make sure we're running inside Merb
if defined?(Merb)  
  require "haml"
  require "merb-haml/template"
  Merb::Plugins.add_rakefiles(File.join(File.dirname(__FILE__) / "merb-haml" / "merbtasks"))
  
  Merb::Plugins.config[:sass] ||= {}

  Merb::BootLoader.after_app_loads do
    
    if File.directory?(Merb::Plugins.config[:sass][:template_location] || Merb.dir_for(:stylesheet) / "sass")
      require "sass/plugin" 
      if Merb::Config[:sass]
        Merb.logger.info("Please define your sass settings in Merb::Plugins.config[:sass] not Merb::Config")
        Sass::Plugin.options = Merb::Config[:sass]
      else
        Sass::Plugin.options = Merb::Plugins.config[:sass]
      end
    end
    
  end
  
  # Hack because Haml uses symbolize_keys
  class Hash
    def symbolize_keys!
      self
    end
  end
  
  generators = File.join(File.dirname(__FILE__), 'generators')
  Merb.add_generators generators / "resource_controller"
  Merb.add_generators generators / "controller"
  Merb.add_generators generators / "layout"
end
