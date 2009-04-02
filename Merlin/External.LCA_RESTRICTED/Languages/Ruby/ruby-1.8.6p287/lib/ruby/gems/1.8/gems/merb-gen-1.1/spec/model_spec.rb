require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::ModelGenerator do
  
  it "should complain if no name is specified" do
    lambda {
      @generator = Merb::Generators::ModelGenerator.new('/tmp', {})
    }.should raise_error(::Templater::TooFewArgumentsError)
  end
  
  before do
    @generator = Merb::Generators::ModelGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "namespaced generator"
  
  it "should create a model" do
    @generator.should create('/tmp/app/models/stuff.rb')
  end
  
  describe "with rspec" do
    
    it "should create a model spec" do
      @generator.should create('/tmp/spec/models/stuff_spec.rb')
    end
    
  end
  
  describe "with test_unit" do
    
    it "should create a model test" do
      @generator = Merb::Generators::ModelGenerator.new('/tmp', { :testing_framework => :test_unit }, 'Stuff')
      @generator.should create('/tmp/test/models/stuff_test.rb')
    end
    
  end
  
  describe "with a namespace" do
    
    before(:each) do
      @generator = Merb::Generators::ModelGenerator.new('/tmp', {}, 'John::Monkey::Stuff')
    end
    
    it "should create a model" do
      @generator.should create('/tmp/app/models/john/monkey/stuff.rb')
    end

    describe "with rspec" do

      it "should create a model spec" do
        @generator.should create('/tmp/spec/models/john/monkey/stuff_spec.rb')
      end

    end

    describe "with test_unit" do

      it "should create a model test" do
        @generator = Merb::Generators::ModelGenerator.new('/tmp', { :testing_framework => :test_unit }, 'John::Monkey::Stuff')
        @generator.should create('/tmp/test/models/john/monkey/stuff_test.rb')
      end

    end
  end
  
end