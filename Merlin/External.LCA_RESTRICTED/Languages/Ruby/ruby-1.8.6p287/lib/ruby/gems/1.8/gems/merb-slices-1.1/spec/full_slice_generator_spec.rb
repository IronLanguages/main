require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe Merb::Generators::FullSliceGenerator do
  
  describe "templates" do
    
    before(:all) { FileUtils.rm_rf(slices_path / 'testing') rescue nil }
    after(:all)  { FileUtils.rm_rf(slices_path / 'testing') rescue nil }
    
    before do
      @generator = Merb::Generators::FullSliceGenerator.new(slices_path, {}, 'testing')
    end
    
    it "should create a number of templates" do
      @generator.invoke!
      files = Dir[slices_path / 'testing' / '**' / '*'].map do |path| 
        path.relative_path_from(slices_path)
      end
      expected = [
        "testing/app", "testing/app/controllers", "testing/app/controllers/application.rb", 
        "testing/app/controllers/main.rb", "testing/app/helpers", 
        "testing/app/helpers/application_helper.rb", "testing/app/views", 
        "testing/app/views/layout", "testing/app/views/layout/testing.html.erb", 
        "testing/app/views/main", "testing/app/views/main/index.html.erb", 
        "testing/config", "testing/config/init.rb", "testing/lib", "testing/lib/testing", 
        "testing/lib/testing/merbtasks.rb", "testing/lib/testing/slicetasks.rb", 
        "testing/lib/testing/spectasks.rb", "testing/lib/testing.rb", 
        "testing/LICENSE", "testing/public", "testing/public/javascripts", 
        "testing/public/javascripts/master.js", "testing/public/stylesheets", 
        "testing/public/stylesheets/master.css", "testing/Rakefile", 
        "testing/README", "testing/spec", "testing/spec/requests", 
        "testing/spec/requests/main_spec.rb", "testing/spec/spec_helper.rb", 
        "testing/spec/testing_spec.rb", "testing/stubs", "testing/stubs/app", 
        "testing/stubs/app/controllers", "testing/stubs/app/controllers/application.rb", 
        "testing/stubs/app/controllers/main.rb", "testing/TODO"
      ]
      diff = expected - files
      diff.should be_empty
    end
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
end