require File.dirname(__FILE__) + '/spec_helper'

describe Merb::Generators::PartControllerGenerator do

  before(:each) do
    @generator = Merb::Generators::PartControllerGenerator.new('/tmp', {}, 'Stuff')
  end
  
  it_should_behave_like "namespaced generator"
  
  it "should create a controller" do
    @generator.should create('/tmp/app/parts/stuff_part.rb')
  end
  
  it "should create a view" do
    @generator.should create('/tmp/app/parts/views/stuff_part/index.html.erb')
  end
  
  describe "with a namespace" do
    
    before(:each) do
      @generator = Merb::Generators::PartControllerGenerator.new('/tmp', {}, 'John::Monkey::Stuff')
    end
    
    it "should create a controller" do
      @generator.should create('/tmp/app/parts/john/monkey/stuff_part.rb')
    end

    it "should create a view" do
      @generator.should create('/tmp/app/parts/views/john/monkey/stuff_part/index.html.erb')
    end

  end
  
end