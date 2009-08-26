require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::MerbPluginGenerator do
  
  describe "templates" do
    
    before do
      @generator = Merb::Generators::MerbPluginGenerator.new('/tmp', {}, 'testing')
    end
    
    it_should_behave_like "named generator"
    
    it "should create a number of views"
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
end