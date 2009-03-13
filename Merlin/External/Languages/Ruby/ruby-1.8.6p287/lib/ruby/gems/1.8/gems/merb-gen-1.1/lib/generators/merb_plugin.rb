module Merb::Generators
  
  class MerbPluginGenerator < NamedGenerator
    
    def initialize(*args)
      Merb.disable(:initfile)
      super
    end

    def self.source_root
      File.join(super, 'application', 'merb_plugin')
    end
    
    option :testing_framework, :default => :rspec, :desc => 'Testing framework to use (one of: rspec, test_unit)'
    option :orm, :default => :none, :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel)'
    option :bin, :as => :boolean # TODO: explain this
    
    desc <<-DESC
      Generates a new Merb plugin.
    DESC
    
    glob!
    
    first_argument :name, :required => true, :desc => "Plugin name"
    
    def destination_root
      File.join(@destination_root, base_name)
    end
    
  end
  
  add :plugin, MerbPluginGenerator
  
end
