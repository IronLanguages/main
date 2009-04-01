module Merb::Generators
  
  class ControllerGenerator < NamespacedGenerator

    def self.source_root
      File.join(super, 'component', 'controller')
    end
    
    desc <<-DESC
      Generates a new controller.
    DESC
    
    option :testing_framework, :desc => 'Testing framework to use (one of: rspec, test_unit)'
    option :template_engine, :desc => 'Template engine to use (one of: erb, haml, markaby, etc...)'
    
    first_argument :name, :required => true, :desc => "controller name"
    
    invoke :helper
    
    template :controller do |template|
      template.source = 'app/controllers/%file_name%.rb'
      template.destination = "app/controllers" / base_path / "#{file_name}.rb"
    end
    
    template :index_erb, :template_engine => :erb do |template|
      template.source = 'app/views/%file_name%/index.html.erb'
      template.destination = "app/views" / base_path / "#{file_name}/index.html.erb"
    end
    
    template :controller_spec, :testing_framework => :rspec do |template|
      template.source = 'spec/requests/%file_name%_spec.rb'
      template.destination = "spec/requests" / base_path / "#{file_name}_spec.rb"
    end
    
    template :controller_test_unit, :testing_framework => :test_unit do |template|
      template.source = 'test/requests/%file_name%_test.rb'
      template.destination = "test/requests" / base_path / "#{file_name}_test.rb"
    end

    def after_generation
      STDOUT.puts "\n\nDon't forget to add request/controller tests first."
    end
  end
  
  add :controller, ControllerGenerator
end
