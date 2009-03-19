module Merb::Generators
  class MailerGenerator < NamespacedGenerator
 
    def self.source_root
      File.dirname(__FILE__) / 'templates' / 'mailer'
    end
    
    desc <<-DESC
      Generates a mailer
    DESC
    
    option :testing_framework, :desc => 'Testing framework to use (one of: rspec, test_unit)'
    
    first_argument :name, :required => true, :desc => "mailer name"
    
    template :mailer do |t|
      t.source = 'app/mailers/%file_name%_mailer.rb'
      t.destination = File.join("app/mailers", base_path, "#{file_name}_mailer.rb")
    end
    
    template :notify_on_event do |t|
      t.source = 'app/mailers/views/%file_name%_mailer/notify_on_event.text.erb'
      t.destination = File.join("app/mailers/views", base_path, "#{file_name}_mailer/notify_on_event.text.erb")
    end
    
    template :controller_spec, :testing_framework => :rspec do |t|
      t.source = 'spec/mailers/%file_name%_mailer_spec.rb'
      t.destination = File.join("spec/mailers", base_path, "#{file_name}_mailer_spec.rb")
    end
 
  end
 
  add :mailer, MailerGenerator
end
