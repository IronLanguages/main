module Merb::Generators
  
  class FullSliceGenerator < BaseSliceGenerator
    
    def initialize(*args)
      Merb.disable(:initfile)
      super
    end

    def self.source_root
      File.join(File.dirname(__FILE__), 'templates', 'full')
    end
    
    glob!
    
    common_template :javascript,  'public/javascripts/master.js'
    common_template :stylesheet,  'public/stylesheets/master.css'
    
    common_template :license,     'LICENSE'
    
    common_template :merbtasks,   'lib/%base_name%/merbtasks.rb'
    common_template :slicetasks,  'lib/%base_name%/slicetasks.rb'
    common_template :spectasks,   'lib/%base_name%/spectasks.rb'
    
    first_argument :name, :required => true
    
    option :testing_framework, :default => :rspec,
                               :desc => 'Testing framework to use (one of: rspec, test_unit).'
    
    def destination_root
      File.join(@destination_root, base_name)
    end
    
  end
  
  add_private :full_slice, FullSliceGenerator
  
end