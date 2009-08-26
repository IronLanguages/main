require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::MerbCoreGenerator do
  
  describe "templates" do
    
    before do
      @generator = Merb::Generators::MerbCoreGenerator.new('/tmp', {}, 'testing')
    end
    
    it "should create an init.rb" do
      @generator.should create('/tmp/testing/config/init.rb')
    end
    
    it "should have an application controller" do
      @generator.should create('/tmp/testing/app/controllers/application.rb')
    end
    
    it "should have an exceptions controller" do
      @generator.should create('/tmp/testing/app/controllers/exceptions.rb')
    end

    it "should have a gitignore file" do
      @generator.should create('/tmp/testing/.gitignore')
    end
    
    it "should have an htaccess file" do
      @generator.should create('/tmp/testing/public/.htaccess')
    end
    
    it "should create a number of views"
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
end
