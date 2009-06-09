require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::ControllerGenerator do

  before(:each) do
    @generator = Merb::Generators::ControllerGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "namespaced generator"
  
  it "should create a controller" do
    @generator.should create('/tmp/app/controllers/stuff.rb')
  end
  
  it "should create a view" do
    @generator.should create('/tmp/app/views/stuff/index.html.erb')
  end
  
  describe "with rspec" do
    
    it "should create a controller spec" do
      @generator.should create('/tmp/spec/requests/stuff_spec.rb')
    end
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
  describe "with test_unit" do
    
    before do
      @generator = Merb::Generators::ControllerGenerator.new('/tmp', { :testing_framework => :test_unit }, 'Stuff')
    end
    
    it "should create a controller test" do
      @generator.should create('/tmp/test/requests/stuff_test.rb')
    end
    
    it "should render templates successfully" do
      lambda { @generator.render! }.should_not raise_error
    end
    
  end
  
  describe "with a namespace" do
    
    before(:each) do
      @generator = Merb::Generators::ControllerGenerator.new('/tmp', {}, 'John::Monkey::Stuff')
    end
    
    it "should create a controller" do
      @generator.should create('/tmp/app/controllers/john/monkey/stuff.rb')
    end
    
    it "should create a view" do
      @generator.should create('/tmp/app/views/john/monkey/stuff/index.html.erb')
    end

    it "should create a helper" do
      @generator.should create('/tmp/app/helpers/john/monkey/stuff_helper.rb')
    end
    
    describe "with rspec" do
      
      it "should render templates successfully" do
        lambda { @generator.render! }.should_not raise_error
      end

    end

    describe "with test_unit" do

      before do
        @generator = Merb::Generators::ControllerGenerator.new('/tmp', { :testing_framework => :test_unit }, 'John::Monkey::Stuff')
      end
      
      it "should render templates successfully" do
        lambda { @generator.render! }.should_not raise_error
      end

    end
  end
  
end