module Merb::Generators
  
  class VeryThinSliceGenerator < BaseSliceGenerator
    
    def self.source_root
      File.join(File.dirname(__FILE__), 'templates', 'very_thin')
    end
    
    glob!
    
    common_template :application, 'application.rb'
    
    common_template :rakefile,    'Rakefile'
    common_template :license,     'LICENSE'
    common_template :todo,        'TODO'
    
    common_template :merbtasks,   'lib/%base_name%/merbtasks.rb'
    common_template :slicetasks,  'lib/%base_name%/slicetasks.rb'
    
    first_argument :name, :required => true
    
    option :testing_framework, :default => :rspec,
                               :desc => 'Testing framework to use (one of: rspec, test_unit).'
    
    def destination_root
      File.join(@destination_root, base_name)
    end
    
  end
  
  add_private :very_thin_slice, VeryThinSliceGenerator
  
end