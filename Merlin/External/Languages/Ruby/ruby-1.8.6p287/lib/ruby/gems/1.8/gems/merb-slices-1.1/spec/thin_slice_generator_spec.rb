require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe Merb::Generators::ThinSliceGenerator do
  
  describe "templates" do
    
    before(:all) { FileUtils.rm_rf(slices_path / 'testing-thin') rescue nil }
    after(:all)  { FileUtils.rm_rf(slices_path / 'testing-thin') rescue nil }
    
    before do
      @generator = Merb::Generators::ThinSliceGenerator.new(slices_path, {}, 'testing-thin')
    end
    
    it "should create a number of templates" do
      @generator.invoke!
      files = Dir[slices_path / 'testing-thin' / '**' / '*'].map do |path| 
        path.relative_path_from(slices_path)
      end
      expected = [
        "testing-thin/application.rb", "testing-thin/lib", "testing-thin/lib/testing-thin", 
        "testing-thin/lib/testing-thin/merbtasks.rb", "testing-thin/lib/testing-thin/slicetasks.rb", 
        "testing-thin/lib/testing-thin.rb", "testing-thin/LICENSE", "testing-thin/public", 
        "testing-thin/public/javascripts", "testing-thin/public/javascripts/master.js", 
        "testing-thin/public/stylesheets", "testing-thin/public/stylesheets/master.css", 
        "testing-thin/Rakefile", "testing-thin/README", "testing-thin/stubs", 
        "testing-thin/stubs/application.rb", "testing-thin/TODO", "testing-thin/views", 
        "testing-thin/views/layout", "testing-thin/views/layout/testing_thin.html.erb", 
        "testing-thin/views/main", "testing-thin/views/main/index.html.erb"
      ]
      files.should == expected
    end
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
end