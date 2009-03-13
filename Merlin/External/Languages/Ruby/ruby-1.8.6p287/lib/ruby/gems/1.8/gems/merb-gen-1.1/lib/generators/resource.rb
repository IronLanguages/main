module Merb::Generators
  
  class ResourceGenerator < Generator
    
    desc <<-DESC
      Generates a new resource.
    DESC
    
    first_argument :name, :required => true, :desc => "resource name (singular)"
    second_argument :attributes, :as => :hash, :default => {}, :desc => "space separated resource model properties in form of name:type. Example: state:string"

    option :testing_framework, :desc => 'Testing framework to use (one of: rspec, test_unit)'
    option :orm, :desc => 'Object-Relation Mapper to use (one of: none, activerecord, datamapper, sequel)'
    
    invoke :model do |generator|
      generator.new(destination_root, options, model_name, attributes)
    end
    
    invoke :resource_controller do |generator|
      generator.new(destination_root, options, controller_name, attributes)
    end
    
    def controller_name
      name.pluralize
    end
    
    def model_name
      name
    end

    def after_generation
      STDOUT << message("resources :#{model_name.pluralize.snake_case} route added to config/router.rb")
    end
  end
  
  add :resource, ResourceGenerator
end
