module Merb::Generators
  
  class SliceGenerator < Generator
    
    option :thin, :as => :boolean, :desc => 'Generates a thin slice'
    option :very_thin, :as => :boolean, :desc => 'Generates an even thinner slice'
    
    desc <<-DESC
      Generates a merb slice.
    DESC

    def initialize(*args)
      Merb.disable(:initfile)
      super
    end
    
    first_argument :name, :required => true
    
    invoke :full_slice, :thin => nil, :very_thin => nil
    invoke :thin_slice, :thin => true
    invoke :very_thin_slice, :very_thin => true
    
  end
  
  class BaseSliceGenerator < NamedGenerator
    
    def self.common_template(name, template_source)
      common_base_dir = File.expand_path(File.dirname(__FILE__))
      template name do |t|
        t.source = File.join(common_base_dir, 'templates', 'common', template_source)
        t.destination = template_source
      end
    end
    
  end
  
  add :slice, SliceGenerator
  
end
