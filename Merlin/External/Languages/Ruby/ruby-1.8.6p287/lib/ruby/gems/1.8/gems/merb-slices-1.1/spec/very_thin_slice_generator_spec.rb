require File.dirname(__FILE__) + '/spec_helper'

slices_path = File.dirname(__FILE__) / 'slices'

describe Merb::Generators::VeryThinSliceGenerator do
  
  describe "templates" do
    
    before(:all) { FileUtils.rm_rf(slices_path / 'testing-very-thin') rescue nil }
    after(:all)  { FileUtils.rm_rf(slices_path / 'testing-very-thin') rescue nil }
    
    before do
      @generator = Merb::Generators::VeryThinSliceGenerator.new(slices_path, {}, 'testing-very-thin')
    end
    
    it "should create a number of templates" do
      @generator.invoke!
      files = Dir[slices_path / 'testing-very-thin' / '**' / '*'].map do |path| 
        path.relative_path_from(slices_path)
      end
      expected = [
        "testing-very-thin/application.rb", "testing-very-thin/lib", 
        "testing-very-thin/lib/testing-very-thin", 
        "testing-very-thin/lib/testing-very-thin/merbtasks.rb", 
        "testing-very-thin/lib/testing-very-thin/slicetasks.rb", 
        "testing-very-thin/lib/testing-very-thin.rb", 
        "testing-very-thin/LICENSE", "testing-very-thin/Rakefile", 
        "testing-very-thin/README", "testing-very-thin/TODO"
      ]
      files.should == expected
    end
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
end